import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { getToken } from '$lib/auth';
import {
	CreatorOrderBy,
	OrderDirection,
	PlayerOrderBy,
	type VoteDto,
	type CreatorPartialDto
} from '$lib/api';

export type UserStats = {
	avatar_url: string;
	name: string;
	ticker: string;
	slug: string;
	value: number;
	isLive: boolean;
	history: VoteDto[];
	url: string;
};

export const load: PageServerLoad = async ({ cookies }) => {
	const client = getApiClient(getToken(cookies) ?? '');
	const featuredCreator = await client.getCreator('dougdoug');
	const featuredCreators = await client
		.getCreators(1, 3, undefined, CreatorOrderBy.IsLive, OrderDirection.Descending)
		.then((creators) =>
			creators.data.map<UserStats>((channel: CreatorPartialDto) => ({
				avatar_url: channel.avatar_url,
				name: channel.name,
				ticker: channel.ticker,
				slug: channel.slug,
				value: channel.value,
				isLive: channel.stream_status.is_live ?? false,
				history: channel.history,
				url: `/creators/${channel.slug}`
			}))
		);
	const topCreators = await client
		.getCreators(1, 3, undefined, CreatorOrderBy.Value, OrderDirection.Descending)
		.then((creators) =>
			creators.data.map<UserStats>((channel: CreatorPartialDto) => ({
				avatar_url: channel.avatar_url,
				name: channel.name,
				ticker: channel.ticker,
				slug: channel.slug,
				value: channel.value,
				isLive: channel.stream_status.is_live ?? false,
				history: channel.history,
				url: `/creators/${channel.slug}`
			}))
		);
	const topPlayers = await client
		.getPlayers(1, 3, undefined, PlayerOrderBy.Credits, OrderDirection.Descending)
		.then((users) =>
			users.data.map<UserStats>((user) => ({
				name: user.name,
				value: user.credits, // TODO: calculate user's value on backend
				avatar_url: user.avatar_url,
				slug: user.name,
				ticker: '',
				url: '/players/' + user.slug,
				isLive: false,
				history: []
			}))
		);

	return {
		featuredCreator,
		featuredCreators,
		topCreators,
		topPlayers
	};
};
