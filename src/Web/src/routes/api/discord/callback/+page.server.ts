import {
	getToken,
	getUserData,
	handleDiscordCallback,
	login,
	logout,
	parseJwt
} from '$lib/auth';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ url, cookies }) => {
	const code = url.searchParams.get('code');

	if (!code) {
		return;
	}

	const token = await handleDiscordCallback(code);
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
