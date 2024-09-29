using Unity.Netcode;

namespace AbsoluteCommons.Networking {
	public class VisibleOnlyToLocalClient : NetworkBehaviour {
		public override void OnNetworkSpawn() {
			if (!base.IsOwner) {
				NetworkObject netObj = GetComponent<NetworkObject>();
				if (netObj && netObj.IsNetworkVisibleTo(NetworkManager.LocalClientId))
					netObj.NetworkHide(NetworkManager.LocalClientId);
			}

			base.OnNetworkSpawn();
		}
	}
}
