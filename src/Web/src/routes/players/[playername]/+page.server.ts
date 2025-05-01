import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { getToken } from '$lib/auth';
import type { UserStats } from '../../+page.server';

export const load: PageServerLoad = (async ({ cookies, params }) => {
	const token = getToken(cookies);
	const client = getApiClient(token ?? '');

	const player = await client.getPlayer(params.playername);

	return {
		player: {
			...player,
			value: player.credits,
			ticker: '',
			isLive: false,
			history: []
		} as UserStats,
		shares: player.shares,
		transactions: player.transactions
	};
});
