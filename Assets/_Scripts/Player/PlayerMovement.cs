using AbsoluteCommons.Attributes;
using AbsoluteCommons.Components;
using AbsoluteCommons.PhysicsMath;
using AbsoluteCommons.Utility;
using Unity.Netcode;
using UnityEngine;

namespace FluidSimulator.Player {
	[AddComponentMenu("Player/Player Movement")]
	[RequireComponent(typeof(CharacterController))]
	public class PlayerMovement : NetworkBehaviour {
		[SerializeField, ReadOnly] private Vector3 _velocity;
		[SerializeField, ReadOnly] private float _velocityMagnitude;
		[SerializeField, ReadOnly] private bool _isGrounded;
		private bool _oldGrounded;
		[SerializeField, ReadOnly] private CollisionFlags _collisionFlags;
		[SerializeField, ReadOnly] private Vector3 _gravity;
		private Vector3 _oldGravity;
		private bool _hasOldGravity;
		[SerializeField] private float _jumpStrength = 6.8f;
		[SerializeField] private float _maxVelocity = 5.5f;
		[SerializeField] private float _sprintingMaxVelocity = 11f;
		[SerializeField] private float _maxFallVelocity = 25f;
		[SerializeField] private float _acceleration = 115f;
		[SerializeField] private float _sprintingAcceleration = 250f;
		[SerializeField] private float _friction = 13f;
		[SerializeField] private float _airFriction = 7f;
		// Extra fields for accurate networking
		private Vector3 _horizontalVelocity;
		private Vector3 _actualMovement;

		[Header("Controls")]
		public bool canJump = true;
		public bool canMoveHorizontally = true;
		public bool zeroVelocity = false;

		private CharacterController _controller;
	//	private Animator _firstPersonAnimator;
	//	private Animator _thirdPersonAnimator;

		private CameraFollow _camera;

		private void Awake() {
			_controller = GetComponent<CharacterController>();

			// NOTE: the child paths may need to be changed if this script is used in a different project
		//	_firstPersonAnimator = transform.gameObject.GetChildComponent<Animator>("Animator/Y Bot Arms");
		//	_thirdPersonAnimator = transform.gameObject.GetChildComponent<Animator>("Animator/Y Bot");

			_clientState = new NetworkVariable<NetworkState>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

			_camera = Camera.main.GetComponent<CameraFollow>();
		}

		public override void OnNetworkSpawn() {
			if (base.IsOwner) {
				// Set the camera's target to the player
				_camera.target = gameObject;
				tag = "Player";
				Debug.Log("[PlayerMovement] Setting camera target to local player");
			} else {
				tag = "Other Players";
			}
		}

		private void Update() {
			if (IsOwner) {
				UpdateClientState();
				TransmitClientState();
			} else
				ConsumeClientState();

			// Move the player
			_collisionFlags = _controller.Move(_actualMovement);
			_oldGrounded = _isGrounded;

			if (_camera.target == gameObject)
				_camera.RecalculatePositionAndRotation();
		}
		
