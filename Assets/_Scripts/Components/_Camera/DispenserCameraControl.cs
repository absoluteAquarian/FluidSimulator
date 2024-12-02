using AbsoluteCommons.Components;
using Unity.Netcode;
using UnityEngine;

namespace FluidSimulator.Components {
	[RequireComponent(typeof(Camera))]
	public class DispenserCameraControl : NetworkBehaviour, IDispenserObject {
		private FirstPersonView _componentFPV;
		private DispenserViewTracker _componentDVT;

		private FirstPersonView.Snapshot _snapshot;

		public FirstPersonView.Axes axes = FirstPersonView.Axes.MouseXAndY;
		public float sensitivityX = 8f;
		public float sensitivityY = 8f;

		public float minimumY = -45f;  // Vertical rotation
		public float maximumY = 45f;

		private void Awake() {
			_componentFPV = Camera.main.GetComponent<FirstPersonView>();
			_componentDVT = Camera.main.GetComponent<DispenserViewTracker>();
		}

		private void Update() {
			if (_componentFPV && _componentDVT && _componentDVT.IsViewingDispenser(gameObject)) {
				// Update the rotation of the dispenser spout and camera
				Quaternion target = Quaternion.Euler(_componentFPV.ViewRotation);

				// Ensure that the Z-axis is not rotated for both objects
				AdjustTransform(transform.parent, target);

				if (base.IsOwner)
					AdjustTransform_Impl(Camera.main.transform, target);
			}
		}

		private void AdjustTransform(Transform transform, Quaternion target) {
			if (base.IsServer)
				AdjustTransform_Impl(transform, target);
			else
				AdjustTransformServerRpc(transform.gameObject, target);
		}

		[ServerRpc(RequireOwnership = false)]
		private void AdjustTransformServerRpc(NetworkObjectReference objRef, Quaternion target) {
			AdjustTransformClientRpc(objRef, target);
		}
		
		[ClientRpc]
		private void AdjustTransformClientRpc(NetworkObjectReference objRef, Quaternion target) {
			NetworkObject obj = objRef;
			if (!obj)
				return;

			AdjustTransform_Impl(obj.transform, target);
		}

		private static void AdjustTransform_Impl(Transform transform, Quaternion target) {
			// Ensure that the Z-axis is not rotated
			float z = transform.eulerAngles.z;
			transform.rotation = target;
			Vector3 euler = transform.eulerAngles;
			euler.z = z;
			transform.eulerAngles = euler;
		}

		public void OnDispenserViewEntered() {
			if (_componentFPV) {
				_snapshot = _componentFPV.CreateSnapshot();
				_componentFPV.ClearRotations();

				// Force the controls to what this component wants
				_componentFPV.axes = axes;
				_componentFPV.sensitivityX = sensitivityX;
				_componentFPV.sensitivityY = sensitivityY;
				_componentFPV.flipVerticalMovement = true;
				_componentFPV.minimumY = minimumY;
				_componentFPV.maximumY = maximumY;
			}
		}

		public void OnDispenserViewExited() {
			if (_componentFPV) {
				// Restore the controls to what they were before
				_componentFPV.RestoreFromSnapshot(_snapshot);
				_snapshot = default;
			}
		}
	}
}
