using System.Collections.Generic;
using System;

namespace AbsoluteCommons.Utility {
	partial class TypeExtensions {
		public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator, bool resetOnCompletion = false) {
			if (enumerator is null)
				throw new ArgumentNullException(nameof(enumerator));

			while (enumerator.MoveNext())
				yield return enumerator.Current;

			if (resetOnCompletion)
				enumerator.Reset();
		}
	}
}
