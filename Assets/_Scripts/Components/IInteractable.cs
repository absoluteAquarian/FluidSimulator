using FluidSimulator.Player;

namespace FluidSimulator.Components {
	public interface IInteractable {
		void OnInteract(WorldInteraction interaction);
	}
}
