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
    userId: Number(
      jsonPayload[
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
      ],
    ),
    slug:
      jsonPayload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
    name: jsonPayload[
      "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenName"
    ],
    role: jsonPayload[
      "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    ],
    avatarUrl: jsonPayload.AvatarUrl,
    updatedAt: jsonPayload.UpdatedAt,
    exp: jsonPayload.exp,
    iss: jsonPayload.iss,
    aud: jsonPayload.aud,
  };
}
