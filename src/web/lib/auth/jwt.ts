export type JwtData = {
  exp: number;
  iss: string;
  aud: string;
};

export type UserData = JwtData & {
  userId: number;
  slug: string;
  avatarUrl: string;
  role: string;
  name: string;
  updatedAt: string;
};

export function parseJwt(token: string) {
  const base64Url = token.split(".")[1];
  const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");

  return JSON.parse(
    decodeURIComponent(
      atob(base64)
        .split("")
        .map(function (c) {
          return "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2);
        })
        .join(""),
    ),
  );
}

export function parseUserToken(token: string): UserData {
  const jsonPayload = parseJwt(token);

  return {
    userId: Number(jsonPayload.sub),
    slug: jsonPayload.name,
    name: jsonPayload.given_name,
    role: jsonPayload.role,
    avatarUrl: jsonPayload.avatar_url,
    updatedAt: jsonPayload.updated_at,
    exp: jsonPayload.exp,
    iss: jsonPayload.iss,
    aud: jsonPayload.aud,
  };
}
