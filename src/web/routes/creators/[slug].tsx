import {
  CreatorDto,
  CreatorShareDto,
  CreatorTransactionDto,
} from "@/lib/api.ts";
import { getApiClient } from "@/lib/index.ts";
import { define } from "@/utils.ts";
import { formatTicker, formatValue } from "@/lib/formatting.ts";
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

      const client = getApiClient(ctx.state.token, ctx.state.auth);
      const creator = await client.getCreator(
        slug,
        hours * 3600,
      );

      let isPlayer, currentUserIsCreator;
      if (ctx.state.user?.userId == creator.id) {
        isPlayer = true;
        currentUserIsCreator = true;
      } else {
        [isPlayer, currentUserIsCreator] = await Promise.all([
          client.getPlayer(slug)
            .then(() => true)
            .catch(() => false),
          client.getSelf()
            .then((user) => user.slug === slug)
            .catch(() => false),
        ]);
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
    return <CreatorNotFound />;
  }

  const { creator } = ctx.data;
  const ticker = formatTicker(creator.ticker);
  const title = `${creator.name} (${ticker}) · TTX`;
  const description =
    `Track ${creator.name} (${ticker}) on TTX — currently valued at ${
      formatValue(creator.value)
    }. Buy and sell shares of your favorite Twitch creators.`;
  const canonical = `${ctx.url.origin}/creators/${creator.slug}`;
  const imageAlt = `${creator.name}'s avatar`;

  return (
    <>
      <Head>
        <title>{`TTX - ${creator.name}`}</title>
        <meta name="description" content={description} />
        <link rel="canonical" href={canonical} />
        <meta property="og:type" content="profile" />
        <meta property="og:site_name" content="TTX" />
        <meta property="og:url" content={canonical} />
        <meta property="og:title" content={title} />
        <meta property="og:description" content={description} />
        <meta property="og:image" content={creator.avatar_url} />
        <meta property="og:image:alt" content={imageAlt} />
        <meta name="twitter:card" content="summary" />
        <meta name="twitter:title" content={title} />
        <meta name="twitter:description" content={description} />
        <meta name="twitter:image" content={creator.avatar_url} />
        <meta name="twitter:image:alt" content={imageAlt} />
      </Head>
      <Creator {...ctx.data} />
    </>
  );
});

function CreatorNotFound() {
  return (
    <>
      <Head>
        <title>TTX - Creator not found</title>
      </Head>
      <div class="mx-auto flex w-full max-w-250 flex-col items-center gap-4 p-4 py-24 text-center">
        <p class="font-display text-6xl max-md:text-4xl">404</p>
        <p class="font-display text-2xl max-md:text-xl">Creator not found</p>
        <p class="max-w-md text-sm opacity-60">
          We couldn't find that creator. They may not be listed on TTX, or the
          link may be incorrect.
        </p>
        <a
          href="/creators"
          class="btn mt-2 rounded-lg border-none bg-purple-600 px-5 text-white shadow hover:bg-purple-700"
        >
          Browse all creators
        </a>
      </div>
    </>
  );
}
