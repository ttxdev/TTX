import { error, json, type RequestHandler } from '@sveltejs/kit';
import { getApiClient } from '$lib';
import { getToken } from '$lib/auth/sessions';

export const POST: RequestHandler = async ({ params, cookies }) => {
	const { slug } = params;

	if (!slug) {
		throw error(400, 'Creator slug is required');
	}

	const token = getToken(cookies);
	if (!token) {
		throw error(401, 'Authentication required');
	}

	const client = getApiClient(token);

	try {
		await client.creatorOptOut(slug);
		return json({ success: true });
	} catch (err) {
		console.error('Error processing opt-out request:', err);
		throw error(500, 'Failed to process opt-out request');
	}

};