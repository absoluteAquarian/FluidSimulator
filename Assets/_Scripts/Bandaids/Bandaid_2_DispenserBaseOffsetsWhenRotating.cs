using AbsoluteCommons.Utility;
using Unity.Netcode;
using UnityEngine;

namespace FluidSimulator.Bandaids {
	public class Bandaid_2_DispenserBaseOffsetsWhenRotating : NetworkBehaviour {
		/*  Fix #2: When rotating the dispenser on remote clients, the base would warp to the world origin.
		 *          This is fixed by just forcing it to the expected position.
		 */

		private GameObject _objBase;

		private void Awake() {
			_objBase = gameObject.GetChild("Base");
		}

		private void Update() {
			_objBase.transform.localPosition = Vector3.zero;  // Force the local position back to the parent
		}
	}
}
