import { logout } from '$lib/auth';
import { redirect, type RequestHandler } from '@sveltejs/kit';

export const GET: RequestHandler = ({ cookies }) => {
	logout(cookies);
	return redirect(307, '/');
};
