import { handleTwitchCallback, login } from '$lib/auth';
import { redirect, type RequestHandler } from '@sveltejs/kit';

export const GET: RequestHandler = async ({ url, cookies }) => {
	const code = url.searchParams.get('code');
	const state = url.searchParams.get('state');

	if (!code || !state) {
		return new Response('Missing code or state', { status: 400 });
	}

	const token = await handleTwitchCallback(code, state);
	login(cookies, token);

	const redir = cookies.get('redirect');
	if (redir) {
		cookies.delete('redirect', { path: '/' });
	}

	throw redirect(307, redir || '/');
};
