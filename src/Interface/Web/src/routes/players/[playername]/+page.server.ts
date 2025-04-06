import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { getToken } from '$lib/auth';

export const load = (async ({ cookies, params }) => {
	const token = getToken(cookies);
	const client = getApiClient(token ?? '');

	const player = await client.getUser(params.playername);

	return {
		player: {
			...player.toJSON(),
			value: player.credits,
			url: `https://www.twitch.tv/${player.name}`,
			history: []
		},
		shares: await client.getUserShares(params.playername).then((s) => s.map((s) => s.toJSON())),
		transactions: await client
			.getUserTransactions(params.playername)
			.then((t) => t.map((t) => t.toJSON()))
	};
}) satisfies PageServerLoad;
