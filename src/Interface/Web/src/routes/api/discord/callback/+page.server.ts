import { handleDiscordCallbackToTwitch, login } from '$lib/auth';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ url, cookies }) => {
	const code = url.searchParams.get('access_token');
	const user = url.searchParams.get('user');

	if (!code || !user) {
		return;
	}

	const token = await handleDiscordCallbackToTwitch(code, user);
	login(cookies, token, 'None');

	return;
};
