import {
	PUBLIC_TWITCH_REDIRECT_URL as twitchRedirectUrl,
	PUBLIC_TWITCH_CLIENT_ID as twitchClientId
} from '$env/static/public';
import { getApiClient } from '$lib';
import { redirect, type Cookies } from '@sveltejs/kit';

const COOKIE_NAME = 'TTX.Session';

type RawJwtData = {
	'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': string; // userId
	'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': string; // name
	'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': string; // role
	AvatarUrl: string;
	UpdatedAt: string;
	exp: number;
	iss: string;
	aud: string;
};

export type JwtData = {
	userId: string;
	name: string;
	avatarUrl: string;
	role: string;
	updatedAt: string;
	exp: number;
	iss: string;
	aud: string;
};

export type UserData = {
	userId: string;
	name: string;
	avatarUrl: string;
	role: string;
};

export type CookieData = {
	token: string;
	user: UserData;
};

export function getTwitchRedirect(): string {
	const state =
		Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);

	return `https://id.twitch.tv/oauth2/authorize?client_id=${twitchClientId}&redirect_uri=${encodeURIComponent(twitchRedirectUrl)}&response_type=code&scope=&state=${state}`;
}

export async function handleTwitchCallback(code: string, state: string): Promise<string> {
	const client = getApiClient('');
	const { access_token } = await client.twitchCallback(code);

	return access_token;
}

export function requestLogin(cookies: Cookies, redir = '/') {
	cookies.set('redirect', redir, { path: '/', expires: new Date(Date.now() + 1000 * 60 * 5) });
	return redirect(307, getTwitchRedirect());
}

export function login(cookies: Cookies, token: string) {
	const jwtData = parseJwt(token);
	cookies.set(
		COOKIE_NAME,
		JSON.stringify({
			token,
			user: {
				userId: jwtData.userId,
				name: jwtData.name,
				avatarUrl: jwtData.avatarUrl,
				role: jwtData.role
			}
		}),
		{ path: '/', expires: new Date(jwtData.exp * 1000) }
	);
}

export function logout(cookies: Cookies) {
	cookies.delete(COOKIE_NAME, {
		path: '/'
	});
}

export function getToken(cookies: Cookies): string | null {
	const data = cookies.get(COOKIE_NAME);
	if (!data) return null;

	return JSON.parse(data).token;
}

export function getUserData(cookies: Cookies): UserData | null {
	const data = cookies.get(COOKIE_NAME);
	if (!data) return null;

	return JSON.parse(data).user;
}

export function parseJwt(token: string): JwtData {
	const base64Url = token.split('.')[1];
	const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');

	const jsonPayload: RawJwtData = JSON.parse(
		decodeURIComponent(
			atob(base64)
				.split('')
				.map(function (c) {
					return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
				})
				.join('')
		)
	);

	return {
		userId: jsonPayload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
		name: jsonPayload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
		role: jsonPayload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
		avatarUrl: jsonPayload.AvatarUrl,
		updatedAt: jsonPayload.UpdatedAt,
		exp: jsonPayload.exp,
		iss: jsonPayload.iss,
		aud: jsonPayload.aud
	};
}
