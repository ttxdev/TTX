import { define } from "@/utils.ts";
import {
  CreatorDto,
  CreatorOrderBy,
  OrderDirection,
  PlayerOrderBy,
} from "../lib/api.ts";
import { getApiClient } from "../lib/index.ts";
import { Placement, Podium } from "../components/Podium.tsx";
import FeaturedCreator from "@/components/FeaturedCreator.tsx";
import { formatTicker, formatValue } from "../lib/formatting.ts";
import { calculatePercentChange } from "../lib/math.ts";
import type { ComponentChildren } from "preact";

function TwitchIcon({ class: className }: { class?: string }) {
  return (
    <svg
      width="16"
      height="16"
      viewBox="0 0 24 24"
      xmlns="http://www.w3.org/2000/svg"
      class={className}
    >
      <path
        fill="currentColor"
        d="M11.571 4.714h1.715v5.143H11.57zm4.715 0H18v5.143h-1.714zM6 0L1.714 4.286v15.428h5.143V24l4.286-4.286h3.428L22.286 12V0zm14.571 11.143l-3.428 3.428h-3.429l-3 3v-3H6.857V1.714h13.714Z"
      />
    </svg>
  );
}

function SearchIcon() {
  return (
    <svg
      class="size-5"
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
    >
      <circle cx="11" cy="11" r="8" />
      <path d="m21 21-4.3-4.3" />
    </svg>
  );
}

function TrendingUpIcon() {
  return (
    <svg
      class="size-5"
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
    >
      <path d="M22 7 13.5 15.5 8.5 10.5 2 17" />
      <path d="M16 7h6v6" />
    </svg>
  );
}

function TrophyIcon() {
  return (
    <svg
      class="size-5"
      xmlns="http://www.w3.org/2000/svg"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
    >
      <path d="M6 9H4.5a2.5 2.5 0 0 1 0-5H6" />
      <path d="M18 9h1.5a2.5 2.5 0 0 0 0-5H18" />
      <path d="M4 22h16" />
      <path d="M10 14.66V17c0 .55-.47.98-.97 1.21C7.85 18.75 7 20.24 7 22" />
      <path d="M14 14.66V17c0 .55.47.98.97 1.21C16.15 18.75 17 20.24 17 22" />
      <path d="M18 2H6v7a6 6 0 0 0 12 0V2Z" />
    </svg>
  );
}

/** Buy/sell call-to-action that adapts to the visitor's auth state. */
function AuthCtaButtons({ user }: { user?: { slug: string } }) {
  const primary =
    "btn flex items-center gap-2 rounded-lg border-none bg-purple-600 px-5 text-white shadow hover:bg-purple-700";
  const secondary =
    "btn rounded-lg border-purple-500 bg-transparent px-5 text-purple-500 shadow-none hover:bg-purple-500/10";

  if (user) {
    return (
      <>
        <a href="/creators" class={primary}>Browse Creators</a>
        <a href={`/players/${user.slug}`} class={secondary}>My Portfolio</a>
      </>
    );
  }

  return (
    <>
      <a href="/api/login?from=/" class={primary}>
        <TwitchIcon class="size-4" />
        Login with Twitch
      </a>
      <a href="/creators" class={secondary}>Browse Creators</a>
    </>
  );
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div class="border-base-content/10 bg-base-200/40 flex flex-col items-center gap-1 rounded-2xl border p-5 text-center">
      <span class="font-display text-3xl max-md:text-2xl">{value}</span>
      <span class="text-xs tracking-widest uppercase opacity-50">{label}</span>
    </div>
  );
}

function Step(
  { icon, title, body }: {
    icon: ComponentChildren;
    title: string;
    body: string;
  },
) {
  return (
    <li class="group flex items-start gap-4">
      <div class="relative flex size-11 shrink-0 items-center justify-center rounded-xl bg-purple-500/10 text-purple-500 transition-colors group-hover:bg-purple-500/20">
        {icon}
      </div>
      <div class="flex flex-col">
        <h3 class="font-semibold">{title}</h3>
        <p class="text-sm opacity-60">{body}</p>
      </div>
    </li>
  );
}

