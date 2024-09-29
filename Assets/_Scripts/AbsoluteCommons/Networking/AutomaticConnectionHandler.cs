using Unity.Netcode;
using UnityEngine;

namespace AbsoluteCommons.Networking {
	[AddComponentMenu("AbsoluteCommons/Networking/Automatic Connection Handler")]
	public class AutomaticConnectionHandler : MonoBehaviour {
		private void Start() {
			// TODO: UI for handling connections?  it won't be needed for the presentation...

			#if UNITY_EDITOR
			if (ParrelSync.ClonesManager.IsClone()) {
				NetworkManager.Singleton.StartClient();
				Debug.Log("[AutomaticConnectionHandler] Starting client for ParrelSync clone project.");
			} else {
				NetworkManager.Singleton.StartHost();
				Debug.Log("[AutomaticConnectionHandler] Starting host for ParrelSync master project.");
			}
			#endif
		}
	}
}
