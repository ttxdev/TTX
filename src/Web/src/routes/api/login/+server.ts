import { requestLogin } from '$lib/auth/sessions';
import { type RequestHandler } from '@sveltejs/kit';

export const GET: RequestHandler = ({ cookies, url }) => {
	const from = url.searchParams.get('from') ?? '/';
	return requestLogin(cookies, from);
};
