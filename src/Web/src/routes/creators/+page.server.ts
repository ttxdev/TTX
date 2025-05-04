import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { CreatorOrderBy, OrderDirection } from '$lib/api';

export const load: PageServerLoad = async ({ url }) => {
	const client = getApiClient('');
	const page = Number(url.searchParams.get('page')) || 1;
	const orderBy = url.searchParams.get('orderBy') || 'Name';
	const orderDir =
		url.searchParams.get('order') == 'desc' ? OrderDirection.Descending : OrderDirection.Ascending;
	const search = url.searchParams.get('search') || '';

	let safeOrderBy: CreatorOrderBy;
	switch (orderBy) {
		case 'Value':
			safeOrderBy = CreatorOrderBy.Value;
			break;
		case 'IsLive':
			safeOrderBy = CreatorOrderBy.IsLive;
			break;
		default:
			safeOrderBy = CreatorOrderBy.Name;
			break;
	}

	const creators = await client.getCreators(page, 20, search, safeOrderBy, orderDir);

	return {
		channels: creators.data.map(c => c.toJSON()),
		total: creators.total,
		currentPage: page,
		sortField: orderBy,
		sortDirection: orderDir,
		searchQuery: search
	};
};
