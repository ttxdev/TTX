import { getApiClient } from "../index.ts";
import { parseJwt, parseUserToken, UserData } from "./jwt.ts";
import { deleteCookie, getCookies, setCookie } from "@std/http/cookie";

export type SessionData = {
  token: string;
  user: UserData;
};

const COOKIE_KEY = "token";
const REDIR_KEY = "redirect";

export async function requestLogin(redir = "/") {
  const loginUrl = await getApiClient().getLoginUrl();
  const headers = new Headers();
  headers.set("Location", loginUrl);
  setCookie(headers, {
    name: REDIR_KEY,
    value: redir,
    path: "/",
    expires: new Date(Date.now() + 1000 * 60 * 5),
  });

  return new Response(undefined, {
    status: 307,
    headers: headers,
  });
}

export function getRedir(headers: Headers) {
  const redir = getCookies(headers)[REDIR_KEY];
  if (!redir) return null;

  return redir;
}

export function removeRedir(headers: Headers) {
  deleteCookie(headers, REDIR_KEY);
}

export function setSession(
  headers: Headers,
  token: string,
) {
  const jwtData = parseJwt(token);

  setCookie(headers, {
    name: COOKIE_KEY,
    path: "/",
    expires: new Date(jwtData.exp * 1000),
    value: token,
  });
}

export function removeSession(headers: Headers) {
  deleteCookie(headers, COOKIE_KEY, {
    path: "/",
  });
}

export function getToken(headers: Headers): string | null {
  return getCookies(headers)[COOKIE_KEY] ?? null;
}

export function getSession(headers: Headers): SessionData | null {
  const token = getToken(headers);
  if (!token) return null;

  return {
    token,
    user: parseUserToken(token),
  };
}
