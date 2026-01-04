import { define } from "@/utils.ts";
import { CreatorOrderBy, OrderDirection, PlayerOrderBy } from "../lib/api.ts";
import { getApiClient } from "../lib/index.ts";
import { Placement, Podium } from "../components/Podium.tsx";
import ExternalLink from "@/components/ExternalLink.tsx";
import Leaderboard from "./(_islands)/Leaderboard.tsx";
import FeaturedChart from "./(_islands)/FeaturedChart.tsx";

export const handler = define.handlers({
  async GET(ctx) {
    const client = getApiClient(ctx.state.token);
    const featuredCreator = await client.getCreator("dougdoug");
    const featuredCreators = await client
      .getCreators(
        1,
        3,
        undefined,
        CreatorOrderBy.IsLive,
        OrderDirection.Descending,
      );
    const topCreators = await client
      .getCreators(
        1,
        3,
        undefined,
        CreatorOrderBy.Value,
        OrderDirection.Descending,
      );
    const topPlayers = await client
      .getPlayers(
        1,
        3,
        undefined,
        PlayerOrderBy.Portfolio,
        OrderDirection.Descending,
      );

    return {
      data: {
        featuredCreator,
        featuredCreators,
        topCreators,
        topPlayers,
      },
    };
  },
});

export default define.page<typeof handler>((ctx) => {
  return (
    <div class="mx-auto flex max-w-250 flex-col space-y-12">
      <section class="flex columns-2 max-md:flex-col max-md:gap-8">
        <div class="flex w-1/2 flex-row items-center justify-center max-md:w-full">
          <div class="flex flex-col items-center">
            <img
              class="w-96 dark:hidden"
              src="/ttx-full-logo-dark.png"
              alt="TTX Logo"
            />
            <img
              class="hidden w-96 dark:block"
              src="/ttx-full-logo-light.png"
              alt="TTX Logo"
            />
            <p class="font-display text-2xl max-md:text-sm">
              Invest in your favorite Creators
            </p>
          </div>
        </div>
        <div class="flex w-1/2 flex-row items-center justify-center px-10 max-md:w-full md:px-0">
          <div class="flex h-full w-full p-4 max-md:p-0">
            <FeaturedChart creator={ctx.data.featuredCreator} />
          </div>
        </div>
      </section>
      <section class="flex columns-2 flex-row-reverse max-md:flex-col max-md:gap-8">
        <div class="flex w-1/2 flex-col gap-4 p-8 max-md:w-full max-md:flex-col max-md:p-0 max-md:text-center">
          <h1 class="font-display mb-2 text-4xl max-md:text-3xl">
            How it Works
          </h1>
          <p class="w-full px-7 text-justify md:px-0">
            TTX is a fantasy stock market for Twitch streamers. You can buy and
            sell shares of your favorite creators.{" "}
            <strong>
              Not real money, not crypto-related, and never will be.
            </strong>
            <br />
            <br />
            Perform up to the minute statistical pricing analysis to time your
            purchases and sales to maximize your investment potential (all tax
            free, as we are free from pesky regulations).
            <br />
            <br />
            Oh.. how does the pricing work? You don't know how the NYSE works,
            ya know... so just pump and dump! I mean... buy low, sell high... I
            mean dollar cost average! Be responsible, have fun,
            <span class="italic">don't call the SEC</span>. TTX, be truly
            invested in your streamer.
          </p>
          <div class="flex flex-row justify-center">
            <ExternalLink
              clientId={ctx.state.discordId}
              href={Deno.env.get("FRESH_PUBLIC_DISCORD_URL")!}
              target="_blank"
              class="w-fit text-purple-500 hover:underline"
            >
              Join our Discord!
            </ExternalLink>
          </div>
        </div>
        <div class="flex w-1/2 flex-col items-center justify-center max-md:w-full">
          <Leaderboard creators={ctx.data.featuredCreators.data} />
          <a
            class="mt-2 hover:underline hover:decoration-purple-500 font-black text-purple-500"
            href="/creators"
          >
            View all creators →
          </a>
        </div>
      </section>
      <section class="flex max-md:flex-col">
        <div class="flex w-full flex-col items-center justify-center gap-12">
          <p class="font-display text-center text-4xl font-bold max-md:text-3xl">
            Leaderboard
          </p>
          <div class="flex w-full flex-row gap-4 px-7 max-md:flex-col md:px-0">
            <div class="w-1/2 max-md:w-full">
              <Podium
                header="Top Creators"
                placements={ctx.data.topCreators.data.map<Placement>((c) => ({
                  name: c.name,
                  value: c.value,
                  avatarUrl: c.avatar_url,
                  href: `/creators/${c.slug}`,
                }))}
              />
            </div>
            <div class="w-1/2 max-md:w-full">
              <Podium
                header="Top Players"
                placements={ctx.data.topPlayers.data.map<Placement>((p) => ({
                  name: p.name,
                  value: p.value,
                  avatarUrl: p.avatar_url,
                  href: `/players/${p.slug}`,
                }))}
              />
            </div>
          </div>
        </div>
      </section>
    </div>
  );
});
