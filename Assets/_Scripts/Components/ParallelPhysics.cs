using AbsoluteCommons.Attributes;
using Unity.Netcode;
using UnityEngine;

namespace FluidSimulator.Components {
	public class ParallelPhysics : NetworkBehaviour {
		[ReadOnly] public Vector3 velocity;

		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
			if (serializer.IsWriter) {
				var writer = serializer.GetFastBufferWriter();

				writer.WriteValueSafe(velocity);
			} else {
				var reader = serializer.GetFastBufferReader();

				reader.ReadValueSafe(out velocity);
			}
		}
	}
}
