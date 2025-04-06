import { getApiClient } from '$lib';
import { getToken } from '$lib/auth';
import { error } from '@sveltejs/kit';
import type { PageServerLoad } from './$types.js';
import { CreatorDto, TimeStep, Vote } from '$lib/api.js';

/** @type {import('./$types').PageServerLoad} */
export const load: PageServerLoad = async ({ cookies, params }) => {
	try {
		const channelSlug = params.channelname.toLowerCase();

		const token = getToken(cookies);
		const client = getApiClient(token || '');
		const creator = await client.getCreator(channelSlug);
		const history = await client.getCreatorValueHistory(channelSlug, TimeStep._0);

		return {
			creator: {
				...creator.toJSON(),
				history: history.history.map((d) => d.toJSON()) as Vote[]
			},
			holders: await client.getCreatorShares(channelSlug).then((r) => r.map((h) => h.toJSON())),
			transactions: await client
				.getCreatorTransactions(channelSlug)
				.then((r) => r.data.map((t) => t.toJSON()))
		};
	} catch {
		error(404, 'Channel not found');
	}
};