function LiveCreatorRow({ creator }: { creator: CreatorDto }) {
  const change = calculatePercentChange(creator.history);
  const up = change >= 0;
  const live = creator.stream_status?.is_live;

  return (
    <a
      href={`/creators/${creator.slug}`}
      class="group border-base-content/10 bg-base-200/40 hover:border-purple-500/30 hover:bg-base-200/70 flex items-center gap-3 rounded-2xl border p-3 transition-colors"
    >
      <div class="relative shrink-0">
        <img
          src={creator.avatar_url}
          alt=""
          class={`size-10 rounded-full object-cover ring-2 ${
            live ? "ring-red-500/60" : "ring-base-content/10"
          }`}
        />
        {live && (
          <span class="border-base-100 absolute -right-0.5 -bottom-0.5 size-3 rounded-full border-2 bg-red-500" />
        )}
      </div>
      <div class="flex min-w-0 flex-col">
        <span class="truncate font-semibold transition-colors group-hover:text-purple-500">
          {creator.name}
        </span>
        <span class="font-mono text-xs opacity-60">
          {formatTicker(creator.ticker)}
        </span>
      </div>
      <div class="ml-auto flex shrink-0 flex-col items-end">
        <span class="font-semibold">{formatValue(creator.value)}</span>
        <span
          class={`text-xs font-semibold ${
            up ? "text-green-500" : "text-red-500"
          }`}
        >
          {up ? "▲" : "▼"} {Math.abs(change).toFixed(1)}%
        </span>
      </div>
    </a>
  );
}

/** Compact vertical list of currently-live creators, paired with the steps. */
function LiveCreators({ creators }: { creators: CreatorDto[] }) {
  return (
    <div class="flex flex-col gap-3">
      <div class="flex items-center justify-between">
        <span class="flex items-center gap-2 text-xs font-semibold tracking-widest text-purple-500 uppercase">
          <span class="inline-block size-2 animate-pulse rounded-full bg-red-500" />
          Live Right Now
        </span>
        <a
          href="/creators"
          class="text-xs font-semibold text-purple-500 hover:underline"
        >
          View all →
        </a>
      </div>
      {creators.map((c) => <LiveCreatorRow key={c.id} creator={c} />)}
    </div>
  );
}

function SectionHeading(
  { kicker, title, subtitle }: {
    kicker?: string;
    title: string;
    subtitle?: string;
  },
) {
  return (
    <div class="flex flex-col items-center gap-2 text-center">
      {kicker && (
        <span class="text-xs font-semibold tracking-widest text-purple-500 uppercase">
          {kicker}
        </span>
      )}
      <h2 class="font-display text-4xl max-md:text-3xl">{title}</h2>
      {subtitle && <p class="max-w-md text-sm opacity-60">{subtitle}</p>}
    </div>
  );
}

