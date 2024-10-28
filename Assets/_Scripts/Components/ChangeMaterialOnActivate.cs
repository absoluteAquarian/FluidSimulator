using AbsoluteCommons.Attributes;
using FluidSimulator.Player;
using UnityEngine;

namespace FluidSimulator.Components {
	public class ChangeMaterialOnActivate : MonoBehaviour, IInteractable {
		[SerializeField] public Color activeColor = Color.green;

		private Renderer[] _renderers;
		private Color[] _originalColors;

		[SerializeField] private bool active = false;
		[SerializeField, ReadOnly] private bool _oldActive;

		private void Awake() {
			// Get all renderers
			_renderers = GetComponentsInChildren<Renderer>();
			_originalColors = new Color[_renderers.Length];
			for (int i = 0; i < _renderers.Length; i++)
				_originalColors[i] = _renderers[i].material.color;
		}

		private void Update() {
			if (active != _oldActive) {
				Debug.Log("Material color states modified");

				_oldActive = active;

				if (active) {
					// Force the colors to be the active color
					for (int i = 0; i < _renderers.Length; i++)
						_renderers[i].material.color = activeColor;
				} else {
					// Restore the original colors
					for (int i = 0; i < _renderers.Length; i++)
						_renderers[i].material.color = _originalColors[i];
				}
			}
		}

		public void SetActive(bool active) {
			this.active = active;

			Debug.Log($"Activation state changed to {active}");
		}

		public void OnInteract(WorldInteraction interaction) => SetActive(true);
	}
}
