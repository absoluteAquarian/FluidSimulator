using Unity.Netcode.Components;
using UnityEngine;

namespace AbsoluteCommons.Utility {
	partial class TypeExtensions {
		public static void IncrementFloat(this Animator animator, string parameterName, float value) {
			if (animator.HasParameter(parameterName))
				animator.SetFloat(parameterName, animator.GetFloat(parameterName) + value);
		}

		public static void IncrementInt(this Animator animator, string parameterName, int value) {
			if (animator.HasParameter(parameterName))
				animator.SetInteger(parameterName, animator.GetInteger(parameterName) + value);
		}

		public static void ForceTrigger(this Animator animator, string triggerName) {
			if (animator.HasParameter(triggerName)) {
				animator.ResetTrigger(triggerName);
				animator.SetTrigger(triggerName);
			}
		}

		public static void NetForceTrigger(this Animator animator, string triggerName) {
			if (animator.gameObject.TryGetComponent(out NetworkAnimator networkSelf))
				networkSelf.ForceTrigger(triggerName);
			else
				animator.ForceTrigger(triggerName);
		}

		public static void ForceTrigger(this Animator animator, int triggerHash) {
			if (animator.HasParameter(triggerHash)) {
				animator.ResetTrigger(triggerHash);
				animator.SetTrigger(triggerHash);
			}
		}

		public static void NetForceTrigger(this Animator animator, int triggerHash) {
			if (animator.gameObject.TryGetComponent(out NetworkAnimator networkSelf))
				networkSelf.ForceTrigger(triggerHash);
			else
				animator.ForceTrigger(triggerHash);
		}

		public static bool HasParameter(this Animator animator, string parameterName) => AnimatorTracker.HasParameter(animator, parameterName);

		public static bool HasParameter(this Animator animator, int parameterHash) => AnimatorTracker.HasParameter(animator, parameterHash);

		public static void SetTriggerSafely(this Animator animator, string parameterName) {
			if (animator.HasParameter(parameterName))
				animator.SetTrigger(parameterName);
		}

		public static void NetSetTriggerSafely(this Animator animator, string parameterName) {
			if (animator.gameObject.TryGetComponent(out NetworkAnimator networkSelf))
				networkSelf.SetTriggerSafely(parameterName);
			else
				animator.SetTriggerSafely(parameterName);
		}

		public static void SetTriggerSafely(this Animator animator, int parameterHash) {
			if (animator.HasParameter(parameterHash))
				animator.SetTrigger(parameterHash);
		}

		public static void NetSetTriggerSafely(this Animator animator, int parameterHash) {
			if (animator.gameObject.TryGetComponent(out NetworkAnimator networkSelf))
				networkSelf.SetTriggerSafely(parameterHash);
			else
				animator.SetTriggerSafely(parameterHash);
		}

		public static void ResetTriggerSafely(this Animator animator, string parameterName) {
			if (animator.HasParameter(parameterName))
				animator.ResetTrigger(parameterName);
		}

		public static void NetResetTriggerSafely(this Animator animator, string parameterName) {
			if (animator.gameObject.TryGetComponent(out NetworkAnimator networkSelf))
				networkSelf.ResetTriggerSafely(parameterName);
			else
				animator.ResetTriggerSafely(parameterName);
		}

		public static void ResetTriggerSafely(this Animator animator, int parameterHash) {
			if (animator.HasParameter(parameterHash))
				animator.ResetTrigger(parameterHash);
		}

		public static void NetResetTriggerSafely(this Animator animator, int parameterHash) {
			if (animator.gameObject.TryGetComponent(out NetworkAnimator networkSelf))
				networkSelf.ResetTriggerSafely(parameterHash);
			else
				animator.ResetTriggerSafely(parameterHash);
		}

		public static void SetBoolSafely(this Animator animator, string parameterName, bool value) {
			if (animator.HasParameter(parameterName))
				animator.SetBool(parameterName, value);
		}

		public static void SetBoolSafely(this Animator animator, int parameterHash, bool value) {
			if (animator.HasParameter(parameterHash))
				animator.SetBool(parameterHash, value);
		}

		public static void SetFloatSafely(this Animator animator, string parameterName, float value) {
			if (animator.HasParameter(parameterName))
				animator.SetFloat(parameterName, value);
		}

		public static void SetFloatSafely(this Animator animator, int parameterHash, float value) {
			if (animator.HasParameter(parameterHash))
				animator.SetFloat(parameterHash, value);
		}

		public static void SetIntSafely(this Animator animator, string parameterName, int value) {
			if (animator.HasParameter(parameterName))
				animator.SetInteger(parameterName, value);
		}

		public static void SetIntSafely(this Animator animator, int parameterHash, int value) {
			if (animator.HasParameter(parameterHash))
				animator.SetInteger(parameterHash, value);
		}

		public static float GetFloatSafely(this Animator animator, string parameterName, float defaultValue = 0f) {
			if (animator.HasParameter(parameterName))
				return animator.GetFloat(parameterName);

			return defaultValue;
		}

		public static float GetFloatSafely(this Animator animator, int parameterHash, float defaultValue = 0f) {
			if (animator.HasParameter(parameterHash))
				return animator.GetFloat(parameterHash);

			return defaultValue;
		}

		public static int GetIntSafely(this Animator animator, string parameterName, int defaultValue = 0) {
			if (animator.HasParameter(parameterName))
				return animator.GetInteger(parameterName);

			return defaultValue;
		}

		public static int GetIntSafely(this Animator animator, int parameterHash, int defaultValue = 0) {
			if (animator.HasParameter(parameterHash))
				return animator.GetInteger(parameterHash);

			return defaultValue;
		}

		public static bool GetBoolSafely(this Animator animator, string parameterName, bool defaultValue = false) {
			if (animator.HasParameter(parameterName))
				return animator.GetBool(parameterName);

			return defaultValue;
		}

		public static bool GetBoolSafely(this Animator animator, int parameterHash, bool defaultValue = false) {
			if (animator.HasParameter(parameterHash))
				return animator.GetBool(parameterHash);

			return defaultValue;
		}
	}
}
