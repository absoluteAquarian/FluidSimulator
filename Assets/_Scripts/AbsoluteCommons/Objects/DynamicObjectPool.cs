using AbsoluteCommons.Attributes;
using AbsoluteCommons.Utility;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace AbsoluteCommons.Objects {
	[AddComponentMenu("Absolute Commons/Objects/Dynamic Object Pool")]
	public class DynamicObjectPool : NetworkBehaviour {
		[SerializeField, ReadOnly] private GameObject _prefab;
		[SerializeField] private int _initialCapacity = 10;
		[SerializeField] private bool _showPoolInHierarchy = false;

		[SerializeField, ReadOnly] private List<GameObject> _pool;
		private BitArray _dirty;
		private GameObject _container;
		private int _index;

		private static GameObject _visibleObjectContainer;

		private void Awake() {
			_pool = new List<GameObject>(_initialCapacity);
			_dirty = new BitArray(_initialCapacity, true);
		}

		public override void OnNetworkSpawn() {
			if (base.IsServer) {
				// Instantiate the prefab at "Assets/Prefabs/Dynamic Pool.prefab"
				_container = Instantiate(Resources.Load("Prefabs/Dynamic Pool")) as GameObject;
				_container.hideFlags = _showPoolInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy;
				_container.GetComponent<NetworkObject>().Spawn(true);
				
				_container.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
				_container.transform.SetParent(transform, false);

				// Make a global pool for objects in the world
				// This is just so they don't clutter the scene list
				if (!_visibleObjectContainer) {
					_visibleObjectContainer = Instantiate(Resources.Load("Prefabs/Visible Dynamic Pool Objects")) as GameObject;
					_visibleObjectContainer.hideFlags = _showPoolInHierarchy ? HideFlags.None : HideFlags.HideInHierarchy;
					_visibleObjectContainer.GetComponent<NetworkObject>().Spawn(true);

					_visibleObjectContainer.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
				}

				ContainerSpawnClientRpc(_container, _visibleObjectContainer);
			}

			base.OnNetworkSpawn();
		}

		[ClientRpc]
		private void ContainerSpawnClientRpc(NetworkObjectReference containerRef, NetworkObjectReference visibleObjectContainerRef) {
			_container = containerRef;
			if (!_container)
				Debug.LogError("[DynamicObjectPool] [ContainerSpawnClientRpc] Read object reference for _container is null");

			_visibleObjectContainer = visibleObjectContainerRef;
			if (!_visibleObjectContainer)
				Debug.LogError("[DynamicObjectPool] [ContainerSpawnClientRpc] Read object reference for _visibleObjectContainer is null");
		}

		public void SetPrefab(GameObject prefab) {
			// Dirty flags do not need to be set if the prefab is the same
			if (_prefab == prefab)
				return;

			_prefab = prefab;
			_dirty.SetAll(true);
		}

		public void SetPrefab<T>(T prefab) where T : Component => SetPrefab(prefab.gameObject);

		public GameObject Get() {
			if (!base.IsServer) {
				Debug.LogError("[DynamicObjectPool] Get() can only be called on the server");
				return null;
			}

			if (_prefab == null) {
				Debug.LogError("[DynamicObjectPool] No prefab set");
				return null;
			}

			for (int i = 0; i < _pool.Count; i++) {
				_index = (_index + 1) % _pool.Count;

				GameObject obj = _pool[_index];
				if (!obj || !obj.activeInHierarchy || (obj.TryGetComponent(out NetworkObject netObj) && !netObj.IsSpawned))
					return PrepareObject(obj);
			}

			// Reached capacity, create a new object
			_index = _pool.Count;
			return PrepareObject(AddNewObject());
		}

		private GameObject PrepareObject(GameObject obj) {
			bool hasNewObject = false;

			if (!obj || _dirty[_index]) {
				// A new object is needed since the prefab has changed or the object was destroyed
				if (obj)
					obj.GetComponent<NetworkObject>().SmartDespawn(true);

				obj = Create();

				_dirty[_index] = false;

				hasNewObject = true;
			}

			NetworkObject netObj = obj.GetComponent<NetworkObject>();
			netObj.SmartSpawn(true, false);

			obj.SetActive(true);
			obj.transform.SetParent(Application.isEditor ? _visibleObjectContainer.transform : null, true);

			netObj.SyncHierarchy(true);

			if (hasNewObject) {
				// Inform clients that the object has changed
				// NOTE: NetworkObjectReference can only be used for spawned objects, hence why the RPC is delayed to here
				AddNewObjectClientRpc(obj, _index);
			}

			PrepareObjectClientRpc(_index);

			PooledObject.EnsureConnection(obj, this, _index);

			return obj;
		}

		[ClientRpc]
		private void PrepareObjectClientRpc(int index) {
			GameObject obj = _pool[index];

			obj.SetActive(true);
		}

		private GameObject AddNewObject() {
			GameObject newObj = Create();
			_pool.Add(newObj);
			_dirty.Length++;
			_dirty[_index] = false;
			return newObj;
		}

		[ClientRpc]
		private void AddNewObjectClientRpc(NetworkObjectReference newObjRef, int index) {
			// Dereference will be null if the object doesn't exist
			GameObject obj = newObjRef;
			if (!obj)
				return;

			if (index < _pool.Count)
				_pool[index] = obj;
			else
				_pool.Add(obj);

			if (_dirty.Length <= index)
				_dirty.Length = index + 1;

			_dirty[index] = false;
		}

		private GameObject Create() {
			GameObject obj = Instantiate(_prefab, _container.transform, false);

			if (!obj.TryGetComponent(out NetworkObject _)) {
				Debug.LogError("[DynamicObjectPool] Prefab does not have a NetworkObject component");
				return null;
			}

			return obj;
		}

		[ServerRpc]
		private void ResetObjectStateServerRpc(int index) {
			ResetObjectState(_pool[index]);
		}

		private void ResetObjectState(GameObject obj) {
			/*
			NetworkObject netObj = obj.GetComponent<NetworkObject>();
			if (netObj.IsSpawned)
				netObj.Despawn(false);
			*/

			if (!obj)
				return;

			obj.SetActive(false);
			obj.transform.SetParent(_container.GetComponent<NetworkObject>().IsSpawned ? _container.transform : null, true);
		}

		public T Get<T>() where T : Component {
			GameObject obj = Get();
			return obj ? obj.GetComponent<T>() : null;
		}

		public void Return(GameObject obj) {
			obj.SetActive(false);

			if (!base.IsServer && base.IsOwner)
				ResetObjectStateServerRpc(_pool.IndexOf(obj));
		}

		public void Return<T>(T component) where T : Component => Return(component.gameObject);

		public override void OnNetworkDespawn() {
			// Force all objects to despawn
			if (base.IsServer) {
				foreach (GameObject obj in _pool) {
					ResetObjectState(obj);

					if (obj)
						obj.GetComponent<NetworkObject>().SmartDespawn(true);
				}
			}
		}

		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
			if (serializer.IsWriter) {
				var writer = serializer.GetFastBufferWriter();

				writer.WriteValueSafe(new NetworkObjectReference(_container));
				writer.WriteValueSafe(new NetworkObjectReference(_visibleObjectContainer));

				writer.WriteValueSafe(_pool.Count);

				using (var bitWriter = writer.EnterBitwiseContext()) {
					bitWriter.TryBeginWriteBits(_pool.Count * 2);

					for (int i = 0; i < _pool.Count; i++) {
						bitWriter.WriteBit(_dirty[i]);
						bitWriter.WriteBit(_pool[i]);
					}
				}

				foreach (GameObject obj in _pool) {
					if (obj)
						writer.WriteValueSafe(new NetworkObjectReference(obj));
				}

				writer.WriteValueSafe(_index);
			} else {
				var reader = serializer.GetFastBufferReader();

				reader.ReadValueSafe(out NetworkObjectReference containerRef);
				reader.ReadValueSafe(out NetworkObjectReference visibleObjectContainerRef);

				_container = containerRef;
				if (!_container)
					Debug.LogError("[DynamicObjectPool] [OnSynchronize] Read object reference for _container is null");

				_visibleObjectContainer = visibleObjectContainerRef;
				if (!_visibleObjectContainer)
					Debug.LogError("[DynamicObjectPool] [OnSynchronize] Read object reference for _visibleObjectContainer is null");

				reader.ReadValueSafe(out int poolCount);
				_pool = new List<GameObject>(poolCount);
				_dirty = new BitArray(poolCount, false);

				BitArray exists = new BitArray(poolCount);
				using (var bitReader = reader.EnterBitwiseContext()) {
					bitReader.TryBeginReadBits((uint)poolCount * 2);

					for (int i = 0; i < poolCount; i++) {
						bitReader.ReadBit(out bool dirty);
						_dirty[i] = dirty;
						bitReader.ReadBit(out bool existsBit);
						exists[i] = existsBit;
					}
				}

				for (int i = 0; i < poolCount; i++) {
					if (exists[i]) {
						reader.ReadValueSafe(out NetworkObjectReference objRef);
						_pool.Add(objRef);
					} else
						_pool.Add(null);
				}

				reader.ReadValueSafe(out _index);
			}
		}
	}
}
