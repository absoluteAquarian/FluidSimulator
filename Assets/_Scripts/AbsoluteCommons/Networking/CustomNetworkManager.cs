using AbsoluteCommons.Utility;
using Unity.Netcode;
using UnityEngine;

namespace AbsoluteCommons.Networking {
	[AddComponentMenu("Tower Defense/Networking/Custom Network Manager")]
	public class CustomNetworkManager : NetworkManager {
		public CustomNetworkManager() {
			base.OnClientConnectedCallback += ClientConnectedCallback;
			base.OnClientDisconnectCallback += ClientDisconnectCallback;
		}

		private void ClientConnectedCallback(ulong clientId) {
			Debug.Log($"Client connected with id: {clientId}");

			/*
			GameObject spawn = GameObject.Find("PlayerSpawn");
			if (spawn) {
				NetworkObject player = base.SpawnManager.GetPlayerNetworkObject(clientId);

				if (player)
					player.transform.SetPositionAndRotation(spawn.transform.position, spawn.transform.rotation);
			}
			*/
		}

		private void ClientDisconnectCallback(ulong clientId) {
			Debug.Log($"Client disconnected with id: {clientId}");

			NetworkObject netObj = SpawnManager.GetPlayerNetworkObject(clientId);
			if (netObj)
				netObj.SmartDespawn(true);
		}
	}
}
