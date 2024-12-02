using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace AbsoluteCommons.Utility {
	partial class TypeExtensions {
		public static void SmartDespawn(this NetworkObject obj, bool destroy, bool includeSelf = true) {
			if (!NetworkManager.Singleton.IsServer)
				return;

			// Calling despawn will move child objects to the root of the scene
			// This is a workaround to keep the hierarchy clean
			Stack<NetworkObject> despawnStack = new Stack<NetworkObject>();
			Queue<Transform> despawnQueue = new Queue<Transform>();
			despawnQueue.Enqueue(obj.gameObject.transform);

			while (despawnQueue.TryDequeue(out Transform current)) {
				if ((includeSelf || current != obj.gameObject.transform) && current.TryGetComponent(out NetworkObject networkObject))
					despawnStack.Push(networkObject);

				foreach (Transform child in current)
					despawnQueue.Enqueue(child);
			}

			while (despawnStack.TryPop(out NetworkObject networkObject)) {
				if (networkObject.IsSpawned)
					networkObject.Despawn(destroy);
			}
		}

		public static void SmartSpawn(this NetworkObject obj, bool destroyWithScene = false, bool syncHierarchy = true) {
			// Calling spawn will not spawn child network objects
			Queue<Transform> spawnQueue = new Queue<Transform>();
			spawnQueue.Enqueue(obj.gameObject.transform);

			while (spawnQueue.TryDequeue(out Transform current)) {
				if (current.TryGetComponent(out NetworkObject networkObject) && !networkObject.IsSpawned)
					networkObject.Spawn(destroyWithScene);

				foreach (Transform child in current)
					spawnQueue.Enqueue(child);
			}

			if (syncHierarchy)
				obj.SyncHierarchy(true);
		}

		public static void SyncHierarchy(this NetworkObject obj, bool onlyCheckDirectParent = false) {
			Queue<Transform> queue = new Queue<Transform>();
			
			if (!onlyCheckDirectParent) {
				Transform rootCurrent = obj.gameObject.transform, root = rootCurrent;
				while (rootCurrent.parent && rootCurrent.gameObject.TryGetComponent(out NetworkObject _)) {
					root = rootCurrent;
					rootCurrent = rootCurrent.parent;
				}

				queue.Enqueue(root);
			} else {
				Transform transform = obj.gameObject.transform;
				if (transform.parent && transform.parent.gameObject.TryGetComponent(out NetworkObject _))
					queue.Enqueue(transform.parent);
				else
					queue.Enqueue(transform);
			}

			while (queue.TryDequeue(out Transform current)) {
				if (current.TryGetComponent(out NetworkObject networkObject)) {
					foreach (Transform child in current) {
						if (child.TryGetComponent(out NetworkObject childNetworkObject))
							EnsureParentConnectionServerRpc(networkObject, childNetworkObject);
					}
				}
			}
		}

		[ServerRpc(RequireOwnership = false)]
		private static void EnsureParentConnectionServerRpc(NetworkObjectReference parentRef, NetworkObjectReference childRef) {
			NetworkObject parentNetworkObject = parentRef;
			if (!parentNetworkObject)
				return;

			NetworkObject childNetworkObject = childRef;
			if (!childNetworkObject)
				return;

			childNetworkObject.transform.SetParent(parentNetworkObject.transform, false);
		}
	}
}
