import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { getToken } from '$lib/auth';

export const load = (async ({ cookies, params }) => {
	const token = getToken(cookies);
	const client = getApiClient(token ?? '');

	const player = await client.getPlayer(params.playername);

	return {
		player: {
			...player.toJSON(),
			value: player.credits,
			url: `https://www.twitch.tv/${player.name}`,
			history: []
		},
		shares: player.shares.map((s) => s.toJSON()),
		transactions: player.transactions.map((t) => t.toJSON())
	};
}) satisfies PageServerLoad;
