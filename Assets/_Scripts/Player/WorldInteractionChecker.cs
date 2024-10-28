using AbsoluteCommons.Utility;
using FluidSimulator.Components;
using System;
using UnityEngine;

namespace FluidSimulator.Player {
	public class WorldInteractionChecker : MonoBehaviour {
		[SerializeField] private float _interactionDistance = 4f;
		[SerializeField] private LayerMask _interactionsExclusion;

		private void Update() {
			if (ClientInput.IsTriggered("Interact")) {
				// Emit a ray from the camera
				Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

				LayerMask occlusion = gameObject.layer.ToLayerMask().Combine(_interactionsExclusion).Exclusion();

				Debug.Log("Checking for interactions");

				// Check if the ray hits anything
				Debug.DrawRay(ray.origin, ray.direction * _interactionDistance, Color.green, 0.4f, true);

				if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, occlusion)) {
					GameObject hitObject = hit.collider.gameObject;

					// Indicate that the object was interacted with
					hitObject.SendMessage(nameof(IInteractable.OnInteract), new WorldInteraction(gameObject, hitObject), SendMessageOptions.DontRequireReceiver);

					// Debugging
					string timeFormat = TimeSpan.FromSeconds(Time.time).ToString(@"hh\:mm\:ss\:fff");
					Debug.Log($"[{timeFormat}] Interacted with {hitObject.name}");
				}
			}
		}
	}

	public class WorldInteraction {
		public readonly GameObject actor;
		public readonly GameObject target;

		public WorldInteraction(GameObject actor, GameObject target) {
			this.actor = actor;
			this.target = target;
		}
	}
}
