using AbsoluteCommons.Utility;
using Unity.Netcode;

namespace AbsoluteCommons.Networking {
	public class DeferredSpawning : NetworkBehaviour {
		public override void OnNetworkSpawn() {
			if (IsServer)
				GetComponent<NetworkObject>().SmartSpawn();
			base.OnNetworkSpawn();
		}

		public override void OnNetworkDespawn() {
			if (IsServer)
				GetComponent<NetworkObject>().SmartDespawn(true, includeSelf: false);
			base.OnNetworkDespawn();
		}
	}
}
