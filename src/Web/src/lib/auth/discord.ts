import type { Cookies } from "@sveltejs/kit";
import { parseJwt, type JwtData } from "./jwt";
import { getApiClient } from "..";
import { login } from "$lib/auth";

const COOKIE_KEY = "gg.ttx.discord";

type DiscordUserData = JwtData & {
  connections: any[];
}

export async function handleDiscordCallback(cookies: Cookies, code: string): Promise<void> {
	const client = getApiClient('');
  const { access_token: token } = await client.discordCallback(code);
  const data = parseDiscordUser(token);
	cookies.set(COOKIE_KEY, token, {
    path: '/',
    exp: new Date(data.exp * 1000),
  });
}

export async function linkDiscordTwitch(cookies: Cookies, twitchId: string): Promise<void> {
  const client = getApiClient('');
  const dToken = cookies.get(COOKIE_KEY)!;
  const { access_token: token } = await client.linkDiscordTwitch({
    access_token: dToken,
    twitch_id: twitchId,
  });

  login(cookies, token);
}

function parseDiscordUser(token: string): DiscordUserData {
  return parseJwt(token) as DiscordUserData;
}
