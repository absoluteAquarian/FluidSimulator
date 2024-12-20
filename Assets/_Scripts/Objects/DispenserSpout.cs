﻿using AbsoluteCommons.Components;
using AbsoluteCommons.Objects;
using AbsoluteCommons.Utility;
using FluidSimulator.Components;
using FluidSimulator.Player;
using Unity.Netcode;
using UnityEngine;

namespace FluidSimulator.Objects {
	public class DispenserSpout : NetworkBehaviour, IDispenserObject {
		[SerializeField] private FluidPool _pool;
		[SerializeField] private int _particlesPerSecond = 15;
		[SerializeField] private int _maxParticlesPerTick = 5;
		[SerializeField] private float _spreadPosition = 0.3f;
		[SerializeField] private float _spreadVelocity = 0.1f;

		private DynamicObjectPool _particleCreator;
		private Camera _localCamera;

		private TimersTracker _timers;
		
		private bool _active;

		private bool _sprayCooldown;
		private bool _spraying;

		private static int resetTimersAction = -1;

		private void Awake() {
			GameObject child = gameObject.GetChild("Pool");
			_particleCreator = child.GetComponent<DynamicObjectPool>();

			child = gameObject.GetChild("Base/Camera");
			_localCamera = child.GetComponent<Camera>();

			_timers = GetComponent<TimersTracker>();

			if (resetTimersAction == -1)
				resetTimersAction = TimersTracker.RegisterCompletionAction(ReleaseConstraints);
		}

		private void Update() {
			// NOTE: TowerDefense had clientside projectile spawning, but FluidSimulator has serverside particle spawning instead.
			if (!_active /* || !base.IsOwner */)
				return;

			if (CanSpawnParticles())
				SpawnParticle();
			else
				_spraying = false;
		}

		private bool CanSpawnParticles() {
			if (_sprayCooldown)
				return false;

			return ClientInput.IsPressed("Interact");
		}

		private void SpawnParticle() {
			_sprayCooldown = true;

			SpawnParticle_HandleNetworking();

			if (!_spraying) {
				// Create a looping timer
				Timer timer = Timer.CreateCountdown(resetTimersAction, 1f / _particlesPerSecond, repeating: true);
				timer.Start();

				_timers.AddTimer(timer);
			}

			_spraying = true;
		}

		private void SpawnParticle_HandleNetworking() {
			// NOTE: TowerDefense had clientside projectile spawning, but FluidSimulator has serverside particle spawning instead.
			/*
			if (!base.IsOwner)
				return;
			*/

			int count = Random.Range(1, _maxParticlesPerTick);

			if (base.IsServer)
				SpawnParticle_ActuallySpawn(CreateSpawnMessage(), count);
			else
				RequestParticleSpawnServerRpc(count);
		}

		[ServerRpc(RequireOwnership = false)]
		private void RequestParticleSpawnServerRpc(int count) {
			SpawnParticle_ActuallySpawn(CreateSpawnMessage(), count);
		}

		// NOTE: TowerDefense had clientside projectile spawning, but FluidSimulator has serverside particle spawning instead.
		/*
		[ClientRpc]
		private void SpawnParticleClientRpc() {
			if (!base.IsOwner)
				SpawnParticle_ActuallySpawn(CreateSpawnMessage());
		}
		*/

		private ParticleSpawnMessage CreateSpawnMessage() => new ParticleSpawnMessage(_localCamera.transform.position, _localCamera.transform.rotation, _localCamera.transform.forward, _spreadPosition, _spreadVelocity);

		private void SpawnParticle_ActuallySpawn(ParticleSpawnMessage msg, int count) {
			for (int i = 0; i < count; i++) {
				GameObject particle = _particleCreator.Get();

				if (particle) {
					// Spawn was successful
					particle.transform.SetPositionAndRotation(msg.GetRandomPosition(), Quaternion.identity);

					if (particle.TryGetComponent(out ParallelPhysics physics))
						physics.velocity = msg.GetRandomVelocity();
				}
			}
		}

		private static void ReleaseConstraints(GameObject obj, Timer timer) {
			DispenserSpout self = obj.GetComponent<DispenserSpout>();

			self._sprayCooldown = false;

			if (self.CanSpawnParticles()) {
				self._spraying = true;
				self.SpawnParticle_HandleNetworking();
				self._sprayCooldown = true;
			} else {
				self._timers.RemoveTimer(timer);
				self._spraying = false;
			}
		}

		public void OnDispenserViewEntered() {
			_active = true;
		}

		public void OnDispenserViewExited() {
			_active = false;
		}

		// Networking stuff
		private struct ParticleSpawnMessage : INetworkSerializable {
			public Vector3 position;
			public Quaternion rotation;
			public Vector3 forward;
			public float spreadPosition;
			public float spreadVelocity;

			public ParticleSpawnMessage(Vector3 position, Quaternion rotation, Vector3 forward, float spreadPosition, float spreadVelocity) {
				this.position = position;
				this.rotation = rotation;
				this.forward = forward;
				this.spreadPosition = spreadPosition;
				this.spreadVelocity = spreadVelocity;
			}

			public Vector3 GetRandomPosition()
				=> position + forward * 0.3f + rotation * new Vector3(Random.Range(-spreadPosition, spreadPosition), Random.Range(-spreadPosition, spreadPosition), Random.Range(0f, spreadPosition + 2f));

			public Vector3 GetRandomVelocity()
				=> forward * 4f + rotation * new Vector3(Random.Range(-spreadVelocity, spreadVelocity), Random.Range(-spreadVelocity, spreadVelocity), Random.Range(0f, spreadVelocity + 2f));

			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
				if (serializer.IsWriter) {
					var writer = serializer.GetFastBufferWriter();

					writer.WriteValueSafe(position);
					writer.WriteValueSafe(rotation);
					writer.WriteValueSafe(forward);
					writer.WriteValueSafe(spreadPosition);
					writer.WriteValueSafe(spreadVelocity);
				} else {
					var reader = serializer.GetFastBufferReader();

					reader.ReadValueSafe(out position);
					reader.ReadValueSafe(out rotation);
					reader.ReadValueSafe(out forward);
					reader.ReadValueSafe(out spreadPosition);
					reader.ReadValueSafe(out spreadVelocity);
				}
			}
		}

		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
			if (serializer.IsWriter) {
				var writer = serializer.GetFastBufferWriter();

				using (var bitWriter = writer.EnterBitwiseContext()) {
					bitWriter.TryBeginWriteBits(3);

					bitWriter.WriteBit(_active);
					bitWriter.WriteBit(_sprayCooldown);
					bitWriter.WriteBit(_spraying);
				}
			} else {
				var reader = serializer.GetFastBufferReader();

				using (var bitReader = reader.EnterBitwiseContext()) {
					bitReader.TryBeginReadBits(3);

					bitReader.ReadBit(out _active);
					bitReader.ReadBit(out _sprayCooldown);
					bitReader.ReadBit(out _spraying);
				}
			}
		}
	}
}
