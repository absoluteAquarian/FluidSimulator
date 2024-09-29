using AbsoluteCommons.Attributes;
using AbsoluteCommons.Collections;
using AbsoluteCommons.Objects;
using System.Collections;
using System.Collections.Concurrent;
using Unity.Netcode;
using UnityEngine;

namespace AbsoluteCommons.Components {
	// Code was derived from:  https://forums.unity.com/threads/need-advice-on-making-high-speed-bullet-trails-with-raycasting.1211583/
	[RequireComponent(typeof(DynamicObjectPool))]
	public class BulletTrailHandler : NetworkBehaviour {
		[SerializeField] private TrailRenderer _trailPrefab;
		[SerializeField] private float _fakeBulletSpeed = 1f;

		[SerializeField, ReadOnly] private DynamicObjectPool _trailPool;
		private WaitForSeconds _destroyDelay;

		// Used to force any leftover trails to despawn when the object is being destroyed
		private FreeList<TrailRenderer> _activeTrails = new();
		private SparseSet _knownActiveIndices = new(16);
		private ConcurrentBag<int> _queuedTrailRemovals = new();

		private void Awake() {
			_trailPool = GetComponent<DynamicObjectPool>();

			_destroyDelay = new WaitForSeconds(_trailPrefab.time);
		}

		private void LateUpdate() {
			CheckRemovalQueue();
		}

		public void CreateTrail(Ray ray, LayerMask collisionMask, float distance = 100f) {
			if (Physics.Raycast(ray, out RaycastHit hit, distance, collisionMask))
				CreateTrail(ray.origin, hit);
			else
				CreateTrail(ray.origin, ray.direction, distance);
		}

		public void CreateTrail(Vector3 spawnPosition, Vector3 end) => InternalCreateTrail(spawnPosition, end);

		public void CreateTrail(Vector3 spawnPosition, Vector3 forward, float distance = 100f) => InternalCreateTrail(spawnPosition, spawnPosition + forward * distance);

		public void CreateTrail(Vector3 spawnPosition, RaycastHit hit) => InternalCreateTrail(spawnPosition, hit.point);

		private void InternalCreateTrail(Vector3 start, Vector3 end) {
			// Ensure that the prefab is set
			if (!_trailPrefab) {
				Debug.LogError("BulletTrailHandler: No trail prefab set");
				return;
			}

			_trailPool.SetPrefab(_trailPrefab);

			if (base.IsOwner)
				InternalCreateTrailServerRpc(start, end);
		}

		[ServerRpc]
		private void InternalCreateTrailServerRpc(Vector3 start, Vector3 end) {
			TrailRenderer trail = _trailPool.Get<TrailRenderer>();
			if (!trail)
				return;

			trail.transform.SetPositionAndRotation(start, Quaternion.identity);

			trail.Clear();

			trail.enabled = true;

			int index = _activeTrails.Insert(trail);
			_knownActiveIndices.Add(index);

			StartCoroutine(SpawnTrailBits(trail, end, index));
		}

		private IEnumerator SpawnTrailBits(TrailRenderer trail, Vector3 end, int activeIndex) {
			Vector3 start = trail.transform.position;
			float distance = Vector3.Distance(start, end);
			float remainingDistance = distance;

			while (remainingDistance > 0) {
				trail.transform.position = Vector3.Lerp(start, end, 1 - remainingDistance / distance);
				remainingDistance -= _fakeBulletSpeed * Time.deltaTime;
				yield return null;
			}

			trail.transform.position = end;

			yield return _destroyDelay;

			trail.enabled = false;
			trail.gameObject.GetComponent<PooledObject>().ReturnToPool();

			_queuedTrailRemovals.Add(activeIndex);
		}

		public override void OnDestroy() {
			// Forcibly despawn any remaining trails
			foreach (int index in _knownActiveIndices)
				_queuedTrailRemovals.Add(index);

			CheckRemovalQueue();

			base.OnDestroy();
		}

		private void CheckRemovalQueue() {
			while (_queuedTrailRemovals.TryTake(out int index)) {
				TrailRenderer trail = _activeTrails[index];

				if (trail) {
					trail.enabled = false;
					trail.gameObject.GetComponent<PooledObject>().ReturnToPool();
				}

				_activeTrails.Remove(index);
				_knownActiveIndices.Remove(index);
			}
		}
	}
}
