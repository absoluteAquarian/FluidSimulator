using UnityEngine;

namespace AbsoluteCommons.Components {
	[AddComponentMenu("Camera-Control/CameraLookTransformInterceptor")]
	[RequireComponent(typeof(CameraFollow))]
	public class CameraFollowTargetTransformInterceptor : MonoBehaviour {
		[SerializeField] private bool lockXAxis = false;
		[SerializeField] private bool lockYAxis = false;
		[SerializeField] private bool lockZAxis = false;

		public bool ForcedLock { get; private set; }

		// Fields for lerping to the target rotation
		private bool lerpToTargetRotation = false;
		[SerializeField] private float lerpSpeed = 3f;
		private float lerpTime = 0f;

		public void AdjustTransform(Transform transform, Quaternion newRotation) {
			// If the lock is forced, then the transform is not adjusted
			if (ForcedLock)
				return;

			Vector3 newEulerAngles = newRotation.eulerAngles;

			if (lockXAxis)
				newEulerAngles.x = transform.eulerAngles.x;

			if (lockYAxis)
				newEulerAngles.y = transform.eulerAngles.y;

			if (lockZAxis)
				newEulerAngles.z = transform.eulerAngles.z;

			// Handle lerping to the target rotation
			if (lerpToTargetRotation && lerpTime < 1) {
				lerpTime += Time.deltaTime * lerpSpeed;
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(newEulerAngles), lerpTime);

				if (lerpTime >= 1) {
					lerpTime = 0;
					lerpToTargetRotation = false;
				}
			} else {
				transform.rotation = Quaternion.Euler(newEulerAngles);
			}
		}

		public void Lock() {
			ForcedLock = true;
			lerpToTargetRotation = false;
			lerpTime = 0;
		}

		public void Unlock(bool lerping) {
			if (!ForcedLock)
				return;

			ForcedLock = false;
			lerpToTargetRotation = lerping;
			lerpTime = 0;
		}
	}
}
