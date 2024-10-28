using AbsoluteCommons.Attributes;
using AbsoluteCommons.Input;
using AbsoluteCommons.Utility;
using System;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace AbsoluteCommons.Components {
	[AddComponentMenu("Camera-Control/CameraFollow")]
	[RequireComponent(typeof(FirstPersonView), typeof(Camera))]
	public class CameraFollow : MonoBehaviour {
		[Header("Camera Properties")]
		public GameObject target;
		[SerializeField] float interpolationStrength = 5;
		[SerializeField] bool instantMovement;
		public bool MovesInstantlyToTarget => instantMovement;

		[SerializeField] float cullingDistance = 0.7f;
		public bool firstPerson;

		[SerializeField, ReadOnly] private bool _isCloseEnoughForFirstPerson;
		public bool FirstPersonRenderingMode {
			get => _isCloseEnoughForFirstPerson;
			private set => _isCloseEnoughForFirstPerson = value;
		}

		[Header("Transition Properties")]
		[SerializeField] float transitionInterpolationStrength = 1;
		[SerializeField] float transitionRotationStrength = 45;
	
		[Header("Camera Offsets")]
		[SerializeField] Vector3 localLookAnchor = Vector3.up * 2;
		[SerializeField] float thirdPersonDistanceFromTarget = 3;
		[SerializeField] float firstPersonCameraHeight = 2;
		[SerializeField] float thirdPersonCameraHeight = 1.5f;
		[SerializeField] float cameraShoulderStride = 1f;

		[Header("Collision")]
		[SerializeField] bool collisionEnabled = true;
		[SerializeField] LayerMask extraLayersToExclude;

		[Header("Private Members")]
		[SerializeField, ReadOnly] private Camera _camera;

		[SerializeField, ReadOnly] private FirstPersonView _view;
		[SerializeField, ReadOnly] private float _transitionTime = 1;
		[SerializeField, ReadOnly] private bool _isTransitioning;
		[SerializeField, ReadOnly] private bool _oldFirstPerson;
		[SerializeField, ReadOnly] private float _cameraShoulder;
		[SerializeField, ReadOnly] private Vector3 _oldPosition;
		[SerializeField, ReadOnly] private Quaternion _oldRotation;

		[SerializeField, ReadOnly] private CameraFollowTargetTransformInterceptor _interceptor;

		void Start() {
			_camera = GetComponent<Camera>();

			_view = GetComponent<FirstPersonView>();

			if (TryGetComponent<Rigidbody>(out var body))
				body.freezeRotation = true;

			_interceptor = GetComponent<CameraFollowTargetTransformInterceptor>();
		}

		private void OnValidate() {
			// Ensure that the fields have sensible values
			cullingDistance = Mathf.Max(0, cullingDistance);
			interpolationStrength = Mathf.Max(0.01f, interpolationStrength);
			transitionInterpolationStrength = Mathf.Max(0.01f, transitionInterpolationStrength);
			transitionRotationStrength = Mathf.Max(0.01f, transitionRotationStrength);
			thirdPersonDistanceFromTarget = Mathf.Max(0, thirdPersonDistanceFromTarget);
		//	localLookAnchor.x = _cameraShoulder;
		}

		void Update() {
			if (!target)
				return;

			CheckTransitionTime();

			CheckCameraMode();

			HidePlayerInTransitionAndFirstPerson();

			_oldPosition = transform.position;
			_oldRotation = transform.rotation;

			UpdatePositionAndRotation();
		}

		private void UpdatePositionAndRotation() {
			if (_isTransitioning)
				HandleTransitionMovement();
			else if (firstPerson)
				HandleFirstPersonMovement();
			else
				HandleThirdPersonMovement();

			HandleCameraRotation();
		}

		void LateUpdate() {
		//	HandleCameraRotation();
		}

		public void RecalculatePositionAndRotation() {
			// Restore the old position and rotation
			transform.SetPositionAndRotation(_oldPosition, _oldRotation);

			// Perform the movement and rotation calculations again
			UpdatePositionAndRotation();
		}

		private void CheckTransitionTime() {
			// If the camera is transitioning, then the camera will move to the target position over time
			if (_isTransitioning) {
				_transitionTime += transitionInterpolationStrength * Time.deltaTime;
				if (_transitionTime >= 1) {
					_isTransitioning = false;
					_transitionTime = 1;
				}
			}
		}

		private void CheckCameraMode() {
			// Toggle first person mode
			if (InputMapper.IsTriggered(KeyCode.F))
				firstPerson = !firstPerson;

			// Handle starting the transition when changing modes
			if (firstPerson) {
				if (!_oldFirstPerson) {
					_oldFirstPerson = true;
					_isTransitioning = true;
					_transitionTime = 1 - _transitionTime;
				}
			} else {
				if (_oldFirstPerson) {
					_oldFirstPerson = false;
					_isTransitioning = true;
					_transitionTime = 1 - _transitionTime;
				}
			}
		}

		private void HidePlayerInTransitionAndFirstPerson() {
			// If the camera is in first person, then the player should be hidden when the camera is close enough
			// Otherwise, the player should always be visible
			if (_isTransitioning) {
				Vector3 fpTarget = target.transform.position + Vector3.up * firstPersonCameraHeight;

				if (VectorMath.DistanceSquared(_camera.transform.position, fpTarget) < cullingDistance * cullingDistance)
					_isCloseEnoughForFirstPerson = true;
				else
					_isCloseEnoughForFirstPerson = false;
			} else if (firstPerson)
				_isCloseEnoughForFirstPerson = true;
		}

		private void HandleTransitionMovement() {
			Vector3 start, end;

			if (firstPerson) {
				start = GetThirdPersonTarget(true);
				end = GetFirstPersonTarget();
			} else {
				start = GetFirstPersonTarget();
				end = GetThirdPersonTarget(true);
			}

			transform.position = Vector3.Lerp(start, end, Easing.InOutCubic(_transitionTime));

			if (CameraTransforms.DisplayCameraLines)
				Debug.DrawLine(start, end, Color.magenta);
		}

		private void HandleFirstPersonMovement() {
			// Shoulder offset is not used in first person
			_cameraShoulder = 0;

		//	localLookAnchor.x = 0;

			transform.position = GetFirstPersonTarget();
		}

		public Vector3 GetFirstPersonTarget() {
			if (!target)
				return transform.position;

			// The camera will attempt to move to a position relative to the target using the camera height
			Vector3 targetPositionRelative = Vector3.up * firstPersonCameraHeight;

			// Attempt to move the camera to the new position
			return target.transform.position + targetPositionRelative;
		}

		private void HandleThirdPersonMovement() {
			HandleThirdPersonMovement_UpdateCameraShoulder();

			Vector3 targetPosition = GetThirdPersonTarget(instantMovement);
			transform.position = targetPosition;

			if (!instantMovement && CameraTransforms.DisplayCameraLines)
				Debug.DrawLine(transform.position, targetPosition, Color.magenta);
		}

		private void HandleThirdPersonMovement_UpdateCameraShoulder() {
			// When rotating horizontally, adjust the camera horizontally and rotate the Z-axis slightly
			// Otherwise, make it return to center
			float step = cameraShoulderStride * 2.75f * Time.deltaTime;
			int horizontal = _view.RotationDirection.Horizontal;
			if (horizontal != 0) {
				// If the direction was changed, then influence the camera toward that direction
				int sign = Math.Sign(_cameraShoulder);
				if (sign != 0 && sign != horizontal)
					_cameraShoulder *= 0.85f;

				_cameraShoulder = Mathf.Clamp(_cameraShoulder + horizontal * step, -cameraShoulderStride, cameraShoulderStride);
			} else if (_cameraShoulder != 0) {
				if (Mathf.Abs(_cameraShoulder) <= step)
					_cameraShoulder = 0;
				else {
					// Attempt a step.  If the camera went past the center, force it to the center
					int sign = Math.Sign(_cameraShoulder);
					_cameraShoulder = Mathf.Clamp(_cameraShoulder * 0.85f - sign * step, -cameraShoulderStride, cameraShoulderStride);

					if (Math.Sign(_cameraShoulder) != sign)
						_cameraShoulder = 0;
				}
			}

			// Update the Z-axis rotation of the camera
			float horizontalFactor = _cameraShoulder / cameraShoulderStride;
			Vector3 rotation = _view.ViewRotation;
			float verticalFactor = AngleMath.AngleToScale(rotation.x, _view.minimumY, _view.maximumY);
		
			Vector3 euler = transform.rotation.eulerAngles;
			euler.z = horizontalFactor * Easing.InCubic(1 - verticalFactor) * -3;

			transform.rotation = Quaternion.Euler(euler);
		
		//	localLookAnchor.x = _cameraShoulder;
		}

		public Vector3 GetThirdPersonTarget(bool useInstantMovement) {
			// The camera will attempt to move to a position relative to the target using the offset
			// and will look at the target with an additional Y-coordinate offset
			Quaternion rotation = Quaternion.Euler(_view.ViewRotation);

			Vector3 targetPositionRelative = rotation * Vector3.up * thirdPersonCameraHeight - rotation * Vector3.forward * thirdPersonDistanceFromTarget;

			// Attempt to move the camera to the new position
			Vector3 targetPosition = target.transform.position + targetPositionRelative;

			// If the current position is close enough to the target position, then the camera will move to the target position immediately
			const float CAMERA_EPSILON = 0.5f;
			if (!useInstantMovement && VectorMath.DistanceSquared(transform.position, targetPosition) < CAMERA_EPSILON * CAMERA_EPSILON)
				useInstantMovement = true;

			targetPosition = GetThirdPersonTarget_CameraAdjustments(ref targetPosition, useInstantMovement);

			return targetPosition;
		}

		private Vector3 GetThirdPersonTarget_CameraAdjustments(ref Vector3 targetPosition, bool instant) {
			// Apply the camera shoulder
			targetPosition += Vector3.right * _cameraShoulder;

			Vector3 lookAnchor = GetLookAnchor();
			float desiredSqrDistance = VectorMath.DistanceSquared(targetPosition, lookAnchor);

			// If collision is enabled, then the camera will attempt to avoid clipping through the environment
			bool collided = false;
			if (collisionEnabled)
				collided = Occlude(ref targetPosition);

			// If the camera is set to move instantly, then the camera will move to the target position immediately
			// Otherwise, the camera will move to the target position over time
			if (!instant)
				targetPosition = Vector3.Slerp(transform.position, targetPosition, interpolationStrength * Time.deltaTime);

			// Attempt to restrict the distance to the look anchor
			// If collision caused the camera to move, then the restriction is ignored to prevent the camera from getting stuck at a weird angle
			if (!collided && RestrictDistanceToLookAnchor(ref targetPosition, lookAnchor, desiredSqrDistance)) {
				// Occlude the new position if the camera was adjusted
				if (collisionEnabled)
					Occlude(ref targetPosition);
			}

			return targetPosition;
		}

		private void HandleCameraRotation() {
			// Rotate toward the view rotation
			Quaternion target;
			if (firstPerson)
				target = Quaternion.Euler(_view.ViewRotation);
			else
				target = GetThirdPersonLookRotation();

			float z = transform.rotation.eulerAngles.z;

			if (_isTransitioning)
				transform.rotation = Quaternion.Lerp(transform.rotation, target, transitionRotationStrength * Time.deltaTime);
			else
				transform.rotation = target;

			// Ensure that the camera doesn't rotate around the Z-axis
			Vector3 euler = transform.rotation.eulerAngles;
			euler.z = z;
			transform.rotation = Quaternion.Euler(euler);

			// Rotate the target to face the camera
			HandleCameraRotation_RotateTarget();
		}

		public Quaternion GetFirstPersonLookRotation() => Quaternion.Euler(_view.ViewRotation);

		public Quaternion GetThirdPersonLookRotation() {
			// Raycast from the look anchor in the direction of the view rotation
			// If the ray hits anything, then the rotation will be adjusted to face the hit point
			// Otherwise, the rotation will be adjusted to face the view rotation
			Quaternion rotation;
			if (instantMovement)
				rotation = Quaternion.Euler(_view.ViewRotation);
			else { 
				// The camera may not be at the target position yet.  Make it look at a point in the intended direction
				Vector3 targetPosition = GetThirdPersonTarget(true);
				Vector3 point = targetPosition + Quaternion.Euler(_view.ViewRotation) * Vector3.forward * 100;

				rotation = RotationMath.RotationTo(transform.position, point);
			}

			Ray ray = new Ray(GetLookAnchor(), rotation * Vector3.forward);

			if (Physics.Raycast(ray, out var hit, 100, target.layer.ToLayerMask().Exclusion(), QueryTriggerInteraction.Ignore)) {
				// The raycast hit something.  Make the rotation face the hit point
				return RotationMath.RotationTo(transform.position, hit.point);
			}

			// The raycast didn't hit anything.  Make the rotation face the view rotation
			return rotation;
		}

		private void HandleCameraRotation_RotateTarget() {
			bool rayHasHit = false;
			if (_view.IsLocked) {
				// Raycast from the camera and rotate the player to face the hit point
				Ray ray = new Ray(_camera.transform.position, Quaternion.Euler(_view.ViewRotation) * Vector3.forward);
		
				// If the ray hits any object, rotate the player to face the hit point
				// The current object should not be included in the raycast
				if (CheckCameraRaycast(out var hit)) {
					// If the hit point is closer than the look anchor, then don't rotate the player
					if (VectorMath.DistanceSquared(target.transform.position, hit.point) >= localLookAnchor.sqrMagnitude) {
						Quaternion rotation = RotationMath.RotationTo(target.transform.position, hit.point);

						SetTargetRotation(rotation, target);

						rayHasHit = true;

						// Draw a line from the camera to the hit point
						if (CameraTransforms.DisplayCameraLines)
							Debug.DrawRay(ray.origin, ray.direction * 5, Color.yellow);
					}
				}
			}

			if (!rayHasHit) {
				// Rotate the player to face the rotation of the camera
				Quaternion rotation = Quaternion.Euler(_view.ViewRotation);

				SetTargetRotation(rotation, target);

				// Draw a line from the camera with the view rotation
				if (CameraTransforms.DisplayCameraLines)
					Debug.DrawRay(_camera.transform.position, _camera.transform.forward * 5, Color.red);
			}

			// Draw a line from the player to the player's forward vector
			if (CameraTransforms.DisplayCameraLines)
				Debug.DrawRay(target.transform.position + target.transform.up * (firstPerson ? firstPersonCameraHeight : thirdPersonCameraHeight), target.transform.forward * 5, Color.blue);
		}

		public void SetTargetRotation(Quaternion rotation, GameObject obj) {
			if (!obj)
				return;

			if (_interceptor)
				_interceptor.AdjustTransform(obj.transform, rotation);
			else
				obj.transform.rotation = rotation;
		}

		public bool CheckCameraRaycast(out RaycastHit hit, float distance = 100) {
			if (!target) {
				hit = default;
				return false;
			}

			Ray ray = new Ray(_camera.transform.position, Quaternion.Euler(_view.ViewRotation) * Vector3.forward);
			return Physics.Raycast(ray, out hit, distance, target.layer.ToLayerMask().Exclusion(), QueryTriggerInteraction.Ignore);
		}

		private Vector3 GetLookAnchor() {
			Quaternion rotation = Quaternion.Euler(_view.ViewRotation);
			return target.transform.position + rotation * Vector3.right * localLookAnchor.x + /* rotation * */ Vector3.up * localLookAnchor.y + rotation * Vector3.forward * localLookAnchor.z;
		}

		private bool Occlude(ref Vector3 targetPosition) {
			// Always exclude the player from the raycast
			int occlusion = target.layer.ToLayerMask().Combine(extraLayersToExclude).Exclusion();

			// Perform a box raycast to ensure the camera doesn't clip through the environment
			// A box raycast is used instead of a linear raycast to ensure that the wall offset is taken into account
			Vector3 raycastStart = GetLookAnchor();
			Vector3 castDirection = VectorMath.DirectionTo(raycastStart, targetPosition);
			const float BOX_SIZE = 0.5f;

			if (Physics.SphereCast(raycastStart, BOX_SIZE, castDirection, out RaycastHit hit, Vector3.Distance(raycastStart, targetPosition), occlusion, QueryTriggerInteraction.Ignore)) {
				targetPosition = hit.point;

				// Move the camera slightly away from the wall
				targetPosition += hit.normal * BOX_SIZE;

				return true;
			}

			return false;
		}

		private bool RestrictDistanceToLookAnchor(ref Vector3 targetPosition, Vector3 anchor, float desiredSqrDistance) {
			// Adjust the target position to be a constant distance from the anchor
			if (VectorMath.DistanceSquared(targetPosition, anchor) != desiredSqrDistance) {
				targetPosition = anchor + VectorMath.DirectionTo(anchor, targetPosition) * Mathf.Sqrt(desiredSqrDistance);
				return true;
			}

			return false;
		}
	}
}