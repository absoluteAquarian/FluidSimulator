using UnityEngine;

namespace AbsoluteCommons.Utility {
	public static class VectorMath {
		public static float DistanceSquared(Vector3 a, Vector3 b) => Vector3.SqrMagnitude(a - b);

		public static Vector3 DirectionTo(Vector3 from, Vector3 to) => Vector3.Normalize(to - from);

		public static Vector3 DirectionFrom(Vector3 from, Vector3 to) => Vector3.Normalize(from - to);

		public static void RestrictMagnitude(ref Vector2 vector, float maxMagnitude) {
			if (vector.sqrMagnitude > maxMagnitude * maxMagnitude)
				vector = vector.normalized * maxMagnitude;
		}

		public static void RestrictMagnitude(ref Vector3 vector, float maxMagnitude) {
			if (vector.sqrMagnitude > maxMagnitude * maxMagnitude)
				vector = vector.normalized * maxMagnitude;
		}

		public static Vector2 GetXZ(this Vector3 vector) => new Vector2(vector.x, vector.z);

		public static Vector3 ToXZ(this Vector2 vector, float y = 0) => new Vector3(vector.x, y, vector.y);
	}
}
