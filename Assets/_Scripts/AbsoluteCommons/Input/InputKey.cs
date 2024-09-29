using UnityEngine;

namespace AbsoluteCommons.Input {
	public readonly struct InputKey : IInputControl {
		public readonly KeyCode Key { get; }

		public InputKey(KeyCode key) => Key = key;

		void IInputControl.Register(InputMap map) => map.DefineKey(Key);
	}
}
