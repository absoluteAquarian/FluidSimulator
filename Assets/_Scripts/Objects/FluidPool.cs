using AbsoluteCommons.Components;
using AbsoluteCommons.Objects;
using AbsoluteCommons.Utility;
using FluidSimulator.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Jobs;

namespace FluidSimulator.Objects {
	public class FluidPool : NetworkBehaviour {
		[SerializeField] private FluidDatabase database;

		[Header("Primary Fluid")]
		[SerializeField, AbsoluteCommons.Attributes.ReadOnly] private DynamicObjectPool _fluidCreatorPrimary;
		[SerializeField] private FluidID _primaryFluid;
		[SerializeField] private GameObject _dispenserPrimary;
		[SerializeField, AbsoluteCommons.Attributes.ReadOnly] private TimersTracker _dispenserPrimaryTimers;

		[Header("Secondary Fluid")]
		[SerializeField, AbsoluteCommons.Attributes.ReadOnly] private DynamicObjectPool _fluidCreatorSecondary;
		[SerializeField] private FluidID _secondaryFluid;
		[SerializeField] private GameObject _dispenserSecondary;
		[SerializeField, AbsoluteCommons.Attributes.ReadOnly] private TimersTracker _dispenserSecondaryTimers;

		private void Awake() {
			if (!_dispenserPrimary) {
				Debug.LogError("[FluidPool] [Awake] Primary dispenser does not exist");
				goto FindSecondaryInfo;
			}

			GameObject firstFluid = _dispenserPrimary.GetChild("Pool");
			if (firstFluid) {
				_fluidCreatorPrimary = firstFluid.GetComponent<DynamicObjectPool>();
				if (!_fluidCreatorPrimary) {
					Debug.LogError("[FluidPool] [Awake] Primary dispenser DynamicObjectPool does not exist");
					goto FindPrimaryTimers;
				}

				_fluidCreatorPrimary.SetPrefab(database.GetFluid(_primaryFluid));

			FindPrimaryTimers:
				_dispenserPrimaryTimers = _dispenserPrimary.GetComponent<TimersTracker>();
				if (!_dispenserPrimaryTimers)
					Debug.LogError("[FluidPool] [Awake] Primary dispenser TimersTracker does not exist");
			}

		FindSecondaryInfo:
			if (!_dispenserSecondary) {
				Debug.LogError("[FluidPool] [Awake] Secondary dispenser does not exist");
				return;
			}

			GameObject secondFluid = _dispenserSecondary.GetChild("Pool");
			if (secondFluid) {
				_fluidCreatorSecondary = secondFluid.GetComponent<DynamicObjectPool>();
				if (!_fluidCreatorSecondary) {
					Debug.LogError("[FluidPool] [Awake] Secondary dispenser DynamicObjectPool does not exist");
					goto FindSecondaryTimers;
				}

				_fluidCreatorSecondary.SetPrefab(database.GetFluid(_secondaryFluid));

			FindSecondaryTimers:
				_dispenserSecondaryTimers = _dispenserSecondary.GetComponent<TimersTracker>();
				if (!_dispenserSecondaryTimers)
					Debug.LogError("[FluidPool] [Awake] Secondary dispenser TimersTracker does not exist");
			}
		}

		private void Update() {
			// Use jobs to simulate spring-like motions (mx" + γx' + kx = 0)
			UpdateJobCache();
			HandleJobs();
		}

		private void LateUpdate() {
			FinishJobs();
		}

		// Job system stuff
		private int _lastPrimaryParticleCount, _lastSecondaryParticleCount;
		private int numParticles = -1;
		private NativeArray<ParticlePhysics> _particles;
		private NativeArray<ParticleForce> _forces;
		private NativeArray<ParticleVelocity> _velocities;
		private NativeArray<ParticlePosition> _positions;
		private NativeArray<PositionCorrection> _corrections;
	//	private NativeArray<bool> _nanCheck;
		private TransformAccessArray _transforms;
		private ContainerBounds _bounds;

