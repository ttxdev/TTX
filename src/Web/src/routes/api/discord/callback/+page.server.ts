import { getApiClient } from '$lib';
import {
	getToken,
	getUserData,
	handleDiscordCallbackToTwitch,
	login,
	logout,
	parseJwt
} from '$lib/auth';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ url, cookies }) => {
	const code = url.searchParams.get('access_token');

	if (!code) {
		return;
	}

	const token = await handleDiscordCallbackToTwitch(code);
	login(cookies, token, 'None');

	const jwtData = parseJwt(token);

	const user = {
		userId: jwtData.userId,
		name: jwtData.name,
		avatarUrl: jwtData.avatarUrl,
		role: jwtData.role
	};

	return {
		user,
		token
	};
};
