import { writable } from 'svelte/store';

export function createOwnedStore<T>(
  toId: (model: T) => number
) {
	const store = writable<Map<number, T[]>>(new Map());

	function add(model: T) {
		store.update((store) => {
		  const id = toId(model)
			const owner = store.get(id) || [];
			owner.push(model);
			store.set(id, owner);
			return store;
		});
	}

	function set(id: number, models: T[]) {
		store.update((store) => {
			store.set(id, models);
			return store;
		});
	}

	return {
		store,
		add,
		set
	};
}
