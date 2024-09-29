using Unity.Netcode.Components;
using UnityEngine;

namespace AbsoluteCommons.Utility {
	partial class TypeExtensions {
		public static void ForceTrigger(this NetworkAnimator animator, string triggerName) {
			if (animator.HasParameter(triggerName)) {
				animator.ResetTrigger(triggerName);
				animator.SetTrigger(triggerName);
			}
		}

		public static void ForceTrigger(this NetworkAnimator animator, int triggerHash) {
			if (animator.HasParameter(triggerHash)) {
				animator.ResetTrigger(triggerHash);
				animator.SetTrigger(triggerHash);
			}
		}

		public static bool HasParameter(this NetworkAnimator animator, string parameterName) {
			Animator nonNetworkSelf = animator.gameObject.GetComponent<Animator>();
			return nonNetworkSelf && nonNetworkSelf.HasParameter(parameterName);
		}

		public static bool HasParameter(this NetworkAnimator animator, int parameterHash) {
			Animator nonNetworkSelf = animator.gameObject.GetComponent<Animator>();
			return nonNetworkSelf && nonNetworkSelf.HasParameter(parameterHash);
		}

		public static void SetTriggerSafely(this NetworkAnimator animator, string parameterName) {
			if (animator.HasParameter(parameterName))
				animator.SetTrigger(parameterName);
		}

		public static void SetTriggerSafely(this NetworkAnimator animator, int parameterHash) {
			if (animator.HasParameter(parameterHash))
				animator.SetTrigger(parameterHash);
		}

		public static void ResetTriggerSafely(this NetworkAnimator animator, string parameterName) {
			if (animator.HasParameter(parameterName))
				animator.ResetTrigger(parameterName);
		}

		public static void ResetTriggerSafely(this NetworkAnimator animator, int parameterHash) {
			if (animator.HasParameter(parameterHash))
				animator.ResetTrigger(parameterHash);
		}
	}
}
