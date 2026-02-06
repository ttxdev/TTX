import {
  CreatorDto,
  CreatorShareDto,
  CreatorTransactionDto,
  TimeStep,
} from "@/lib/api.ts";
import { getApiClient } from "@/lib/index.ts";
import { define } from "@/utils.ts";
import Creator, { CreatorProps, Interval } from "./(_islands)/Creator.tsx";
import { Head } from "fresh/runtime";

export const handler = define.handlers({
  async GET(ctx): Promise<{ data: CreatorProps | null }> {
    try {
      const slug = ctx.params.slug.toLowerCase();
      const interval =
        (ctx.url.searchParams.get("interval") ?? "1h") as Interval;
      let hours: number;
      switch (interval) {
        case "24h":
          hours = 24;
          break;
        case "12h":
          hours = 12;
          break;
        case "6h":
          hours = 6;
          break;
        default:
          hours = 1;
          break;
      }

      const client = getApiClient(ctx.state.token);
      const creator = await client.getCreator(
        slug,
        TimeStep.Minute,
        `${hours}:00:00`
      );

      let isPlayer, currentUserIsCreator;
      if (ctx.state.user?.userId == creator.id) {
        isPlayer = true;
        currentUserIsCreator = true;
      } else {
        isPlayer = await client.getPlayer(slug)
          .then(() => true)
          .catch(() => false);
        currentUserIsCreator = await client.getSelf()
          .then((user) => user.slug === slug)
          .catch(() => false);
      }

      return {
        data: {
          url: ctx.url,
          state: ctx.state,
          creator: creator.toJSON() as CreatorDto,
          shares: creator.shares.map<CreatorShareDto>((d) => d.toJSON()),
          transactions: creator.transactions
            .sort((a, b) => b.created_at.getTime() - a.created_at.getTime())
            .map<CreatorTransactionDto>((d) => d.toJSON()),
          interval,
          isPlayer,
          currentUserIsCreator,
        },
      };
    } catch (err) {
      console.error(err);
      return { data: null };
    }
  },
});

export default define.page<typeof handler>((ctx) => {
  if (!ctx.data) {
    return <div>creator not found</div>;
  }

  return (
    <>
      <Head>
        <title>{`TTX - ${ctx.data.creator.name}`}</title>
      </Head>
      <Creator {...ctx.data} />
    </>
  );
});
