import { createDefine } from "fresh";
import { UserData } from "./lib/auth/jwt.ts";

export type Context = {
  state: State;
  url: URL;
};

export interface State {
  token?: string;
  user?: UserData;
  discordId: string | null;
  auth?: { unauthorized: boolean };
}

export const define = createDefine<State>();
