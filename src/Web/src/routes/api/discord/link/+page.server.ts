import { getApiClient } from '$lib';
import { LinkDiscordTwitchDto } from '$lib/api';
import { login } from '$lib/auth/sessions';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ url, cookies }) => {
	const linkToken = url.searchParams.get('link_token');
	const twitchId = url.searchParams.get('twitch_id');

	if (!linkToken || !twitchId) {
		return;
	}

  let client = getApiClient('');
	const { access_token: token } = await client.linkDiscordTwitch(new LinkDiscordTwitchDto({
    access_token: linkToken,
    twitch_id: twitchId,
  }));

	client = getApiClient(token);
	const user = await client.getSelf();
	console.log(user)
	login(cookies, token, 'None');
  return { token, user: user.toJSON() };
};