export const handler = define.handlers({
  async GET(ctx) {
    const client = getApiClient(ctx.state.token, ctx.state.auth);
    const [featuredCreator, featuredCreators, topCreators, topPlayers] =
      await Promise.all([
        client.getCreator("dougdoug"),
        client.getCreators(
          1,
          4,
          undefined,
          CreatorOrderBy.IsLive,
          OrderDirection.Descending,
        ),
        client.getCreators(
          1,
          3,
          undefined,
          CreatorOrderBy.Value,
          OrderDirection.Descending,
        ),
        client.getPlayers(
          1,
          3,
          undefined,
          PlayerOrderBy.Portfolio,
          OrderDirection.Descending,
        ),
      ]);

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
  const { featuredCreator, featuredCreators, topCreators, topPlayers } =
    ctx.data;
  const user = ctx.state.user;

  const topCreatorValue = topCreators.data[0]?.value;
  const topPlayerValue = topPlayers.data[0]?.portfolio;

  return (
    <div class="mx-auto flex max-w-250 flex-col space-y-24">
      {/* Hero */}
      <section class="grid items-center gap-8 md:grid-cols-2">
        <div class="flex flex-col items-center gap-6 text-center max-md:gap-4">
          <img
            class="w-96 max-md:w-72 dark:hidden"
            src="/ttx-full-logo-dark.png"
            alt="TTX Logo"
          />
          <img
            class="hidden w-96 max-md:w-72 dark:block"
            src="/ttx-full-logo-light.png"
            alt="TTX Logo"
          />
          <p class="font-display text-2xl max-md:text-lg">
            Invest in your favorite Creators
          </p>
          <p class="max-w-sm text-sm opacity-70">
            A fantasy stock market for Twitch streamers. Buy low, sell high, and
            climb the leaderboard
          </p>
          <div class="flex flex-row flex-wrap items-center justify-center gap-3">
            <AuthCtaButtons user={user} />
          </div>
        </div>
        <div class="flex h-full w-full flex-col px-10 md:px-4">
          <FeaturedCreator creator={featuredCreator} />
        </div>
      </section>

      {/* Stats */}
      <section class="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <Stat
          label="Creators"
          value={featuredCreators.total.toLocaleString()}
        />
        <Stat label="Players" value={topPlayers.total.toLocaleString()} />
        <Stat
          label="Top Stock"
          value={topCreatorValue !== undefined
            ? formatValue(topCreatorValue)
            : "—"}
        />
        <Stat
          label="Top Portfolio"
          value={topPlayerValue !== undefined
            ? formatValue(topPlayerValue)
            : "—"}
        />
      </section>

      {/* How it works — steps paired with creators you can act on right now */}
      <section class="grid items-start gap-10 md:grid-cols-2">
        <div class="flex flex-col gap-8">
          <div class="flex flex-col gap-2 max-md:items-center max-md:text-center">
            <span class="text-xs font-semibold tracking-widest text-purple-500 uppercase">
              Getting Started
            </span>
            <h2 class="font-display text-4xl max-md:text-3xl">How it Works</h2>
            <p class="text-sm opacity-60">
              Buy and sell shares of your favorite creators, climb the
              leaderboard, and flex on your friends. Don't know how the NYSE
              works? Doesn't matter — just buy low and sell high. Have fun, and
              {" "}
              <span class="italic">don't call the SEC</span>.
            </p>
          </div>
          <ol class="flex flex-col gap-6">
            <Step
              icon={<SearchIcon />}
              title="Browse Creators"
              body="Find your favorite Twitch streamers listed on the market."
            />
            <Step
              icon={<TrendingUpIcon />}
              title="Buy & Sell Shares"
              body="Trade on their momentum while they're live and the price moves."
            />
            <Step
              icon={<TrophyIcon />}
              title="Climb the Leaderboard"
              body="Grow your portfolio and rise through the player rankings."
            />
          </ol>
        </div>

        <LiveCreators creators={featuredCreators.data} />

        <div class="mx-auto flex max-w-2xl items-start gap-2.5 rounded-xl border border-amber-500/30 bg-amber-500/10 p-3 text-left text-sm md:col-span-2">
          <svg
            class="mt-0.5 size-4 shrink-0 text-amber-500"
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
            stroke-linecap="round"
            stroke-linejoin="round"
          >
            <circle cx="12" cy="12" r="10" />
            <path d="M12 16v-4" />
            <path d="M12 8h.01" />
          </svg>
          <p>
            <strong class="text-amber-600 dark:text-amber-400">
              TTX is all parody.
            </strong>{" "}
            This project will <strong>never</strong>{" "}
            be associated with real money or crypto. Please read our{" "}
            <a href="/privacy" class="text-purple-500 hover:underline">
              privacy policy
            </a>{" "}
            if you have anymore concerns or contact us on our Discord.
          </p>
        </div>
      </section>

      {/* Leaderboard */}
      <section class="flex flex-col items-center gap-8">
        <SectionHeading
          kicker="Rankings"
          title="Leaderboard"
          subtitle="The biggest creators and the sharpest investors."
        />
        <div class="grid w-full gap-4 px-7 md:grid-cols-2 md:px-0">
          <Podium
            header="Top Creators"
            placements={topCreators.data.map<Placement>((c) => ({
              name: c.name,
              value: c.value,
              avatarUrl: c.avatar_url,
              href: `/creators/${c.slug}`,
            }))}
          />
          <Podium
            header="Top Players"
            placements={topPlayers.data.map<Placement>((p) => ({
              name: p.name,
              value: p.value,
              avatarUrl: p.avatar_url,
              href: `/players/${p.slug}`,
            }))}
          />
        </div>
      </section>

      <section class="flex flex-col items-center gap-4 rounded-2xl border border-purple-500/20 bg-gradient-to-br from-purple-600/15 to-purple-900/10 p-10 text-center">
        <span class="text-xs font-semibold tracking-widest text-purple-500 uppercase">
          Get Started
        </span>
        <h2 class="font-display text-3xl max-md:text-2xl">
          Ready to get truly invested?
        </h2>

        <div class="mt-2 flex flex-row flex-wrap justify-center gap-3">
          <AuthCtaButtons user={user} />
        </div>
      </section>
    </div>
  );
});
