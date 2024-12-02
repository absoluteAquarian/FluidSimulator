using UnityEngine;

namespace FluidSimulator.Objects {
	[CreateAssetMenu(fileName = nameof(FluidDatabase), menuName = "Fluid Simulator/Fluid Database", order = 1)]
	public class FluidDatabase : ScriptableObject {
		[SerializeField] private GameObject[] fluids;

		public FluidParticle GetFluid(FluidID id) {
			GameObject prefab = fluids[(int)id];

			if (!prefab) {
				Debug.LogError($"[FluidDatabase] [Get] Fluid ID {id} does not exist");
				return null;
			}

			return prefab.GetComponent<FluidParticle>();
		}
	}
}
