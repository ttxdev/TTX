import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { getToken } from '$lib/auth';
import {
	Order,
	OrderDirection,
	type CreatorDto,
	type CreatorPartialDto,
	type Vote
} from '$lib/api';

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
	const featuredCreators = await client
		.getCreators(1, 3, undefined, undefined, [
			new Order({
				by: 'Name',
				dir: OrderDirection.Descending
			})
		])
		.then((creators) =>
			creators.data.map<UserStats>((channel: CreatorPartialDto) => ({
				avatar_url: channel.avatar_url,
				name: channel.name,
				ticker: channel.ticker,
				slug: channel.slug,
				value: channel.value,
				isLive: channel.stream_status.is_live ?? false,
				history: channel.history.map((v) => v.toJSON()) ?? [],
				url: `/channels/${channel.slug}`
			}))
		);
	const topCreators = await client
		.getCreators(1, 3, undefined, undefined, [
			new Order({
				by: 'Value',
				dir: OrderDirection.Descending
			})
		])
		.then((creators) =>
			creators.data.map<UserStats>((channel: CreatorPartialDto) => ({
				avatar_url: channel.avatar_url,
				name: channel.name,
				ticker: channel.ticker,
				slug: channel.slug,
				value: channel.value,
				isLive: channel.stream_status.is_live ?? false,
				history: channel.history.map((v) => v.toJSON()) ?? [],
				url: `/channels/${channel.slug}`
			}))
		);
	const topPlayers = await client
		.getUsers(1, 3, undefined, undefined, [
			new Order({
				by: 'Credits',
				dir: OrderDirection.Descending
			})
		])
		.then((users) =>
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
		topPlayers
	};
};
