using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace AbsoluteCommons.Utility {
	public static class EasingExtensions {
		public static float Ease(EasingMode mode, float t) {
			return mode switch {
				EasingMode.Linear => t,
				EasingMode.EaseInSine => Easing.InSine(t),
				EasingMode.EaseOutSine => Easing.OutSine(t),
				EasingMode.EaseInOutSine => Easing.InOutSine(t),
				EasingMode.EaseInCubic => Easing.InCubic(t),
				EasingMode.EaseOutCubic => Easing.OutCubic(t),
				EasingMode.EaseInOutCubic => Easing.InOutCubic(t),
				EasingMode.EaseInCirc => Easing.InCirc(t),
				EasingMode.EaseOutCirc => Easing.OutCirc(t),
				EasingMode.EaseInOutCirc => Easing.InOutCirc(t),
				EasingMode.EaseInElastic => Easing.InElastic(t),
				EasingMode.EaseOutElastic => Easing.OutElastic(t),
				EasingMode.EaseInOutElastic => Easing.InOutElastic(t),
				EasingMode.EaseInBack => Easing.InBack(t),
				EasingMode.EaseOutBack => Easing.OutBack(t),
				EasingMode.EaseInOutBack => Easing.InOutBack(t),
				EasingMode.EaseInBounce => Easing.InBounce(t),
				EasingMode.EaseOutBounce => Easing.OutBounce(t),
				EasingMode.EaseInOutBounce => Easing.InOutBounce(t),
				_ => Easing.Linear(t)
			};
		}
	}
}
