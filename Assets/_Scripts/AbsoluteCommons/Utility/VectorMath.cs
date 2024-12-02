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

		public static Vector3 RandomPointOnUnitSphere() {
			// From: https://math.stackexchange.com/a/1586185

			float u1 = Random.Range(0f, 1f);
			float u2 = Random.Range(0f, 1f);
			float phi = Mathf.Acos(2f * u1 - 1f) - Mathf.PI / 2;
			float lambda = 2f * Mathf.PI * u2;

			return new Vector3(Mathf.Cos(phi) * Mathf.Cos(lambda), Mathf.Cos(phi) * Mathf.Sin(lambda), Mathf.Sin(phi));
		}
	}
}
