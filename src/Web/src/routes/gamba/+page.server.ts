import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { fail, redirect, type Actions } from '@sveltejs/kit';
import { type CreatorPartialDto, type CreatorRarityDto, type LootBoxDto } from '$lib/api';
import { getToken } from '$lib/auth/sessions';


export const load: PageServerLoad = async ({ cookies }) => {
	const client = getApiClient(getToken(cookies) ?? '');

	try {
		const user = await client.getSelf();
		if (!user) {
			throw redirect(307, '/');
		}

		const unopenedBoxes = user.loot_boxes
			.filter((box) => !box.is_open)
			.map((box) => box.toJSON()) as LootBoxDto[];

		const openedBoxes = user.loot_boxes
			.filter((box) => box.is_open)
			.map((box) => box.toJSON()) as LootBoxDto[];

		return {
			unopenedBoxes,
			openedBoxes
		};
	} catch {
		throw redirect(307, '/');
	}
};

export const actions: Actions = {
	openLootbox: async ({ cookies, request }) => {
		const client = getApiClient(getToken(cookies) ?? '');
		const data = await request.formData();
		const lootboxId = data.get('lootboxId');
		if (!lootboxId) {
			return fail(400, { message: 'Lootbox ID is required' });
		}

		const parsedLootboxId = parseInt(lootboxId as string);
		if (isNaN(parsedLootboxId)) {
			return fail(400, { message: 'Invalid lootbox ID' });
		}

		try {
			const res = await client.gamba(parsedLootboxId);

			const rarities = res.rarities.map((r) => r.toJSON()) as CreatorRarityDto[];
			const dupedRarities = [...rarities, ...rarities];
			const winnerIndex = rarities.findIndex((r) => r.creator.id === res.result.creator.id) + rarities.length;
			return {
				winner: res.result.creator.toJSON() as CreatorPartialDto,
				rarities: dupedRarities,
				winnerIndex
			};
		} catch {
			return fail(400, { message: 'Failed to open lootbox' });
		}
	}
};
