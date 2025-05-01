import { writable } from 'svelte/store';

type CreatorModel = {
	creator_id: number;
};

export function createCreatorStore<T extends CreatorModel>() {
	const store = writable<Map<number, T[]>>(new Map());

	function add(model: T) {
		store.update((store) => {
			const creator = store.get(model.creator_id) || [];
			creator.push(model);
			store.set(model.creator_id, creator);
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
