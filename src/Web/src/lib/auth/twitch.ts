import {
	PUBLIC_TWITCH_REDIRECT_URL as twitchRedirectUrl,
	PUBLIC_TWITCH_CLIENT_ID as twitchClientId
} from '$env/static/public';

export function getTwitchRedirect(): string {
	const state =
		Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);

	return `https://id.twitch.tv/oauth2/authorize?client_id=${twitchClientId}&redirect_uri=${encodeURIComponent(twitchRedirectUrl)}&response_type=code&scope=&state=${state}`;
}
