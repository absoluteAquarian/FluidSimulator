using FluidSimulator.Objects;
using FluidSimulator.Player;
using Unity.Netcode;
using UnityEngine;

namespace FluidSimulator.Bandaids {
	public class Bandaid_3_FixHostClientLosingControl : NetworkBehaviour {
		/*  Fix #3: The player should regain movement controls after leaving the dispenser view, and that is not happening on the host client.
		 *          To fix this, each console is manually checked for whether they're being used by the local player.
		 */

		private PlayerMovement _pm;
		private DispenserConsole _dispenserLocal, _dispenserRemote;

		private void Awake() {
			_pm = GetComponent<PlayerMovement>();
			_dispenserLocal = GameObject.Find("DispenserConsoleP1").GetComponent<DispenserConsole>();
			_dispenserRemote = GameObject.Find("DispenserConsoleP2").GetComponent<DispenserConsole>();
		}

		private void Update() {
			if (base.IsLocalPlayer && !_dispenserLocal.IsViewingDispenser && !_dispenserRemote.IsViewingDispenser)
				_pm.enabled = true;
		}
	}
}
