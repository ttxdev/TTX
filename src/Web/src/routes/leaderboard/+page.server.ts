import type { PageServerLoad } from './$types';
import { getApiClient } from '$lib';
import { PlayerOrderBy, OrderDirection } from '$lib/api';

export const load: PageServerLoad = async ({ url }) => {
    const client = getApiClient('');
    const page = Number(url.searchParams.get('page')) || 1;
    const search = url.searchParams.get('search') || ''; // Get the search query
    const orderBy = url.searchParams.get('orderBy') || 'Portfolio';
    const orderDir =
        url.searchParams.get('order') === 'asc' ? OrderDirection.Ascending : OrderDirection.Descending;

    let safeOrderBy: PlayerOrderBy;
    switch (orderBy) {
        case 'Name':
            safeOrderBy = PlayerOrderBy.Name;
            break;
        case 'Credits':
            safeOrderBy = PlayerOrderBy.Credits;
            break;
        default:
            safeOrderBy = PlayerOrderBy.Portfolio;
            break;
    }

    // Pass the search parameter to the API call
    const players = await client.getPlayers(page, 20, search, safeOrderBy, orderDir);

    return {
        players: players.data.map((p) => p.toJSON()),
        total: players.total,
        currentPage: page,
        sortField: orderBy,
        sortDirection: orderDir,
        searchQuery: search // Return the search query for client-side use
    };
};