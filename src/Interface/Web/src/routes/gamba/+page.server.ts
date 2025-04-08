import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { getToken } from '$lib/auth';
import { Rarity, type CreatorDto } from '$lib/api';

export type RarityClass = 'pennies' | 'normal' | 'rare' | 'epic';

export type CreatorBox = CreatorDto & {
	rarity_class: RarityClass;
};

export const load: PageServerLoad = async ({ cookies, depends }) => {
	depends('gamba');
	const client = getApiClient(getToken(cookies) ?? '');
	const gamba = await client.gamba();

	const mappedChannels = gamba.rarities
		.sort(() => Math.random() - 0.5)
		.map<CreatorBox>((channel) => {
			let rarity_class;
			switch (channel.rarity) {
				case Rarity.Pennies:
					rarity_class = 'pennies';
					break;
				case Rarity.Common:
					rarity_class = 'normal';
					break;
				case Rarity.Rare:
					rarity_class = 'rare';
					break;
				case Rarity.Epic:
					rarity_class = 'epic';
					break;
			}

			return {
				...channel.toJSON(),
				rarity_class
			};
		});

	// Repeat the choices 3 times to make the spin **cooler** (maybe redundant)
	const choices = [...new Array(2)].flatMap(() => mappedChannels);
	const winnerIndex = Math.floor(Math.random() * mappedChannels.length) + mappedChannels.length;
	return {
		choices,
		winnerIndex
	};
};
