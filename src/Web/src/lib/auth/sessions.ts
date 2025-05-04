import { redirect, type Cookies } from "@sveltejs/kit";
import { parseJwt, type JwtData } from "./jwt";
import { getTwitchRedirect } from "./twitch";

export type UserData = JwtData & {
	userId: string;
	name: string;
	avatarUrl: string;
	role: string;
	updatedAt: string;
};

export type SessionData = {
	token: string;
	user: UserData;
};

const COOKIE_KEY = 'TTX.Session';

export function requestLogin(cookies: Cookies, redir = '/') {
	cookies.set('redirect', redir, { path: '/', expires: new Date(Date.now() + 1000 * 60 * 5) });
	return redirect(307, getTwitchRedirect());
}

export function login(cookies: Cookies, token: string, sameSite = 'Lax') {
	const jwtData = parseUserToken(token);
	cookies.set(
		COOKIE_KEY,
		JSON.stringify({
			token,
			user: {
				userId: jwtData.userId,
				name: jwtData.name,
				avatarUrl: jwtData.avatarUrl,
				role: jwtData.role
			}
		}),
		{ path: '/', expires: new Date(jwtData.exp * 1000), sameSite }
	);
}

export function logout(cookies: Cookies, sameSite = 'Lax') {
	cookies.delete(COOKIE_KEY, {
		path: '/',
		sameSite
	});
}

export function getSession(cookies: Cookies): SessionData | null {
	const data = cookies.get(COOKIE_KEY);
	if (!data) return null;

	const parsed = JSON.parse(data);
  return {
    token: parsed.token,
    user: parsed.user
  }
}

export function getToken(cookies: Cookies): string | null {
  return getSession(cookies)?.token ?? null;
}

function parseUserToken(token: string): UserData {
	const jsonPayload = parseJwt(token);

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
