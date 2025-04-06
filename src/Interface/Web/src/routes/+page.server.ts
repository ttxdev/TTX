import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { getToken } from '$lib/auth';
import type { CreatorDto, Vote } from '$lib/api';

export type UserStats = {
	avatar_url: string;
	name: string;
	ticker: string;
	slug: string;
	value: number;
	isLive: boolean;
	history: Vote[];
	url: string;
};

export const load: PageServerLoad = async ({ cookies }) => {
	const client = getApiClient(getToken(cookies) ?? '');
	const featuredCreator = await client.getCreator('dougdoug').then((channel) => channel.toJSON());
	const featuredCreators = await client.getCreators(1, 3, 'IsLive', 'desc').then((creators) =>
		creators.data.map<UserStats>((channel: CreatorDto) => ({
			...channel.toJSON(),
			url: `/channels/${channel.slug}`,
			isLive: channel.is_live
		}))
	);
	const topCreators = await client.getCreators(1, 3, 'Value', 'desc').then((creators) =>
		creators.data.map<UserStats>((channel: CreatorDto) => ({
			...channel.toJSON(),
			url: `/channels/${channel.slug}`,
			isLive: channel.is_live
		}))
	);
	const topPlayers = await client.getUsers(1, 3, 'Credits', 'desc').then((users) =>
		users.data.map<UserStats>((user) => ({
			name: user.name,
			value: user.credits, // TODO: calculate user's value on backend
			avatar_url: user.avatar_url,
			slug: user.name,
			ticker: '',
			url: '/players/' + user.name,
			isLive: false,
			history: []
		}))
	);

	return {
		featuredCreator,
		featuredCreators,
		topCreators,
		topPlayers,
	};
};
