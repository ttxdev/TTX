import { PUBLIC_DISCORD_CLIENT_ID as discordClientId } from '$env/static/public';
import { DiscordSDK } from '@discord/embedded-app-sdk';
import type { Cookies } from '@sveltejs/kit';
import { COOKIE_NAME } from './auth';

export const discordSdk =
	typeof window !== 'undefined' && window.location.host === discordClientId + '.discordsays.com'
		? new DiscordSDK(discordClientId)
		: null;

export function discordSetupNeeded(query: URLSearchParams, cookies: Cookies): boolean {
	return query.has('frame_id') && !cookies.get(COOKIE_NAME);
}
