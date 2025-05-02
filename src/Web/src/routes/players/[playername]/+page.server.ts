import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { getToken } from '$lib/auth';
import type { PlayerTransactionDto, PlayerShareDto, PlayerDto } from '$lib/api';
import type { LinkableUser } from '$lib/types';
export const load = (async ({ cookies, params }) => {
	const token = getToken(cookies);
	const client = getApiClient(token ?? '');

	const player = await client.getPlayer(params.playername);
	const transactions = player.transactions.sort((b, a) => a.created_at.getTime() - b.created_at.getTime())
		.map((t) => t.toJSON()) as PlayerTransactionDto[];

	return {
		player: {
			...player.toJSON(),
			url: `https://www.twitch.tv/${player.name}`,
		} as LinkableUser<PlayerDto>,
		shares: player.shares.map((s) => s.toJSON()) as PlayerShareDto[],
		transactions
	};
}) satisfies PageServerLoad;