		private GenerateForcesJob forceUpdateJob;
		private JobHandle forceUpdateJobHandle;
		private VelocityPositionUpdateJob velPosUpdateJob;
		private JobHandle velPosUpdateJobHandle;
		private ApplyTransformsJob transformsJob;
		private JobHandle transformsJobHandle;

		private void UpdateJobCache() {
			int total = _fluidCreatorPrimary.Count + _fluidCreatorSecondary.Count;
			int prevTotal = _lastPrimaryParticleCount + _lastSecondaryParticleCount;

			if (total > prevTotal) {
				InitOrResizeTransformsArray(total, prevTotal);
				InitOrResizeParticleArrays(total, prevTotal);
				CheckForNewParticles(total, prevTotal);

				_lastPrimaryParticleCount = _fluidCreatorPrimary.Count;
				_lastSecondaryParticleCount = _fluidCreatorSecondary.Count;
			}

			// TODO: Read from the objects in the scene to determine this dynamically
			//       Hardcoding will suffice for now
			const float X_STRIDE = 5f, Z_STRIDE = 20f;
			const float WALL_THICKNESS = 0.1f, WALL_HEIGHT = 15f;

			_bounds = new ContainerBounds() {
				xp = Z_STRIDE / 2 - WALL_THICKNESS / 2,
				xn = -Z_STRIDE / 2 + WALL_THICKNESS / 2,
				zp = X_STRIDE / 2 - WALL_THICKNESS / 2,
				zn = -X_STRIDE / 2 + WALL_THICKNESS / 2,
				ceiling = WALL_HEIGHT / 2,
				floor = -WALL_HEIGHT / 2 - 0.45f
			};
		}

		private void InitOrResizeParticleArrays(int total, int prevTotal) {
			// Update the particle data array
			if (numParticles < 0 || total >= numParticles) {
				int length = numParticles > 0 ? numParticles : 128;
				while (length < total)
					length *= 2;
				
				NativeArray<ParticlePhysics> particles = new NativeArray<ParticlePhysics>(length, Allocator.Persistent);
				NativeArray<ParticleForce> forces = new NativeArray<ParticleForce>(length, Allocator.Persistent);
				NativeArray<ParticleVelocity> velocities = new NativeArray<ParticleVelocity>(length, Allocator.Persistent);
				NativeArray<ParticlePosition> positions = new NativeArray<ParticlePosition>(length, Allocator.Persistent);
				NativeArray<PositionCorrection> corrections = new NativeArray<PositionCorrection>(length, Allocator.Persistent);
				NativeArray<bool> nanCheck = new NativeArray<bool>(length, Allocator.Persistent);

				// Copy over the previous data
				if (_particles.IsCreated)
					_particles.AsSpan().CopyTo(particles.AsSpan());
				if (_forces.IsCreated)
					_forces.AsSpan().CopyTo(forces.AsSpan());
				if (_velocities.IsCreated)
					_velocities.AsSpan().CopyTo(velocities.AsSpan());
				if (_positions.IsCreated)
					_positions.AsSpan().CopyTo(positions.AsSpan());
				if (_corrections.IsCreated)
					_corrections.AsSpan().CopyTo(corrections.AsSpan());
			//	if (_nanCheck.IsCreated)
			//		_nanCheck.AsSpan().CopyTo(nanCheck.AsSpan());

				// Dispose of the old arrays
				if (_particles.IsCreated)
					_particles.Dispose();
				if (_forces.IsCreated)
					_forces.Dispose();
				if (_velocities.IsCreated)
					_velocities.Dispose();
				if (_positions.IsCreated)
					_positions.Dispose();
				if (_corrections.IsCreated)
					_corrections.Dispose();
			//	if (_nanCheck.IsCreated)
			//		_nanCheck.Dispose();

				// Assign the new arrays
				_particles = particles;
				_forces = forces;
				_velocities = velocities;
				_positions = positions;
				_corrections = corrections;
			//	_nanCheck = nanCheck;

				numParticles = length;
			}
		}

