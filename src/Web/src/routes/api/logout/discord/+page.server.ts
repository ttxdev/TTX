import { logout } from '$lib/auth/sessions';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = ({ cookies }) => {
	logout(cookies, 'None');

	return;
};
