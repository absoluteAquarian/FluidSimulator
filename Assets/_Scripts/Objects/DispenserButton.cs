using AbsoluteCommons.Attributes;
using FluidSimulator.Components;
using FluidSimulator.Player;
using UnityEngine;

namespace FluidSimulator.Objects {
	public class DispenserButton : MonoBehaviour, IInteractable, IDispenserObject {
		private DispenserConsole _console;
		private ChangeMaterialOnActivate _changeMaterial;

		[SerializeField, ReadOnly] private GameObject _whoPressedMe;

		private void Awake() {
			_console = GetComponentInParent<DispenserConsole>();
			_changeMaterial = GetComponent<ChangeMaterialOnActivate>();
		}

		public void OnInteract(WorldInteraction interaction) {
			if (interaction.target != gameObject)
				return;  // Not the target, do nothing
			
			_console.JumpViewToDispenser();

			_whoPressedMe = interaction.actor;

			// Lock control from the player
			if (_whoPressedMe.TryGetComponent(out PlayerMovement movement)) {
				movement.enabled = false;

				Debug.Log($"[DispenserButton] [OnInteract] Player movement for object \"{_whoPressedMe.name}\" has been disabled");
			}
		}

		public void OnDispenserViewEntered() { }

		public void OnDispenserViewExited() {
			_changeMaterial.SetActive(false);

			// Restore control to the player
			if (_whoPressedMe && _whoPressedMe.TryGetComponent(out PlayerMovement movement)) {
				movement.enabled = true;

				Debug.Log($"[DispenserButton] [OnDispenserViewExited] Player movement for object \"{_whoPressedMe.name}\" has been re-enabled");
			}

			_whoPressedMe = null;
		}
	}
}
