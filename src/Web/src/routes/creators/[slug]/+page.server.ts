import { getApiClient } from '$lib';
import { getToken } from '$lib/auth';
import { error } from '@sveltejs/kit';
import type { PageServerLoad } from './$types.js';
import { CreatorDto, CreatorShareDto, CreatorTransactionDto, TimeStep } from '$lib/api.js';

export type Interval = '24h' | '12h' | '6h' | '1h'

export const load: PageServerLoad = async ({ cookies, params, url }) => {
	try {
		const channelSlug = params.slug.toLowerCase();
		const interval = (url.searchParams.get('interval') || '1h') as Interval;
		const hours = interval === '24h' ? 24 : interval === '12h' ? 12 : interval === '6h' ? 6 : 1;

		const token = getToken(cookies);
		const client = getApiClient(token || '');
		const creator = await client.getCreator(channelSlug,
			TimeStep.Minute,
			new Date(Date.now() - hours * 60 * 60 * 1000)
		);

		return {
			creator: creator.toJSON() as CreatorDto,
			shares: creator.shares.map((d) => d.toJSON() as CreatorShareDto),
			transactions: creator.transactions
  			.sort((a, b) => b.created_at.getTime() - a.created_at.getTime())
		  	.map((d) => d.toJSON() as CreatorTransactionDto),
			interval
		};
	} catch (err) {
		console.error(err);
		error(404, 'Channel not found');
	}
};
