using AbsoluteCommons.Utility;
using Unity.Netcode;
using UnityEngine;

namespace FluidSimulator.Bandaids {
	public class Bandaid_1_IncorrectSpoutSpawnTransform : NetworkBehaviour {
		/*  Fix #1: Dispensers were spawning with the wrong orientation on remote clients.
		 *          This is fixed by just forcing it to the expected orientation.
		 */

		protected override void OnNetworkPostSpawn() {
			GameObject objBase = gameObject.GetChild("Base");
			objBase.transform.localEulerAngles = Vector3.zero;  // Base was given an incorrect orientation of -180 Z

			GameObject objSpout = gameObject.GetChild("Base/Spout");
			objSpout.transform.localEulerAngles = Vector3.zero;  // Spout was given an incorrect orientation of -180 Z
		}
	}
}
