import type { LayoutServerLoad } from './$types';
import { getApiClient } from '$lib';
import { redirect } from '@sveltejs/kit';
import { getToken, logout } from '$lib/auth/sessions';

export const load: LayoutServerLoad = async ({ cookies, url }) => {
	const query = url.searchParams;

	if (query.has('frame_id')) {
		return redirect(307, '/discord');
	}

	const token = getToken(cookies);
	const client = getApiClient(token ?? '');

	if (!token) {
		return;
	}

	try {
		const self = await client.getSelf();
		const liveHoldings = self.shares.map((s) => ({
			creator: s.creator.name,
			slug: s.creator.slug,
			avatar: s.creator.avatar_url,
			isLive: s.creator.stream_status.is_live
		}));

    const unOpenedBoxes = self.loot_boxes.filter(b => !b.is_open).length;
		const drawerData = Promise.all([liveHoldings, unOpenedBoxes])

		return {
      user: self.toJSON(),
			token,
			drawerData
		};
	} catch {
		logout(cookies);
	}
};
