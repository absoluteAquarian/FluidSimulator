using UnityEngine;
using UnityEngine.Pool;

namespace AbsoluteCommons.Objects {
	public class EventObjectPool<T> where T : Object {
		public T prefab;

		private readonly ObjectPool<T> pool;

		// Events
		public event System.Action<EventObjectPool<T>, T> OnCreate;
		public event System.Action<T> OnGet;
		public event System.Action<T> OnReturn;
		public event System.Action<T> OnDestroy;

		public EventObjectPool(T prefab, int initialCapacity = 10, int maxCapacity = 1000) {
			this.prefab = prefab;
			pool = new ObjectPool<T>(Create, GetCallback, ReturnCallback, DestroyCallback, true, initialCapacity, maxCapacity);
		}

		public T Get() => pool.Get();

		public void Return(T obj) => pool.Release(obj);

		private T Create() {
			T obj = Object.Instantiate(prefab);
			OnCreate?.Invoke(this, obj);
			return obj;
		}

		private void GetCallback(T obj) => OnGet?.Invoke(obj);

		private void ReturnCallback(T obj) => OnReturn?.Invoke(obj);

		private void DestroyCallback(T obj) {
			OnDestroy?.Invoke(obj);
			Object.Destroy(obj);
		}
	}
}
