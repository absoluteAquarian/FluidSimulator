using AbsoluteCommons.Attributes;
using Unity.Netcode;
using UnityEngine;

namespace AbsoluteCommons.Objects {
	public class PooledObject : NetworkBehaviour {
		public DynamicObjectPool Pool {
			get => _pool;
			private set => _pool = value;
		}

		public int Index {
			get => _index;
			private set => _index = value;
		}

		[SerializeField, ReadOnly] private DynamicObjectPool _pool;
		[SerializeField, ReadOnly] private int _index = -1;

		public static void EnsureConnection(GameObject obj, DynamicObjectPool pool, int index) {
			if (!obj.TryGetComponent(out PooledObject component))
				component = obj.AddComponent<PooledObject>();

			component._pool = pool;
			component._index = index;

			if (component.IsOwner)
				component.EnsureConnectionServerRpc(obj, pool.gameObject, index);
		}

		[ServerRpc]
		private void EnsureConnectionServerRpc(NetworkObjectReference pooledObjectRef, NetworkObjectReference poolRef, int index) {
			GameObject pooledObj = pooledObjectRef;
			if (!pooledObj)
				return;

			GameObject pool = poolRef;
			if (!pool)
				return;

			EnsureConnection(pooledObj, pool.GetComponent<DynamicObjectPool>(), index);

			EnsureConnectionClientRpc(pooledObjectRef, poolRef, index);
		}

		[ClientRpc]
		private void EnsureConnectionClientRpc(NetworkObjectReference pooledObjectRef, NetworkObjectReference poolRef, int index) {
			if (IsOwner)
				return;

			GameObject pooledObj = pooledObjectRef;
			if (!pooledObj)
				return;

			GameObject pool = poolRef;
			if (!pool)
				return;

			EnsureConnection(pooledObj, pool.GetComponent<DynamicObjectPool>(), index);
		}

		public void ReturnToPool() {
			Pool.Return(gameObject);
			_index = -1;
		}

		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
			if (serializer.IsWriter) {
				var writer = serializer.GetFastBufferWriter();

				if (_pool) {
					writer.WriteValueSafe(true);
					writer.WriteValueSafe(_pool.NetworkObjectId);
				} else
					writer.WriteValueSafe(false);

				writer.WriteValueSafe(_index);
			} else {
				var reader = serializer.GetFastBufferReader();

				reader.ReadValueSafe(out bool hasPool);

				if (hasPool) {
					reader.ReadValueSafe(out ulong poolId);
					_pool = NetworkManager.Singleton.SpawnManager.SpawnedObjects[poolId].GetComponent<DynamicObjectPool>();
				} else
					_pool = null;

				reader.ReadValueSafe(out _index);
			}

			base.OnSynchronize(ref serializer);
		}

		public override void OnNetworkDespawn() {
			if (_index >= 0)
				ReturnToPool();

			base.OnNetworkDespawn();
		}
	}
}
