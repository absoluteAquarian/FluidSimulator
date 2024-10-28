using AbsoluteCommons.Attributes;
using AbsoluteCommons.Components;
using AbsoluteCommons.Objects;
using AbsoluteCommons.Utility;
using Unity.Netcode;
using UnityEngine;

namespace FluidSimulator.Objects {
	public class FluidPool : NetworkBehaviour {
		[Header("Primary Fluid")]
		[SerializeField, ReadOnly] private DynamicObjectPool _fluidCreatorPrimary;
		[SerializeField] private GameObject _initialPrimaryFluidPrefab;
		[SerializeField] private GameObject _dispenserPrimary;
		[SerializeField, ReadOnly] private TimersTracker _dispenserPrimaryTimers;

		[Header("Secondary Fluid")]
		[SerializeField, ReadOnly] private DynamicObjectPool _fluidCreatorSecondary;
		[SerializeField] private GameObject _initialSecondaryFluidPrefab;
		[SerializeField] private GameObject _dispenserSecondary;
		[SerializeField, ReadOnly] private TimersTracker _dispenserSecondaryTimers;

		private void Awake() {
			if (!_dispenserPrimary) {
				Debug.LogError("[FluidPool] [Awake] Primary dispenser does not exist");
				goto FindSecondaryInfo;
			}

			GameObject firstFluid = _dispenserPrimary.GetChild("Pool");
			if (firstFluid) {
				_fluidCreatorPrimary = firstFluid.GetComponent<DynamicObjectPool>();
				if (!_fluidCreatorPrimary) {
					Debug.LogError("[FluidPool] [Awake] Primary dispenser DynamicObjectPool does not exist");
					goto FindPrimaryTimers;
				}

				if (_initialPrimaryFluidPrefab)
					_fluidCreatorPrimary.SetPrefab(_initialPrimaryFluidPrefab);

			FindPrimaryTimers:
				_dispenserPrimaryTimers = _dispenserPrimary.GetComponent<TimersTracker>();
				if (!_dispenserPrimaryTimers)
					Debug.LogError("[FluidPool] [Awake] Primary dispenser TimersTracker does not exist");
			}

		FindSecondaryInfo:
			if (!_dispenserSecondary) {
				Debug.LogError("[FluidPool] [Awake] Secondary dispenser does not exist");
				return;
			}

			GameObject secondFluid = _dispenserSecondary.GetChild("Pool");
			if (secondFluid) {
				_fluidCreatorSecondary = secondFluid.GetComponent<DynamicObjectPool>();
				if (!_fluidCreatorSecondary) {
					Debug.LogError("[FluidPool] [Awake] Secondary dispenser DynamicObjectPool does not exist");
					goto FindSecondaryTimers;
				}

				if (_initialSecondaryFluidPrefab)
					_fluidCreatorSecondary.SetPrefab(_initialSecondaryFluidPrefab);

			FindSecondaryTimers:
				_dispenserSecondaryTimers = _dispenserSecondary.GetComponent<TimersTracker>();
				if (!_dispenserSecondaryTimers)
					Debug.LogError("[FluidPool] [Awake] Secondary dispenser TimersTracker does not exist");
			}
		}

		public void SetPrimaryFluidPrefab(GameObject prefab) {
			if (_fluidCreatorPrimary)
				_fluidCreatorPrimary.SetPrefab(prefab);
		}

		public void SetPrimaryFluidPrefab<T>(T prefab) where T : Component {
			if (_fluidCreatorPrimary)
				_fluidCreatorPrimary.SetPrefab(prefab);
		}

		public void SetSecondaryFluidPrefab(GameObject prefab) {
			if (_fluidCreatorSecondary)
				_fluidCreatorSecondary.SetPrefab(prefab);
		}

		public void SetSecondaryFluidPrefab<T>(T prefab) where T : Component {
			if (_fluidCreatorSecondary)
				_fluidCreatorSecondary.SetPrefab(prefab);
		}

		private void Update() {
			// TODO: simulate particle densities
			// TODO: remove colliders from particles, use Job to simulate spring-like collisions (m * d^2x/dt^2 + c * dx/dt + k * x = 0)
		}
	}
}
