import type { LayoutServerLoad } from './$types';
import { getToken, getUserData, logout } from '$lib/auth';
import { getApiClient } from '$lib';
import { redirect } from '@sveltejs/kit';

export const load: LayoutServerLoad = async ({ cookies, url }) => {
	const query = url.searchParams;

	if (query.has('frame_id')) {
		return redirect(307, '/discord');
	}

	const user = getUserData(cookies);
	const token = getToken(cookies);
	const client = getApiClient(token ?? '');

	if (!user) {
		return {
			user,
			token
		};
	}

	try {
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
	} catch {
		logout(cookies);
		return {
			user,
			token
		};
	}
};
