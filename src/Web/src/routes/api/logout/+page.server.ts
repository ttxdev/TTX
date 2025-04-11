import { logout } from '$lib/auth';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = ({ cookies }) => {
	logout(cookies);

	return;
};
