import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import {
	CreatorOrderBy,
	OrderDirection,
	PlayerDto,
	PlayerOrderBy,
	type CreatorPartialDto
} from '$lib/api';
import type { LinkableUser } from '$lib/types';

export const load: PageServerLoad = async () => {
	const client = getApiClient('');
	const featuredCreator = await client.getCreator('dougdoug').then((channel) => channel.toJSON());
	const featuredCreators = await client
		.getCreators(1, 3, undefined, CreatorOrderBy.IsLive, OrderDirection.Descending)
		.then((creators) =>
			creators.data.map<LinkableUser<CreatorPartialDto>>((creator: CreatorPartialDto) => ({
  			...creator.toJSON(),
				url: `/creators/${creator.slug}`
			}))
		);
	const topCreators = await client
		.getCreators(1, 3, undefined, CreatorOrderBy.Value, OrderDirection.Descending)
		.then((creators) =>
			creators.data.map<LinkableUser<CreatorPartialDto>>((creator: CreatorPartialDto) => ({
  			...creator.toJSON(),
				url: `/creators/${creator.slug}`
			}))
		);
	const topPlayers = await client
		.getPlayers(1, 3, undefined, PlayerOrderBy.Portfolio, OrderDirection.Descending)
		.then((users) =>
			users.data.map<LinkableUser<PlayerDto>>((player) => ({
  			...player.toJSON(),
				url: '/players/' + player.slug,
			}))
		);

	return {
		featuredCreator,
		featuredCreators,
		topCreators,
		topPlayers
	};
};
