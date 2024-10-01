using UnityEngine;
using UnityEngine.Rendering;

namespace AbsoluteCommons.Components {
	[AddComponentMenu("Absolute Commons/Rendering/Conditional Rendering")]
	public class ConditionalRendering : MonoBehaviour {
		public bool canRender = true;
		public ShadowCastingMode renderVisibleShadowMode = ShadowCastingMode.On;
		public ShadowCastingMode renderHiddenShadowMode = ShadowCastingMode.ShadowsOnly;

		private void Update() {
			foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>()) {
				if (canRender) {
					renderer.enabled = true;
					renderer.shadowCastingMode = renderVisibleShadowMode;
				} else {
					renderer.enabled = renderHiddenShadowMode != ShadowCastingMode.Off;
					renderer.shadowCastingMode = renderHiddenShadowMode;
				}
			}
		}
	}
}
