import { PUBLIC_DISCORD_CLIENT_ID as discordClientId } from '$env/static/public';
import { DiscordSDK } from '@discord/embedded-app-sdk';

export const discordSdk =
	typeof window !== 'undefined' && window.location.host === discordClientId + '.discordsays.com'
		? new DiscordSDK(discordClientId)
		: null;
