using FluidSimulator.Components;
using UnityEngine;

namespace FluidSimulator.Objects {
	public class DispenserConsole : MonoBehaviour {
		[SerializeField] private GameObject _dispenserObject;

		private DispenserViewTracker _view;
		private bool _viewingDispenser;

		private void Awake() {
			_view = Camera.main.GetComponent<DispenserViewTracker>();
		}

		private void Update() {
			if (_viewingDispenser && Input.GetKeyDown(KeyCode.E)) {
				_view.ExitDispenserCamera();
				_viewingDispenser = false;

				BroadcastMessage(nameof(IDispenserObject.OnDispenserViewExited), SendMessageOptions.DontRequireReceiver);
				_dispenserObject.BroadcastMessage(nameof(IDispenserObject.OnDispenserViewExited), SendMessageOptions.DontRequireReceiver);
			}
		}

		public void JumpViewToDispenser() {
			if (_viewingDispenser)
				return;  // Already viewing the dispenser

			_view.SwitchToDispenser(_dispenserObject);
			_viewingDispenser = true;

			BroadcastMessage(nameof(IDispenserObject.OnDispenserViewEntered), SendMessageOptions.DontRequireReceiver);
			_dispenserObject.BroadcastMessage(nameof(IDispenserObject.OnDispenserViewEntered), SendMessageOptions.DontRequireReceiver);
		}
	}
}
