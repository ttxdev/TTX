import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { getToken } from '$lib/auth';
import type { CreatorDto } from '$lib/api';

export type Rarity = 'pennies' | 'normal' | 'rare' | 'epic';

export type CreatorBox = CreatorDto & {
	rarity_class: Rarity;
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
				case 0:
					rarity_class = 'pennies';
					break;
				case 1:
					rarity_class = 'normal';
					break;
				case 2:
					rarity_class = 'rare';
					break;
				case 3:
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
