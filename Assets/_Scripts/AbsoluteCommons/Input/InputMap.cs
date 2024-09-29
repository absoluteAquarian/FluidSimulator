using System;
using System.Collections.Generic;
using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace AbsoluteCommons.Input {
	public class InputMap {
		private InputState[] _states = Array.Empty<InputState>();
		private float[] _raw = Array.Empty<float>();
		private readonly Dictionary<string, int> _map = new();
		private readonly Dictionary<KeyCode, int> _keyMap = new();

		public InputMap DefineAxis(string name) {
			_map[name] = _states.Length;
			Array.Resize(ref _states, _states.Length + 1);
			Array.Resize(ref _raw, _raw.Length + 1);
			return this;
		}

		public InputMap DefineKey(KeyCode key) {
			_keyMap[key] = _states.Length;
			Array.Resize(ref _states, _states.Length + 1);
			Array.Resize(ref _raw, _raw.Length + 1);
			return this;
		}

		public void Update() {
			foreach (var (name, id) in _map) {
				InputState state;
				if (UnityInput.GetButtonDown(name))
					state = InputState.Triggered;
				else if (UnityInput.GetButton(name))
					state = InputState.Pressing;
				else if (UnityInput.GetButtonUp(name))
					state = InputState.Released;
				else
					state = InputState.None;

				_states[id] = state;

				_raw[id] = UnityInput.GetAxis(name);
			}

			foreach (var (key, id) in _keyMap) {
				InputState state;
				if (UnityInput.GetKeyDown(key))
					state = InputState.Triggered;
				else if (UnityInput.GetKey(key))
					state = InputState.Pressing;
				else if (UnityInput.GetKeyUp(key))
					state = InputState.Released;
				else
					state = InputState.None;

				_states[id] = state;

				_raw[id] = state == InputState.Triggered || state == InputState.Pressing ? 1 : 0;
			}
		}

		public InputState GetState(string name) {
			if (_map.TryGetValue(name, out var id))
				return _states[id];
			return InputState.None;
		}

		public InputState GetState(KeyCode key) {
			if (_keyMap.TryGetValue(key, out var id))
				return _states[id];
			return InputState.None;
		}

		public float GetRaw(string name) {
			if (_map.TryGetValue(name, out var id))
				return _raw[id];
			return 0;
		}

		public float GetRaw(KeyCode key) {
			if (_keyMap.TryGetValue(key, out var id))
				return _raw[id];
			return 0;
		}

		public bool IsInactive(string name) => GetState(name) == InputState.None;

		public bool IsInactive(KeyCode key) => GetState(key) == InputState.None;

		public bool IsTriggered(string name) => GetState(name) == InputState.Triggered;

		public bool IsTriggered(KeyCode key) => GetState(key) == InputState.Triggered;

		public bool IsPressed(string name) => GetState(name) == InputState.Pressing;

		public bool IsPressed(KeyCode key) => GetState(key) == InputState.Pressing;

		public bool IsReleased(string name) => GetState(name) == InputState.Released;

		public bool IsReleased(KeyCode key) => GetState(key) == InputState.Released;
	}
}