		private void InitOrResizeTransformsArray(int total, int prevTotal) {
			// Update the transforms array
			if (!_transforms.isCreated || total >= _transforms.length) {
				int length = _transforms.length > 0 ? _transforms.length : 128;
				while (length < total)
					length *= 2;

				TransformAccessArray transforms = new TransformAccessArray(length);

				// Copy over the previous data
				if (_transforms.isCreated) {
					for (int i = 0; i < _transforms.length; i++)
						transforms.Add(_transforms[i]);
				}

				// Dispose of the old array
				if (_transforms.isCreated)
					_transforms.Dispose();

				// Assign the new array
				_transforms = transforms;
			}
		}

		private void CheckForNewParticles(int total, int prevTotal) {
			// Populate the arrays with the new particles
			foreach (Transform transform in _fluidCreatorPrimary.ExtractObjectTransforms(_lastPrimaryParticleCount))
				_transforms.Add(transform);

			foreach (Transform transform in _fluidCreatorSecondary.ExtractObjectTransforms(_lastSecondaryParticleCount))
				_transforms.Add(transform);

			int nextIndex = prevTotal;

			ParticlePhysics baseParticle = CreateNewParticle(_primaryFluid);
			for (int i = _lastPrimaryParticleCount; i < _fluidCreatorPrimary.Count; i++, nextIndex++) {
				Transform transform = _transforms[nextIndex];

				_particles[nextIndex] = baseParticle;
				_forces[nextIndex] = default;
				_velocities[nextIndex] = transform.gameObject.GetComponent<ParallelPhysics>().velocity;
				_positions[nextIndex] = transform.position;
				_corrections[nextIndex] = default;
			}

			baseParticle = CreateNewParticle(_secondaryFluid);
			for (int i = _lastSecondaryParticleCount; i < _fluidCreatorSecondary.Count; i++, nextIndex++) {
				Transform transform = _transforms[nextIndex];

				_particles[nextIndex] = baseParticle;
				_forces[nextIndex] = default;
				_velocities[nextIndex] = transform.gameObject.GetComponent<ParallelPhysics>().velocity;
				_positions[nextIndex] = transform.position;
				_corrections[nextIndex] = default;
			}
		}

		private ParticlePhysics CreateNewParticle(FluidID id) {
			var prefab = database.GetFluid(id);
			if (!prefab) {
				Debug.LogError($"[FluidPool] [CreateNewParticle] Fluid ID {id} does not exist");
				return default;
			}

			return new ParticlePhysics() {
				mass = prefab.mass,
				radius = prefab.radius
			};
		}

		[BurstCompile]
		private void HandleJobs() {
			if (!_particles.IsCreated || !_transforms.isCreated) {
				// No particles to simulate
				return;
			}

			int total = _fluidCreatorPrimary.Count + _fluidCreatorSecondary.Count;

			forceUpdateJob = new GenerateForcesJob() {
				particles = _particles,
				forces = _forces,
				positions = _positions,
			//	nanCheck = _nanCheck,
				time = Time.time,
				deltaTime = Time.fixedDeltaTime,
				totalParticles = total
			};

			forceUpdateJobHandle = forceUpdateJob.Schedule(total, 64);

			velPosUpdateJob = new VelocityPositionUpdateJob() {
				particles = _particles,
				forces = _forces,
				velocities = _velocities,
				positions = _positions,
				corrections = _corrections,
				bounds = _bounds,
				time = Time.time,
				deltaTime = Time.fixedDeltaTime
			};

			velPosUpdateJobHandle = velPosUpdateJob.Schedule(total, 64, forceUpdateJobHandle);

			transformsJob = new ApplyTransformsJob() {
				particles = _particles,
				positions = _positions,
				corrections = _corrections,
				bounds = _bounds
			};

			transformsJobHandle = transformsJob.Schedule(_transforms, velPosUpdateJobHandle);

			JobHandle.ScheduleBatchedJobs();
		}

