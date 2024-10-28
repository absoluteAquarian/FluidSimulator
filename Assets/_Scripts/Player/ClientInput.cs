using AbsoluteCommons.Input;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace FluidSimulator.Player {
	public class ClientInput : NetworkBehaviour {
		private void Awake() {
			if (!InputMapper.Initialize(this.gameObject, GetControls()))
				Destroy(this);
		}

		private static IEnumerable<IInputControl> GetControls() {
			yield return new InputAxis("Horizontal");
			yield return new InputAxis("Vertical");
			yield return new InputAxis("Mouse X");
			yield return new InputAxis("Mouse Y");
		//	yield return new InputKey(KeyCode.F);  // Camera Firstperson Toggle
			yield return new InputKey(KeyCode.Escape);  // Camera Lock Toggle
			yield return new InputAxis("Interact");
			yield return new InputKey(KeyCode.E);  // Exit dispenser view
		}

		private void Update() => InputMapper.Update();

		public override void OnDestroy() => InputMapper.Destroy(this.gameObject);

		public static float GetRaw(string name) => InputMapper.GetRaw(name);

		public static float GetRaw(KeyCode key) => InputMapper.GetRaw(key);

		public static bool IsInactive(string name) => InputMapper.IsInactive(name);

		public static bool IsInactive(KeyCode key) => InputMapper.IsInactive(key);

		public static bool IsTriggered(string name) => InputMapper.IsTriggered(name);

		public static bool IsTriggered(KeyCode key) => InputMapper.IsTriggered(key);

		public static bool IsPressed(string name) => InputMapper.IsPressed(name);

		public static bool IsPressed(KeyCode key) => InputMapper.IsPressed(key);

		public static bool IsReleased(string name) => InputMapper.IsReleased(name);

		public static bool IsReleased(KeyCode key) => InputMapper.IsReleased(key);
	}
}
