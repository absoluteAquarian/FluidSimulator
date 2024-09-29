using Unity.Netcode;
using UnityEngine;

namespace AbsoluteCommons.Networking {
	public class ClientModelSpawner : NetworkBehaviour {
		public GameObject localClientPrefab;
		public GameObject remoteClientPrefab;

		// Code was derived from: https://forum.unity.com/threads/different-prefab-for-each-player-netcode.1235155

		public override void OnNetworkSpawn() {
			if (base.IsServer)
				SpawnClientObjectServerRpc(NetworkManager.Singleton.LocalClientId, true);
			else
				SpawnClientObjectServerRpc(NetworkManager.Singleton.LocalClientId, false);

			base.OnNetworkSpawn();
		}

		[ServerRpc(RequireOwnership = false)]
		private void SpawnClientObjectServerRpc(ulong clientID, bool localClient) {
			GameObject obj = Instantiate(localClient ? localClientPrefab : remoteClientPrefab);
			obj.SetActive(true);
			NetworkObject netObj = obj.GetComponent<NetworkObject>();
			// NOTE: AddComponent will not be synced to the client!
			// obj.AddComponent<DeferredSpawning>();  // Ensure that child objects are also spawned
			netObj.SpawnAsPlayerObject(clientID, true);

			SetClientPositionClientRpc(netObj);
		}

		[ClientRpc]
		private void SetClientPositionClientRpc(NetworkObjectReference obj) {
			NetworkObject clientObj = obj;
			if (!clientObj)
				return;

			// Move the player to the spawn point
			clientObj.transform.SetPositionAndRotation(gameObject.transform.position, gameObject.transform.rotation);
			Debug.Log($"Client {clientObj.OwnerClientId} object spawned at {gameObject.transform.position}");
		}
	}
}
