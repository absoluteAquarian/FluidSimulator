using UnityEngine;

namespace AbsoluteCommons.Utility {
	public static class RotationMath {
		public static Quaternion RotationTo(Vector3 from, Vector3 to) => Quaternion.LookRotation(to - from, Vector3.up);

		public static Quaternion RotationTo(Vector3 from, Vector3 to, Vector3 up) => Quaternion.LookRotation(to - from, up);

		public static Quaternion RotationFrom(Vector3 from, Vector3 to) => Quaternion.LookRotation(from - to, Vector3.up);

		public static Quaternion RotationFrom(Vector3 from, Vector3 to, Vector3 up) => Quaternion.LookRotation(from - to, up);
	}
}