		private void FinishJobs() {
			// Wait for the job responsible for updating the transforms to complete
			// The other jobs are prerequisites for this one, so they will be waited on automatically
			transformsJobHandle.Complete();
		}

		private struct ContainerBounds {
			public float xp, xn;
			public float zp, zn;
			public float ceiling;
			public float floor;
		}

		private struct ParticlePhysics {
			public float mass;
			public float radius;
		}

		private struct ParticleForce {
			public Vector3 vector;

			public static implicit operator ParticleForce(Vector3 vector) {
				return new ParticleForce() {
					vector = vector
				};
			}
		}

		private struct ParticleVelocity {
			public Vector3 vector;

			public static implicit operator ParticleVelocity(Vector3 vector) {
				return new ParticleVelocity() {
					vector = vector
				};
			}
		}

		private struct ParticlePosition {
			public Vector3 vector;

			public static implicit operator ParticlePosition(Vector3 vector) {
				return new ParticlePosition() {
					vector = vector
				};
			}
		}

		private struct PositionCorrection {
			public bool xp, xn;
			public bool zp, zn;
			public bool ceiling;
			public bool floor;
		}

		[BurstCompile]
		private struct GenerateForcesJob : IJobParallelFor {
			[ReadOnly] public NativeArray<ParticlePhysics> particles;
			public NativeArray<ParticleForce> forces;
			[ReadOnly] public NativeArray<ParticlePosition> positions;
		//	public NativeArray<bool> nanCheck;

			public float time, deltaTime;
			public float totalParticles;

			void IJobParallelFor.Execute(int index) {
				ParticlePhysics particle = particles[index];

				// Derived from: https://www.youtube.com/watch?v=kyQP4t_wOGI

				Vector3 force = Vector3.zero;

				// Gravity
				force += deltaTime * particle.mass * Physics.gravity * 8f;  // Hack gravity to make it a bit stronger

				/*
				if (float.IsNaN(force.x) || float.IsNaN(force.y) || float.IsNaN(force.z)) {
					Debug.LogError($"[GenerateForcesJob] [Execute] Force was NaN for particle {index} (check 1)");
					nanCheck[index] = true;
				}
				*/

				// Inter-particle forces
				// Particles have a weak attractive force to each other, but a strong repulsive force
				// This is to simulate the surface tension of water
				const float ATTRACTION_COEFFICIENT = 0.4f;
				const float REPULSION_COEFFICIENT = 5.5f;

				Vector3 position = positions[index].vector;

				for (int i = 0; i < totalParticles; i++) {
					if (i == index)
						continue;

					ParticlePhysics other = particles[i];
					Vector3 otherPosition = positions[i].vector;

					float sqDistance = (position - otherPosition).sqrMagnitude;
					float minDistance = 2.0f;
					float maxDistance = 3.0f;

					float superFarDistance = maxDistance * 2f;

					// Particles that are too far away won't affect each other that much, so skip them
					if (sqDistance > superFarDistance * superFarDistance)
						continue;

					float distance = Vector3.Distance(position, otherPosition);

					if (distance == 0)
						continue;

					Vector3 dir = VectorMath.DirectionTo(position, otherPosition);

					float attraction, repulsion;
					if (distance < minDistance) {
						attraction = 0;
						repulsion = REPULSION_COEFFICIENT * (minDistance / distance);
					} else if (distance >= minDistance && distance < maxDistance) {
						float factor = (distance - minDistance) / (maxDistance - minDistance);
						attraction = ATTRACTION_COEFFICIENT * factor;
						repulsion = REPULSION_COEFFICIENT * (1f - factor);
					} else {
						attraction = ATTRACTION_COEFFICIENT * (maxDistance / distance);
						repulsion = 0;
					}

					// Apply the forces
					force += attraction * deltaTime / particle.mass * dir + repulsion * deltaTime / particle.mass * -dir;

					/*
					if (!nanCheck[index] && (float.IsNaN(force.x) || float.IsNaN(force.y) || float.IsNaN(force.z))) {
						Debug.LogError($"[GenerateForcesJob] [Execute] Force was NaN for particle {index} (check 2) after handling other particle {i}");
						nanCheck[index] = true;
					}
					*/
				}

				/*
				if (!nanCheck[index] && (float.IsNaN(force.x) || float.IsNaN(force.y) || float.IsNaN(force.z)))
					Debug.LogError($"[GenerateForcesJob] [Execute] Force was NaN for particle {index} (check 3)");
				*/

				forces[index] = force;
			}
		}

