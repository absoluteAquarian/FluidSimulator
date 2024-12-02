using AbsoluteCommons.Attributes;
using FluidSimulator.Components;
using FluidSimulator.Player;
using Unity.Netcode;
using UnityEngine;

namespace FluidSimulator.Objects {
	public class DispenserConsole : NetworkBehaviour {
		[SerializeField] private GameObject _dispenserObject;

		private DispenserViewTracker _view;
		[SerializeField, ReadOnly] private bool _viewingDispenser;

		public bool IsViewingDispenser => _viewingDispenser;

		private void Awake() {
			_view = Camera.main.GetComponent<DispenserViewTracker>();
		}

		private void Update() {
			if (_viewingDispenser && ClientInput.IsTriggered(KeyCode.E)) {
				_view.ExitDispenserCamera();
				_viewingDispenser = false;

				BroadcastExit();

				Debug.Log("Exiting dispenser view");
			}
		}

		private void BroadcastExit() {
			if (base.IsServer)
				BroadcastExit_SendMessages();
			else
				BroadcastExitServerRpc();
		}

		[ServerRpc(RequireOwnership = false)]
		private void BroadcastExitServerRpc() {
			BroadcastExitClientRpc();
		}

		[ClientRpc]
		private void BroadcastExitClientRpc() {
			if (IsOwner)
				return;

			_view.ExitDispenserCamera();
			_viewingDispenser = false;

			BroadcastExit_SendMessages();
		}

		private void BroadcastExit_SendMessages() {
			BroadcastMessage(nameof(IDispenserObject.OnDispenserViewExited), SendMessageOptions.DontRequireReceiver);
			_dispenserObject.BroadcastMessage(nameof(IDispenserObject.OnDispenserViewExited), SendMessageOptions.DontRequireReceiver);
		}

		public void JumpViewToDispenser() {
			if (_viewingDispenser)
				return;  // Already viewing the dispenser

			_view.SwitchToDispenser(_dispenserObject);
			_viewingDispenser = true;

			BroadcastEnter();

			Debug.Log("Jumping to dispenser view");
		}

		private void BroadcastEnter() {
			if (base.IsServer)
				BroadcastEnter_SendMessages();
			else
				BroadcastEnterServerRpc();
		}

		[ServerRpc(RequireOwnership = false)]
		private void BroadcastEnterServerRpc() {
			BroadcastEnterClientRpc();
		}

		[ClientRpc]
		private void BroadcastEnterClientRpc() {
			if (IsOwner)
				return;

			_view.SwitchToDispenser(_dispenserObject);
			_viewingDispenser = true;

			BroadcastEnter_SendMessages();
		}

		private void BroadcastEnter_SendMessages() {
			BroadcastMessage(nameof(IDispenserObject.OnDispenserViewEntered), SendMessageOptions.DontRequireReceiver);
			_dispenserObject.BroadcastMessage(nameof(IDispenserObject.OnDispenserViewEntered), SendMessageOptions.DontRequireReceiver);
		}
	}
}
