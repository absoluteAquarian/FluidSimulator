using AbsoluteCommons.Utility;
using UnityEngine;

namespace AbsoluteCommons.Components {
	[AddComponentMenu("Absolute Commons/Components/Animator Data Caching")]
	public class TrackedAnimator : MonoBehaviour {
		[SerializeField] private Animator _animator;

		private void OnEnable() {
			AnimatorTracker.Track(_animator);
		}

		private void OnDisable() {
			AnimatorTracker.Untrack(_animator);
		}
	}
}
