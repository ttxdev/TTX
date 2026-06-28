import { define } from "@/utils.ts";
import { getApiClient } from "../lib/index.ts";
import Gamba from "./(_islands)/Gamba.tsx";

export const handler = define.handlers({
  async GET(ctx) {
    const client = getApiClient(ctx.state.token);
    const self = await client.getSelf();

    return {
      data: {
        boxes: self.loot_boxes,
      },
    };
  },
});

export default define.page<typeof handler>((ctx) => {
  return (
    <div>
      <Gamba boxes={ctx.data.boxes} state={ctx.state} />
    </div>
  );
});
