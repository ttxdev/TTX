import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { getToken } from '$lib/auth';
import { Order, OrderDirection } from '$lib/api';

export const load: PageServerLoad = async ({ cookies, url }) => {
	const client = getApiClient(getToken(cookies) ?? '');
	const page = Number(url.searchParams.get('page')) || 1;
	const orderBy = url.searchParams.get('orderBy') || 'Name';
	const order =
		url.searchParams.get('order') == 'desc' ? OrderDirection.Descending : OrderDirection.Ascending;
	const search = url.searchParams.get('search') || '';

	let safeOrderBy = [new Order({ by: 'Name', dir: order })];
	if (orderBy === 'Name') {
		safeOrderBy = [new Order({ by: 'Name', dir: order })];
	} else if (orderBy === 'Value') {
		safeOrderBy = [new Order({ by: 'Value', dir: order })];
	} else if (orderBy === 'IsLive') {
		safeOrderBy = [new Order({ by: 'IsLive', dir: order }), new Order({ by: 'Name', dir: OrderDirection.Ascending })];
	}

	const creators = await client.getCreators(
		page,
		20,
		search ? 'Slug' : undefined, // Changed from 'All' to 'Slug'
		search,
		safeOrderBy
	);

	return {
		channels: creators.data.map((c) => c.toJSON()),
		total: creators.total,
		currentPage: page,
		sortField: orderBy,
		sortDirection: order,
		searchQuery: search
	};
};
