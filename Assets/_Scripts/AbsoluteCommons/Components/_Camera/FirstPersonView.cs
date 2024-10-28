using AbsoluteCommons.Input;
using System;
using UnityEngine;

// Code was derived from: https://stackoverflow.com/questions/8465323/unity-fps-rotation-camera

namespace AbsoluteCommons.Components {
	[AddComponentMenu("Camera-Control/FirstPersonView")]
	public class FirstPersonView : MonoBehaviour {
		public enum Axes {
			MouseXAndY,
			MouseX,
			MouseY
		}

		public readonly struct Direction {
			private readonly int _horizontal, _vertical;

			public int Horizontal => _horizontal;
			public int Vertical => _vertical;

			public Direction(int horizontal, int vertical) {
				_horizontal = horizontal;
				_vertical = vertical;
			}
		}

		public Axes axes = Axes.MouseXAndY;
		public float sensitivityX = 15f;
		public float sensitivityY = 15f;

		public bool flipVerticalMovement;

		public float minimumY = -60f;  // Vertical rotation
		public float maximumY = 60f;

		float rotationX = 0f;
		float rotationY = 0f;

		private int _rotationDirectionHorizontal;
		private int _rotationDirectionVertical;

		public Vector3 ViewRotation => new Vector3(-rotationX, rotationY, 0);
		public Direction RotationDirection => new Direction(_rotationDirectionHorizontal, _rotationDirectionVertical);

		// Start is called before the first frame update
		void Start() {
			if (TryGetComponent<Rigidbody>(out var body))
				body.freezeRotation = true;
		}

		private bool _lockedCamera = false;

		public bool IsLocked => _lockedCamera;

		private bool _lockedButNotReally;

		// Update is called once per frame
		void Update() {
			if (InputMapper.IsTriggered(KeyCode.Escape))
				ToggleLock();

			Cursor.visible = !_lockedCamera;

			if (!_lockedCamera || _lockedButNotReally) {
				_rotationDirectionHorizontal = 0;
				_rotationDirectionVertical = 0;
				return;
			}

			switch (axes) {
				case Axes.MouseXAndY:
					SetRotationX();
					SetRotationY();
					break;
				case Axes.MouseX:
					SetRotationY();
					break;
				case Axes.MouseY:
					SetRotationX();
					break;
			}
		}

		public void SetLocked(bool locked) {
			if (locked == _lockedCamera)
				return;

			_lockedCamera = locked;
			Cursor.lockState = _lockedCamera ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = !_lockedCamera;
		}

		public void ToggleLock() => SetLocked(!_lockedCamera);

		public void ForceLock() => _lockedButNotReally = true;

		public void ReleaseForcedLock() => _lockedButNotReally = false;

		private void SetRotationX() {
			float rotation = InputMapper.GetRaw("Mouse Y") * sensitivityY;

			if (flipVerticalMovement)
				rotation = -rotation;

			_rotationDirectionVertical = Math.Sign(rotation);

			rotationX += rotation;
			rotationX = Mathf.Clamp(rotationX, minimumY, maximumY);
		}

		private void SetRotationY() {
			float rotation = InputMapper.GetRaw("Mouse X") * sensitivityX;

			_rotationDirectionHorizontal = Math.Sign(rotation);

			rotationY += rotation;
			rotationY %= 360;
		}

		public void ClearRotations(bool horizontal = true, bool vertical = true) {
			if (horizontal)
				rotationY = 0;
			if (vertical)
				rotationX = 0;
		}

		public Snapshot CreateSnapshot() => new Snapshot(this);

		public void RestoreFromSnapshot(Snapshot snapshot) {
			axes = snapshot.axes;
			sensitivityX = snapshot.sensitivityX;
			sensitivityY = snapshot.sensitivityY;
			flipVerticalMovement = snapshot.flipVerticalMovement;
			minimumY = snapshot.minimumY;
			maximumY = snapshot.maximumY;
			rotationX = snapshot.rotationX;
			rotationY = snapshot.rotationY;
		}

		public readonly struct Snapshot {
			public readonly Axes axes;
			public readonly float sensitivityX, sensitivityY;
			public readonly bool flipVerticalMovement;
			public readonly float minimumY, maximumY;
			public readonly float rotationX, rotationY;

			public Snapshot(FirstPersonView fpv) {
				axes = fpv.axes;
				sensitivityX = fpv.sensitivityX;
				sensitivityY = fpv.sensitivityY;
				flipVerticalMovement = fpv.flipVerticalMovement;
				minimumY = fpv.minimumY;
				maximumY = fpv.maximumY;
				rotationX = fpv.rotationX;
				rotationY = fpv.rotationY;
			}
		}
	}
}
