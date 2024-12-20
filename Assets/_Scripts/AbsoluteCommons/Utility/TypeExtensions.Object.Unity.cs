﻿using AbsoluteCommons.Objects;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace AbsoluteCommons.Utility {
	partial class TypeExtensions {
		public static void DestroyAndSetNull(ref Object obj) {
			if (obj) {
				Object.Destroy(obj);
				obj = null;
			}
		}

		public static void DestroyOrDespawnAndSetNull(ref GameObject obj) {
			if (obj) {
				if (obj.TryGetComponent(out NetworkObject netObj))
					netObj.SmartDespawn(true);
				else
					Object.Destroy(obj);

				obj = null;
			}
		}

		public static void DestroyDespawnOrReturnToPoolAndSetNull(ref GameObject obj) {
			if (obj) {
				if (obj.TryGetComponent(out PooledObject pooled))
					pooled.ReturnToPool();
				else if (obj.TryGetComponent(out NetworkObject netObj))
					netObj.SmartDespawn(true);
				else
					Object.Destroy(obj);

				obj = null;
			}
		}

		public static void DestroyAndSetNull<T>(ref T obj) where T : Object {
			if (obj) {
				Object.Destroy(obj);
				obj = null;
			}
		}

		public static bool TryGetComponentInParent<T>(this GameObject obj, out T component) where T : Component {
			Transform current = obj.transform;

			while (current) {
				if (current.TryGetComponent(out component))
					return true;

				current = current.parent;
			}

			component = null;
			return false;
		}

		public static string GetHierarchyPath(this GameObject obj) {
			StringBuilder sb = new StringBuilder(obj.name);
			Transform current = obj.transform;

			while (current.parent) {
				current = current.parent;
				sb.Insert(0, '/').Insert(0, current.name);
			}

			return sb.ToString();
		}
	}
}