		private void UpdateClientState() {
			_isGrounded = _controller.isGrounded;

			// Handle grounded interaction
			if (_isGrounded && _velocity.y < 0) {
				_velocity.y = 0;

				// Set the triggers in the animator
				/*
				_thirdPersonAnimator.SetBoolSafely("jumping", false);
				if (!_oldGrounded && _thirdPersonAnimator.GetBool("falling"))
					_thirdPersonAnimator.SetBoolSafely("landing", true);
				*/
			}

			// Handle sprinting
			bool sprinting = ClientInput.IsPressed("Sprint");
		//	_thirdPersonAnimator.SetBool("sprinting", sprinting);

			// Get a rotation to be applied to directions
			Vector3 euler = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
			Vector3 forward = Quaternion.Euler(euler) * Vector3.forward;
			Quaternion movementRotation = ArbitraryGravity.GetRotation(_gravity, forward);

			// Get horizontal movement
			if (!zeroVelocity) {
				if (canMoveHorizontally) {
					Vector3 move = new Vector3(ClientInput.GetRaw("Horizontal"), 0, ClientInput.GetRaw("Vertical"));

					// Ensure that the movement vector can't be longer than 1
					// This is to prevent faster diagonal movement
					if (move.sqrMagnitude > 1)
						move.Normalize();

					move = movementRotation * move;
					move *= (sprinting ? _sprintingAcceleration : _acceleration) * Time.deltaTime;

					_velocity += move;
				}
			} else {
				_velocity.x = 0;
				_velocity.z = 0;
				zeroVelocity = false;
			}

			// Apply friction
			float frictionForce = (_isGrounded ? _friction : _airFriction) * Time.deltaTime;
			Vector3 planar = _velocity.PerpendicularTo(_gravity);

			_velocity -= frictionForce * planar;

			_velocity = _velocity
				.WithPerpendicularDeadZone(_gravity, 0.1f)
				.WithPerpendicularSpeedCap(_gravity, sprinting ? _sprintingMaxVelocity : _maxVelocity);

			Vector3 horizontalVelocity = _velocity.PerpendicularTo(_gravity);
			_horizontalVelocity = horizontalVelocity;
			_velocityMagnitude = horizontalVelocity.magnitude;

		//	_thirdPersonAnimator.SetBool("hasHorizontalMovement", _velocityMagnitude > 0.1f);

			Vector3 unitHorizontal = horizontalVelocity.normalized;

			float forwardDot = Vector3.Dot(unitHorizontal, movementRotation * Vector3.forward);
			if (Mathf.Abs(forwardDot) < 0.01f)
				forwardDot = 0;

		//	_thirdPersonAnimator.SetFloat("forwardVelocity", forwardDot);

			float rightDot = Vector3.Dot(unitHorizontal, movementRotation * Vector3.right);
			if (Mathf.Abs(rightDot) < 0.01f)
				rightDot = 0;

		//	_thirdPersonAnimator.SetFloat("strafeVelocity", rightDot);

			bool canTaunt = _isGrounded /* && !_thirdPersonAnimator.GetBool("landing") */;

			// Handle jump
			if (canJump && _isGrounded && ClientInput.IsTriggered("Jump")) {
				_velocity += _jumpStrength * ArbitraryGravity.Up(_gravity);

				// Set the triggers in the animator
			//	_thirdPersonAnimator.SetBool("jumping", true);

				canTaunt = false;
			}

			_gravity = Physics.gravity * Time.deltaTime;

			// Player is "falling" if the dot product of the velocity and gravity is positive (vectors are in the same direction)
			Vector3 projection = _velocity.ParallelTo(_gravity);
			if (!_isGrounded && _velocity.IsFallingToward(_gravity) && (!_hasOldGravity || projection.sqrMagnitude > _oldGravity.sqrMagnitude)) {
				// Set the triggers in the animator
			//	_thirdPersonAnimator.SetBool("falling", true);

				canTaunt = false;
			} else {
				// Set the triggers in the animator
			//	_thirdPersonAnimator.SetBool("falling", false);
			}

			/*
			if (_firstPersonAnimator)
				_firstPersonAnimator.SetBool("canTaunt", canTaunt);

			_thirdPersonAnimator.SetBool("canTaunt", canTaunt);
			*/

			// Apply gravity
			_velocity += _gravity;
			_oldGravity = _gravity;
			_hasOldGravity = true;

			/*
			if (ArbitraryGravity.RestrictTerminalVelocity(ref _velocity, _gravity, _maxFallVelocity)) {
				// Set the triggers in the animator
				_thirdPersonAnimator.SetBool("longFall", true);
			}
			*/

			// Move the player
			_actualMovement = _velocity * Time.deltaTime;
		//	_collisionFlags = _controller.Move(_actualMovement);
		}

		private NetworkVariable<NetworkState> _clientState;

		private void TransmitClientState() {
			NetworkState state = NetworkState.CopyState(this);

			if (IsServer)
				_clientState.Value = state;
			else
				TransmiteStateServerRpc(state);
		}

		[ServerRpc]
		private void TransmiteStateServerRpc(NetworkState state) {
			_clientState.Value = state;
		}

		private void ConsumeClientState() {
			NetworkState state = _clientState.Value;

			_velocity = state._velocity;
		//	_collisionFlags = state._collisionFlags;
			_gravity = state._gravity;
			_isGrounded = state._isGrounded;
			canJump = state.canJump;
			canMoveHorizontally = state.canMoveHorizontally;
			zeroVelocity = state.zeroVelocity;
			_horizontalVelocity = state._horizontalVelocity;
			_actualMovement = state._actualMovement;
		}

		private struct NetworkState : INetworkSerializable {
			public Vector3 _velocity;
		//	public CollisionFlags _collisionFlags;
			public Vector3 _gravity;
			public bool _isGrounded;
			public bool canJump;
			public bool canMoveHorizontally;
			public bool zeroVelocity;
			public Vector3 _horizontalVelocity;
			public Vector3 _actualMovement;

