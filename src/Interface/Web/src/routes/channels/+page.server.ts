import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { getToken } from '$lib/auth';

export const load: PageServerLoad = async ({ cookies, url }) => {
	try {
		const client = getApiClient(getToken(cookies) ?? '');
		const page = Number(url.searchParams.get('page')) || 1;
		const orderBy = url.searchParams.get('orderBy') || 'Name';
		const order = url.searchParams.get('order') || 'asc';
		const search = url.searchParams.get('search') || '';

		// Ensure orderBy is a valid field
		const validOrderFields = ['Name', 'Value', 'IsLive'];
		const safeOrderBy = validOrderFields.includes(orderBy) ? orderBy : 'Name';

		// Ensure order is valid
		const safeOrder = ['asc', 'desc'].includes(order.toLowerCase()) ? order.toLowerCase() : 'asc';

		try {
			const channels = await client.getCreators(
				page,
				20,
				safeOrderBy,
				safeOrder,
				search ? 'Slug' : undefined, // Changed from 'All' to 'Slug'
				search
			);

			return {
				channels: channels.data.map((c) => c.toJSON()),
				total: channels.total,
				currentPage: page,
				sortField: safeOrderBy,
				sortDirection: safeOrder,
				searchQuery: search
			};
		} catch (apiError) {
			console.error('API Error:', apiError);
			// Return empty state on API error
			return {
				channels: [],
				total: 0,
				currentPage: 1,
				sortField: 'Name',
				sortDirection: 'asc',
				searchQuery: search
			};
		}
	} catch (error) {
		console.error('Error in load function:', error);
		return {
			channels: [],
			total: 0,
			currentPage: 1,
			sortField: 'Name',
			sortDirection: 'asc',
			searchQuery: ''
		};
	}
};
