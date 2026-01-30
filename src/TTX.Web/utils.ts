import { createDefine } from "fresh";
import { UserData } from "./lib/auth/jwt.ts";

export type Context = {
  state: State;
  url: URL;
};

// This specifies the type of "ctx.state" which is used to share
// data among middlewares, layouts and routes.
export interface State {
  token?: string;
  user?: UserData;
  discordId: string | null;
}

export const define = createDefine<State>();
