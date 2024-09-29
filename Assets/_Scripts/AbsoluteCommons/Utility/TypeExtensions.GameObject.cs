using System.Collections.Generic;
using UnityEngine;

namespace AbsoluteCommons.Utility {
	partial class TypeExtensions {
		public static GameObject FindChildRecursively(this GameObject parent, string name) {
			Queue<GameObject> scanQueue = new();
			scanQueue.Enqueue(parent);

			// Traverse the hierarchy
			while (scanQueue.TryDequeue(out GameObject current)) {
				if (current.name == name)
					return current;

				foreach (Transform child in current.transform)
					scanQueue.Enqueue(child.gameObject);
			}

			return null;
		}

		public static GameObject GetChild(this GameObject parent, string path) {
			string[] pathParts = path.Split('/');
			Transform current = parent.transform;

			// Traverse the path
			foreach (string part in pathParts) {
				current = current.Find(part);
				if (current == null)
					return null;
			}

			return current.gameObject;
		}

		public static T GetChildComponent<T>(this GameObject parent, string path) where T : Component {
			GameObject child = parent.GetChild(path);
			return child == null ? null : child.GetComponent<T>();
		}

		public static bool IsObjectOrParentOfObject(this GameObject searcher, GameObject target) {
			if (!searcher)
				return false;

			Transform check = target.transform;
			
			while (check != null) {
				if (check.gameObject == searcher)
					return true;

				check = check.parent;
			}

			return false;
		}
	}
}
