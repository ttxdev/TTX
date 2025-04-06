import { type RequestHandler } from '@sveltejs/kit';
import { requestLogin } from '$lib/auth';

export const GET: RequestHandler = ({ cookies, url }) => {
	const from = url.searchParams.get('from') ?? '/';
	return requestLogin(cookies, from);
};
