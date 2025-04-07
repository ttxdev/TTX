import type { LayoutServerLoad } from './$types';
import { getToken, getUserData } from '$lib/auth';
import { getApiClient } from '$lib';

export const load: LayoutServerLoad = async ({ cookies }) => {
	const user = getUserData(cookies);
	const token = getToken(cookies);
	const client = getApiClient(token ?? '');

	if (!user) {
		return {
			user,
			token
		};
	}

	const self = await client.getSelf();
	const liveHoldings = self.shares.map((s) => ({
		creator: s.creator.name,
		slug: s.creator.slug,
		avatar: s.creator.avatar_url,
		isLive: s.creator.stream_status.is_live
	}));

	return {
		user,
		token,
		liveHoldings
	};
};
