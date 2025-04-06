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

	const liveHoldings = client.getUserShares(user.name).then((s) =>
	s.map((s) => ({
			creator: s.creator.name,
			slug: s.creator.slug,
			avatar: s.creator.avatar_url,
			isLive: s.creator.is_live
		}))
	);

	return {
		user,
		token,
		liveHoldings
	};
};
