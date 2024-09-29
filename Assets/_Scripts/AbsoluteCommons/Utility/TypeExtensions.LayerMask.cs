using UnityEngine;

namespace AbsoluteCommons.Utility {
	partial class TypeExtensions {
		public static LayerMask Combine(this LayerMask mask, LayerMask other) {
			return mask | other;
		}

		public static LayerMask Exclusion(this LayerMask mask) {
			return ~mask;
		}

		public static LayerMask Intersection(this LayerMask mask, LayerMask other) {
			return mask & other;
		}

		public static LayerMask ToLayerMask(this int layer) {
			return 1 << layer;
		}

		public static LayerMask ToLayerMask(this string layerName) {
			return 1 << LayerMask.NameToLayer(layerName);
		}

		public static LayerMask Union(this LayerMask mask, LayerMask other) {
			return mask | other;
		}

	}
}
