using AbsoluteCommons.Components;
using Unity.Netcode;
using UnityEngine;

namespace AbsoluteCommons.Networking {
	public class PlayerNetcode : NetworkBehaviour {
		private CameraFollow _camera;

		private NetworkVariable<PlayerCameraState> _cameraState;

		public Quaternion FirstPersonLookRotation => _cameraState.Value.FirstPersonRotation;

		public Quaternion ThirdPersonLookRotation => _cameraState.Value.ThirdPersonRotation;

		public Vector3 FirstPersonCameraTarget => _cameraState.Value.FirstPersonTarget;

		public Vector3 ThirdPersonLookTarget => _cameraState.Value.ThirdPersonLookTarget;

		private void Awake() {
			_camera = Camera.main.GetComponent<CameraFollow>();
			_cameraState = new NetworkVariable<PlayerCameraState>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
		}

		private void Update() {
			if (IsOwner) {
				// Update and transmit the network state
				TransmitState();
			} else
				ConsumeState();
		}

		private void TransmitState() {
			PlayerCameraState state = new PlayerCameraState(_camera);

			if (base.IsServer)
				_cameraState.Value = state;
			else
				TransmitStateServerRpc(state);
		}

		[ServerRpc]
		private void TransmitStateServerRpc(PlayerCameraState state) {
			_cameraState.Value = state;
		}

		private void ConsumeState() {
			_camera.SetTargetRotation(ThirdPersonLookRotation, gameObject);
		}
	}

	public struct PlayerCameraState : INetworkSerializable {
		private Vector3 fpEuler, tpEuler;

		private Vector3 fpTarget;
		private Vector3 tpLookTarget;

		public readonly Quaternion FirstPersonRotation => Quaternion.Euler(fpEuler);

		public readonly Quaternion ThirdPersonRotation => Quaternion.Euler(tpEuler);

		public readonly Vector3 FirstPersonTarget => fpTarget;

		public readonly Vector3 ThirdPersonLookTarget => tpLookTarget;

		public PlayerCameraState(CameraFollow camera) {
			fpEuler = camera.GetFirstPersonLookRotation().eulerAngles;
			tpEuler = camera.GetThirdPersonLookRotation().eulerAngles;
			fpTarget = camera.GetFirstPersonTarget();

			if (camera.CheckCameraRaycast(out var hit, 100f))
				tpLookTarget = hit.point;
			else
				tpLookTarget = camera.transform.position + camera.transform.forward * 100f;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
			if (serializer.IsWriter) {
				var writer = serializer.GetFastBufferWriter();

				writer.WriteValueSafe(fpEuler);
				writer.WriteValueSafe(tpEuler);
				writer.WriteValueSafe(fpTarget);
				writer.WriteValueSafe(tpLookTarget);
			} else {
				var reader = serializer.GetFastBufferReader();

				reader.ReadValueSafe(out fpEuler);
				reader.ReadValueSafe(out tpEuler);
				reader.ReadValueSafe(out fpTarget);
				reader.ReadValueSafe(out tpLookTarget);
			}
		}
	}
}
