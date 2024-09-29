using UnityEngine;

namespace AbsoluteCommons.Components {
	[RequireComponent(typeof(PhysicsMetricsInitializer), typeof(PhysicsMetrics))]
	public class PhysicsMetricsFinalizer : MonoBehaviour {
		private void Update() {
			var initializer = GetComponent<PhysicsMetricsInitializer>();
			if (!initializer.hasCheckedGravity) {
				Debug.LogError($"[PhysicsMetricsFinalizer] Object {gameObject.name} has not checked gravity yet. Move PhysicsMetricsInitializer component to above PhysicsMetricsFinalizer component.");
				return;
			}

			// Restore the global gravity
			Physics.gravity = initializer.cachedGravity;

			initializer.hasCheckedGravity = false;
		}
	}
}