			public static NetworkState CopyState(PlayerMovement self) {
				return new NetworkState() {
					_velocity = self._velocity,
				//	_collisionFlags = self._collisionFlags,
					_gravity = self._gravity,
					_isGrounded = self._isGrounded,
					canJump = self.canJump,
					canMoveHorizontally = self.canMoveHorizontally,
					zeroVelocity = self.zeroVelocity,
					_horizontalVelocity = self._horizontalVelocity,
					_actualMovement = self._actualMovement
				};
			}

			void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer) {
				if (serializer.IsWriter) {
					var writer = serializer.GetFastBufferWriter();

					writer.WriteValueSafe(_velocity);
				//	writer.WriteValueSafe(_collisionFlags);
					writer.WriteValueSafe(_gravity);
					writer.WriteValueSafe(_horizontalVelocity);
					writer.WriteValueSafe(_actualMovement);

					using (BitWriter bitWriter = writer.EnterBitwiseContext()) {
						bitWriter.TryBeginWriteBits(4);

						bitWriter.WriteBit(_isGrounded);
						bitWriter.WriteBit(canJump);
						bitWriter.WriteBit(canMoveHorizontally);
						bitWriter.WriteBit(zeroVelocity);
					}
				} else {
					var reader = serializer.GetFastBufferReader();

					reader.ReadValueSafe(out _velocity);
				//	reader.ReadValueSafe(out _collisionFlags);
					reader.ReadValueSafe(out _gravity);
					reader.ReadValueSafe(out _horizontalVelocity);
					reader.ReadValueSafe(out _actualMovement);

					using (BitReader bitReader = reader.EnterBitwiseContext()) {
						bitReader.TryBeginReadBits(4);

						bitReader.ReadBit(out _isGrounded);
						bitReader.ReadBit(out canJump);
						bitReader.ReadBit(out canMoveHorizontally);
						bitReader.ReadBit(out zeroVelocity);
					}
				}
			}
		}

		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
			if (serializer.IsWriter) {
				var writer = serializer.GetFastBufferWriter();
				writer.WriteValueSafe(_velocity);
				writer.WriteValueSafe(_velocityMagnitude);
				writer.WriteValueSafe(_collisionFlags);
				writer.WriteValueSafe(_gravity);
				writer.WriteValueSafe(_maxFallVelocity);
				writer.WriteValueSafe(_jumpStrength);
				writer.WriteValueSafe(_maxVelocity);
				writer.WriteValueSafe(_sprintingMaxVelocity);
				writer.WriteValueSafe(_acceleration);
				writer.WriteValueSafe(_sprintingAcceleration);
				writer.WriteValueSafe(_friction);
				writer.WriteValueSafe(_airFriction);
				writer.WriteValueSafe(_horizontalVelocity);
				writer.WriteValueSafe(_actualMovement);

				using (BitWriter bitWriter = writer.EnterBitwiseContext()) {
					bitWriter.TryBeginWriteBits(6);

					bitWriter.WriteBit(_isGrounded);
					bitWriter.WriteBit(_oldGrounded);
					bitWriter.WriteBit(_hasOldGravity);
					bitWriter.WriteBit(canJump);
					bitWriter.WriteBit(canMoveHorizontally);
					bitWriter.WriteBit(zeroVelocity);
				}
			} else {
				var reader = serializer.GetFastBufferReader();
				reader.ReadValueSafe(out _velocity);
				reader.ReadValueSafe(out _velocityMagnitude);
				reader.ReadValueSafe(out _collisionFlags);
				reader.ReadValueSafe(out _gravity);
				reader.ReadValueSafe(out _maxFallVelocity);
				reader.ReadValueSafe(out _jumpStrength);
				reader.ReadValueSafe(out _maxVelocity);
				reader.ReadValueSafe(out _sprintingMaxVelocity);
				reader.ReadValueSafe(out _acceleration);
				reader.ReadValueSafe(out _sprintingAcceleration);
				reader.ReadValueSafe(out _friction);
				reader.ReadValueSafe(out _airFriction);
				reader.ReadValueSafe(out _horizontalVelocity);
				reader.ReadValueSafe(out _actualMovement);

				using (BitReader bitReader = reader.EnterBitwiseContext()) {
					bitReader.TryBeginReadBits(6);

					bitReader.ReadBit(out _isGrounded);
					bitReader.ReadBit(out _oldGrounded);
					bitReader.ReadBit(out _hasOldGravity);
					bitReader.ReadBit(out canJump);
					bitReader.ReadBit(out canMoveHorizontally);
					bitReader.ReadBit(out zeroVelocity);
				}
			}

			base.OnSynchronize(ref serializer);
		}
	}
}
