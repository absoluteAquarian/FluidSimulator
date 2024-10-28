using AbsoluteCommons.Attributes;
using AbsoluteCommons.Components;
using AbsoluteCommons.Utility;
using UnityEngine;

namespace FluidSimulator.Components {
	[RequireComponent(typeof(Camera))]
	public class DispenserViewTracker : MonoBehaviour {
		private Camera _camera;
		private FirstPersonView _componentFPV;
		private CameraFollow _componentCF;
		private CameraFollowTargetTransformInterceptor _componentCFTTI;

		private float _fov;

		[SerializeField, ReadOnly] private GameObject _dispenserTarget;
		[SerializeField, ReadOnly] private Camera _dispenserCamera;

		private void Awake() {
			_camera = GetComponent<Camera>();
			_componentFPV = GetComponent<FirstPersonView>();
			_componentCF = GetComponent<CameraFollow>();
			_componentCFTTI = GetComponent<CameraFollowTargetTransformInterceptor>();
		}

		private void Update() {
			if (_dispenserCamera) {
				// Mimic the dispenser camera
				_camera.transform.SetPositionAndRotation(_dispenserCamera.transform.position, _dispenserCamera.transform.rotation);
			}
		}

		public bool IsViewingDispenser(GameObject obj) => _dispenserTarget.IsObjectOrParentOfObject(obj);

		public void SwitchToDispenser(GameObject dispenser) {
			if (dispenser == null)
				return;  // Invalid target, do nothing

			if (_dispenserTarget == dispenser)
				return;  // Already looking at the target, do nothing

			_dispenserTarget = dispenser;

			// Disable the FPS camera
		//	if (_componentFPV)
		//		_componentFPV.ForceLock();
			if (_componentCF)
				_componentCF.enabled = false;
			if (_componentCFTTI)
				_componentCFTTI.enabled = false;

			// Go to the dispenser camera
			_dispenserCamera = _dispenserTarget.GetComponentInChildren<Camera>();

			// Save the current camera metrics
			_fov = _camera.fieldOfView;

			// Set the camera metrics
			_camera.fieldOfView = _dispenserCamera.fieldOfView;
		}

		public void ExitDispenserCamera() {
			if (!_dispenserTarget)
				return;  // No target, do nothing

			_dispenserTarget = null;

			// Enable the FPS camera
		//	if (_componentFPV)
		//		_componentFPV.ReleaseForcedLock();
			if (_componentCF)
				_componentCF.enabled = true;
			if (_componentCFTTI)
				_componentCFTTI.enabled = true;

			// Go back to the FPS camera
			_dispenserCamera = null;

			// Restore the camera metrics
			_camera.fieldOfView = _fov;
		}
	}
}
