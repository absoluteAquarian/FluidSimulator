using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AbsoluteCommons.Utility {
	// Inspired by: https://gist.github.com/hasanbayatme/f7f1d9d0f8470b718fda836f6668c215
	public static class AnimatorTracker {
		private static readonly Dictionary<Animator, HashSet<int>> _animatorToParameters = new();
		private static readonly Dictionary<Animator, int> _animatorToUsageCount = new();
		private static readonly Dictionary<string, int> _parameterHashes = new();

		public static void Clear() {
			_animatorToParameters.Clear();
			_animatorToUsageCount.Clear();
			_parameterHashes.Clear();
		}

		public static int GetParameterNameHash(string name) {
			if (_parameterHashes.TryGetValue(name, out int parameterHash))
				return parameterHash;

			return _parameterHashes[name] = Animator.StringToHash(name);
		}

		public static void Track(Animator animator) {
			if (_animatorToUsageCount.TryGetValue(animator, out int count)) {
				_animatorToUsageCount[animator] = count + 1;
			} else {
				_animatorToUsageCount[animator] = 1;
				_animatorToParameters[animator] = GetParameters(animator);
			}
		}

		public static void Untrack(Animator animator) {
			if (_animatorToUsageCount.TryGetValue(animator, out int count)) {
				if (count == 1) {
					_animatorToUsageCount.Remove(animator);
					_animatorToParameters.Remove(animator);
				} else {
					_animatorToUsageCount[animator] = count - 1;
				}
			} else {
				Debug.LogWarning($"Untracking an Animator that was not tracked: {animator}");
			}
		}

		public static HashSet<int> GetParameters(Animator animator) {
			if (_animatorToParameters.TryGetValue(animator, out HashSet<int> parameters))
				return parameters;

			parameters = new HashSet<int>();

			for (int i = 0; i < animator.parameterCount; i++) {
				AnimatorControllerParameter parameter = animator.GetParameter(i);
				parameters.Add(parameter.nameHash);
			}

			return parameters;
		}

		public static bool HasParameter(Animator animator, int parameterNameHash) => GetParameters(animator).Contains(parameterNameHash);

		public static bool HasParameter(Animator animator, string parameterName) => HasParameter(animator, GetParameterNameHash(parameterName));
	}
}
