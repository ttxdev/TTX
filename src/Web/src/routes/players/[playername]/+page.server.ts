import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { error } from '@sveltejs/kit';
import type { PlayerTransactionDto, PlayerShareDto, PlayerDto } from '$lib/api';
import type { LinkableUser } from '$lib/types';

export const load = (async ({ params }) => {
  try {
    const client = getApiClient('');
  	const player = await client.getPlayer(params.playername);
  	const transactions = player.transactions.sort((b, a) => a.created_at.getTime() - b.created_at.getTime())
  		.map((t) => t.toJSON()) as PlayerTransactionDto[];
    const isStreamer = client.getCreator(player.slug).then(() => true).catch(() => false);

  	return {
  		player: {
  			...player.toJSON(),
  			url: `https://www.twitch.tv/${player.slug}`,
  		} as LinkableUser<PlayerDto>,
  		shares: player.shares.map((s) => s.toJSON()) as PlayerShareDto[],
  		transactions,
  		isStreamer
  	};
  } catch (err) {
    console.error(err);
		throw error(404, 'Player not found');
  }
}) satisfies PageServerLoad;
