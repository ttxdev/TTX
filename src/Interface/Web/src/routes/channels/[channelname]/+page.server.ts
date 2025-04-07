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

		return {
			creator: creator.toJSON(),
			shares: creator.shares.map((d) => d.toJSON()),
			transactions: creator.transactions.map((d) => d.toJSON())
		};
	} catch {
		error(404, 'Channel not found');
	}
};
