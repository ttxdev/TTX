import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { getToken } from '$lib/auth';
import type { PlayerTransactionDto, PlayerShareDto } from '$lib/api';
import type { UserStats } from '../../+page.server';
export const load = (async ({ cookies, params }) => {
	const token = getToken(cookies);
	const client = getApiClient(token ?? '');

	const player = await client.getPlayer(params.playername);
	const transactions = player.transactions.sort((b, a) => a.created_at.getTime() - b.created_at.getTime())
		.map((t) => t.toJSON()) as PlayerTransactionDto[];

	const isStreamer = client.getCreator(player.name).then(res => res !== null)

	return {
		player: {
			...player.toJSON(),
			value: player.credits,
			url: `https://www.twitch.tv/${player.name}`,
			history: []
		} as UserStats,
		shares: player.shares.map((s) => s.toJSON()) as PlayerShareDto[],
		transactions,
		isStreamer
	};
}) satisfies PageServerLoad;
