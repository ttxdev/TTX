import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { getToken } from '$lib/auth';
import { CreatorOrderBy, OrderDirection } from '$lib/api';

export const load: PageServerLoad = async ({ cookies, url }) => {
	const client = getApiClient(getToken(cookies) ?? '');
	const page = Number(url.searchParams.get('page')) || 1;
	const orderBy = url.searchParams.get('orderBy') || 'Name';
	const order =
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

	const creators = await client.getCreators(page, 20, search, safeOrderBy);

	return {
		channels: creators.data.map((c) => c.toJSON()),
		total: creators.total,
		currentPage: page,
		sortField: orderBy,
		sortDirection: order,
		searchQuery: search
	};
};
