using UnityEngine;

namespace AbsoluteCommons.Utility {
	partial class TypeExtensions {
		public static void SetRotationWithPivot(this Transform transform, Vector3 pivot, Quaternion rotation) {
			Vector3 offset = pivot - transform.position;
			transform.SetPositionAndRotation(pivot, rotation);
			transform.position -= rotation * offset;
		}

		// TODO: this method doesn't do what i want it to do
		public static void AddRotationWithPivot(this Transform transform, Vector3 pivot, Quaternion rotation) {
			Vector3 offset = pivot - transform.position;
			transform.SetPositionAndRotation(pivot, transform.rotation * rotation);
			transform.position -= rotation * offset;
		}
	}
}
