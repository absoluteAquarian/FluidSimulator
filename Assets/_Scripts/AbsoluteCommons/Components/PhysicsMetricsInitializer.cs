using AbsoluteCommons.Attributes;
using UnityEngine;

namespace AbsoluteCommons.Components {
	[RequireComponent(typeof(PhysicsMetrics), typeof(PhysicsMetricsFinalizer))]
	public class PhysicsMetricsInitializer : MonoBehaviour {
		internal Vector3 cachedGravity;
		internal bool hasCheckedGravity;

		#if UNITY_EDITOR
		[SerializeField, ReadOnly] private Vector3 _cachedGravity;
		#endif

		private void Update() {
			// Override the global gravity for components that use it
			PhysicsMetrics metrics = GetComponent<PhysicsMetrics>();
			
			cachedGravity = Physics.gravity;

			#if UNITY_EDITOR
			_cachedGravity = cachedGravity;
			#endif

			if (metrics.useGravityOverride)
				Physics.gravity = metrics.gravityOverride;

			Physics.gravity *= metrics.gravityScale;

			hasCheckedGravity = true;
		}
	}
}
