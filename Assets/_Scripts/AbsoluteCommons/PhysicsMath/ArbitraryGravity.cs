using AbsoluteCommons.Utility;
using UnityEngine;

namespace AbsoluteCommons.PhysicsMath {
	public static class ArbitraryGravity {
		/// <summary>
		/// Gets the up vector with respect to the gravity vector.
		/// </summary>
		public static Vector3 Up(Vector3 gravity) => -gravity.normalized;

		/// <summary>
		/// Gets the right vector with respect to the gravity vector.
		/// </summary>
		public static Vector3 Right(Vector3 gravity) => Vector3.Cross(Up(gravity), Vector3.forward);

		/// <summary>
		/// Gets the forward vector with respect to the gravity vector.
		/// </summary>
		public static Vector3 Forward(Vector3 gravity) => -Vector3.Cross(Up(gravity), Right(gravity));

		/// <summary>
		/// Gets a rotation quaternion with respect to the gravity vector.
		/// </summary>
		public static Quaternion GetRotation(Vector3 gravity, Vector3 forward) => Quaternion.LookRotation(forward, Up(gravity));

		/// <summary>
		/// Gets a rotation quaternion with respect to the gravity vector.
		/// </summary>
		public static Quaternion GetRotation(Vector3 gravity, Transform transform) => GetRotation(gravity, transform.forward.PerpendicularTo(gravity).normalized);

		/// <summary>
		/// Returns whether the velocity vector is in the same direction as the gravity vector.
		/// </summary>
		public static bool IsFallingToward(this Vector3 velocity, Vector3 gravity) => Vector3.Dot(velocity, gravity) > 0;

		/// <summary>
		/// Gets the component of the velocity vector parallel to the gravity vector.
		/// </summary>
		public static Vector3 ParallelTo(this Vector3 velocity, Vector3 gravity) => Vector3.Project(velocity, gravity);

		/// <summary>
		/// Gets the component of the velocity vector perpendicular to the gravity vector.
		/// </summary>
		public static Vector3 PerpendicularTo(this Vector3 velocity, Vector3 gravity) => Vector3.ProjectOnPlane(velocity, gravity);

		public static Vector3 WithPerpendicularDeadZone(this Vector3 velocity, Vector3 gravity, float deadZone) {
			Vector3 perpendicular = velocity.PerpendicularTo(gravity);

			if (perpendicular.sqrMagnitude <= deadZone * deadZone)
				return velocity.ParallelTo(gravity);

			return velocity;
		}

		/// <summary>
		/// Restricts the velocity vector to a maximum magnitude perpendicular to the gravity vector.
		/// </summary>
		public static Vector3 WithPerpendicularSpeedCap(this Vector3 velocity, Vector3 gravity, float maxVelocity) {
			Vector3 perpendicular = velocity.PerpendicularTo(gravity);

			if (perpendicular.sqrMagnitude <= maxVelocity * maxVelocity)
				return velocity;

			perpendicular = perpendicular.normalized * maxVelocity;

			Vector3 parallel = velocity.ParallelTo(gravity);

			return perpendicular + parallel;
		}

		/// <summary>
		/// Restricts the velocity vector according to a terminal velocity.
		/// </summary>
		public static Vector3 WithTerminalVelocity(this Vector3 velocity, Vector3 gravity, float terminalVelocity) {
			if (!velocity.IsFallingToward(gravity))
				return velocity;

			Vector3 parallel = velocity.ParallelTo(gravity);

			if (parallel.sqrMagnitude < terminalVelocity * terminalVelocity)
				return velocity;

			return velocity - parallel + parallel.normalized * terminalVelocity;
		}

		/// <inheritdoc cref="WithTerminalVelocity(Vector3, Vector3, float)"/>
		public static bool RestrictTerminalVelocity(ref Vector3 velocity, Vector3 gravity, float terminalVelocity) {
			if (!velocity.IsFallingToward(gravity))
				return false;

			Vector3 parallel = velocity.ParallelTo(gravity);

			if (parallel.sqrMagnitude < terminalVelocity * terminalVelocity)
				return false;

			velocity = velocity - parallel + parallel.normalized * terminalVelocity;

			return true;
		}

		/// <summary>
		/// Returns whether the velocity vector is at terminal velocity with respect to the gravity vector.
		/// </summary>
		public static bool AtTerminalVelocity(this Vector3 velocity, Vector3 gravity, float terminalVelocity) {
			if (!velocity.IsFallingToward(gravity))
				return false;

			Vector3 parallel = velocity.ParallelTo(gravity);

			return parallel.sqrMagnitude >= terminalVelocity * terminalVelocity;
		}
	}
}
