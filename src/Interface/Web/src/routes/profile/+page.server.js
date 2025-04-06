import { redirect } from '@sveltejs/kit';

// get user name from session

export function load() {
	throw redirect(307, '/');
}