		[BurstCompile]
		private struct VelocityPositionUpdateJob : IJobParallelFor {
			[ReadOnly] public NativeArray<ParticlePhysics> particles;
			[ReadOnly] public NativeArray<ParticleForce> forces;
			public NativeArray<ParticleVelocity> velocities;
			public NativeArray<ParticlePosition> positions;
			public NativeArray<PositionCorrection> corrections;

			public ContainerBounds bounds;
			public float time, deltaTime;

			void IJobParallelFor.Execute(int index) {
				ParticlePhysics particle = particles[index];
				Vector3 velocity = velocities[index].vector;
				PositionCorrection correction = corrections[index];

				// Derived from: https://www.youtube.com/watch?v=kyQP4t_wOGI

				// Update the velocity
				velocity += deltaTime / particle.mass * forces[index].vector;

				// Damping
				const float DAMPING_COEFFICIENT = 0.22f;
				float damping = 1f / (1f + DAMPING_COEFFICIENT * deltaTime);
				
				velocity.x *= damping;
				velocity.z *= damping;

				// Allow gravity to be much stronger than the damping
				if (velocity.y > 0)
					velocity.y *= damping;

				// Keep the particle within the container
				float radius = particle.radius;
				Vector3 position = positions[index].vector;
				Vector3 futurePosition = position + velocity * deltaTime;

				const float REBOUND = 0.4f;

				if (futurePosition.x - radius <= bounds.xn) {
					correction.xn = true;
					velocity.x *= -1;
				} else if (futurePosition.x + radius >= bounds.xp) {
					correction.xp = true;
					velocity.x *= -1;
				}

				if (futurePosition.z - radius <= bounds.zn) {
					correction.zn = true;
					velocity.z *= -1;
				} else if (futurePosition.z + radius >= bounds.zp) {
					correction.zp = true;
					velocity.z *= -1;
				}

				if (futurePosition.y - radius <= bounds.floor) {
					correction.floor = true;
					velocity.y *= -REBOUND;
				} else if (futurePosition.y + radius >= bounds.ceiling) {
					correction.ceiling = true;
					velocity.y *= -1;
				}

				velocities[index] = velocity;

				// Update the position
				positions[index] = position + deltaTime * velocity;
			}
		}

		[BurstCompile]
		private struct ApplyTransformsJob : IJobParallelForTransform {
			[ReadOnly] public NativeArray<ParticlePhysics> particles;
			[ReadOnly] public NativeArray<ParticlePosition> positions;
			public NativeArray<PositionCorrection> corrections;

			public ContainerBounds bounds;

			void IJobParallelForTransform.Execute(int index, TransformAccess transform) {
				Vector3 position = positions[index].vector;
				float radius = particles[index].radius;
				PositionCorrection correction = corrections[index];

				// Force the particle to stay within the container
				if (correction.xn)
					position.x = bounds.xn + radius;
				else if (correction.xp)
					position.x = bounds.xp - radius;

				if (correction.zn)
					position.z = bounds.zn + radius;
				else if (correction.zp)
					position.z = bounds.zp - radius;

				if (correction.floor)
					position.y = bounds.floor + radius;
				else if (correction.ceiling)
					position.y = bounds.ceiling - radius;

				transform.position = position;
				corrections[index] = default;
			}
		}
	}
}
