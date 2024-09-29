using System.Collections.Generic;
using UnityEngine;

namespace AbsoluteCommons.Input {
	public static class InputMapper {
		private static InputMap _map;

		private static GameObject _clientActor;

		public static bool Initialize(GameObject actor, IEnumerable<IInputControl> controls) {
			if (_clientActor)
				return false;

			_map = new InputMap();
			foreach (var control in controls)
				control.Register(_map);

			_clientActor = actor;
			return true;
		}

		public static void Update() => _map.Update();

		public static void Destroy(GameObject actor) {
			if (_clientActor == actor)
				_clientActor = null;
		}

		private static bool IsInputValid() => _clientActor && _map != null;

		public static float GetRaw(string name) => IsInputValid() ? _map.GetRaw(name) : 0;

		public static float GetRaw(KeyCode key) => IsInputValid() ? _map.GetRaw(key) : 0;

		public static bool IsInactive(string name) => IsInputValid() && _map.IsInactive(name);

		public static bool IsInactive(KeyCode key) => IsInputValid() && _map.IsInactive(key);

		public static bool IsTriggered(string name) => IsInputValid() && _map.IsTriggered(name);

		public static bool IsTriggered(KeyCode key) => IsInputValid() && _map.IsTriggered(key);

		public static bool IsPressed(string name) => IsInputValid() && _map.IsPressed(name);

		public static bool IsPressed(KeyCode key) => IsInputValid() && _map.IsPressed(key);

		public static bool IsReleased(string name) => IsInputValid() && _map.IsReleased(name);

		public static bool IsReleased(KeyCode key) => IsInputValid() && _map.IsReleased(key);
	}
}
