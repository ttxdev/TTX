import { Head } from "fresh/runtime";
import { define, State } from "@/utils.ts";
import ExternalLink from "@/components/ExternalLink.tsx";

type TeamMember = {
  name: string;
  image: string;
  role: string;
  bio: string;
  socials: {
    twitch?: string;
    twitter?: string;
    github?: string;
  };
};

const TEAM_MEMBERS: TeamMember[] = [
  {
    name: "Chai",
    image: "/chai.jpg",
    role: "Chief Executive Officer",
    bio:
      "Passionate about building a platform that empowers users to achieve their financial goals while having fun along the way. With a background in finance and technology, I am dedicated to creating innovative solutions that enhance the trading and gaming experience for our users.",
    socials: {
      github: "https://codeberg.org/dylan",
      twitch: "https://www.twitch.tv/ttlnow",
    },
  },
  {
    name: "Solid",
    image:
      "https://static-cdn.jtvnw.net/jtv_user_pictures/dsolidbear-profile_image-58fe6502815ac8ad-70x70.jpeg",
    role: "Chief Technology Officer",
    bio:
      "Innovating the future of trading and gaming with cutting-edge technology and a passion for excellence. Leading the charge in building a platform that empowers users to achieve their financial goals while having fun along the way.",
    socials: {
      github: "https://github.com/dsolid",
      twitch: "https://twitch.tv/dSolidBear",
    },
  },
  {
    name: "Nate",
    image:
      "https://cdn.discordapp.com/avatars/712700300430934017/52a8dee066743f76b6f91ea516fa6ead.webp?size=480",
    role: "Chief Idea Catalyst",
    bio:
      "Inspiration for innovative features like probabilistic daily incentive system, AI-Driven chance-based gaming simulators, and member-funded prosperity program that thrill our users and keep them coming back for more, driving explosive growth and making our platform the go-to destination for enthusiastic traders and players",
    socials: {
      github: "https://github.com/nathanroberts55",
      twitch: "https://twitch.tv/nastynate55",
    },
  },
  {
    name: "Lycanthropy",
    image: "https://avatars.githubusercontent.com/u/53885212?v=4",
    role: "Chief Mathematical Wizard",
    bio:
      "Mathematical wizard with a passion for creating complex algorithms and data-driven solutions. I love turning numbers into insights and insights into action.",
    socials: {
      twitch: "https://twitch.tv/msi_lycanthropy",
      github: "https://github.com/msilycanthropy",
    },
  },
  {
    name: "Bey",
    image: "/bey.jpg",
    role: "Chief Baking Officer",
    bio:
      "Baking the cake and making it look pretty. I love creating beautiful and user-friendly interfaces that enhance the overall experience of our platform.",
    socials: {
      twitch: "https://twitch.tv/quabey",
      github: "https://github.com/quabey",
    },
  },
  {
    name: "Rub",
    image: "/rub.webp",
    role: "Chief Vibes Officer",
    bio:
      "Enabling users to gamble their life savings away with a smile. I am passionate about creating user-friendly interfaces that enhance the overall experience of our platform.",
    socials: {
      github: "https://github.com/RubenOlano",
      twitch: "https://twitch.tv/RubbyO",
    },
  },
  {
    name: "Goob-Dev",
    image: "https://avatars.githubusercontent.com/u/67513182?v=4",
    role: "Chief Memes Officer",
    bio:
      "Transforming raw footage into captivating stories that resonate with our audience. I am passionate about creating engaging content that showcases our platform and its features.",
    socials: {
      twitch: "https://twitch.tv/manosteel400",
      github: "https://github.com/Goob-hub",
    },
  },
  {
    name: "Pete",
    image: "/the_pete.png",
    role:
      "Senior Distinguished Manager of Idea Generation and Quality Assurance",
    bio:
      "Dedicated to the procurement of bleeding edge features/functionality keeping TTX as a leader in the streamer securities space. Displays a keen eye for errors (and annoying regulations) and ensures that our platform superceeds world class quality standards for our tresured users.",
    socials: {
      twitch: "https://twitch.tv/the_pete",
    },
  },
  {
    name: "highsecurity",
    image: "/highsecurity.png",
    role: "Chief Marketing Officer",
    bio:
      "Creating visually stunning designs that capture the essence of our platform and resonate with our audience. I am passionate about creating engaging content that showcases our platform and its features.",
    socials: {
      twitch: "https://twitch.tv/highsecurity_",
    },
  },
  {
    name: "Darn",
    image: "/darn.webp",
    role: "Chief Number Agent",
    bio: "Moved some numbers up and some numbers down. Didn’t actually do much of anything",
    socials: {
      twitch: "https://twitch.tv/darn1t",
      github: "https://github.com/smhurt158"
    }
  }
];

const THANK_YOU: [name: string, href: string][] = [
  ["Antiparty", "https://github.com/antiparty"],
  ["Aspecticor", "https://twitch.tv/aspecticor"],
  ["Carson", "/doggie.jpg"],
  ["Jester Man", "https://twitch.tv/jesterman4242"],
  ["Liqiors", "https://twitch.tv/liqiors"],
  ["Matt", "https://twitch.tv/stinkycornflakes"],
  ["Mild", "https://twitch.tv/mildlyuniqueusername"],
  ["Mokey", "https://twitch.tv/deathharmonics"],
  ["Nick", "https://twitch.tv/crouton888"],
  ["Scum", "https://twitch.tv/scumble__"],
  ["Swinda", "https://twitch.tv/swinda_"],
  ["t1zz", "https://twitch.tv/t1zz_tv"],
  ["Vortex", "https://github.com/VortexHero"],
  ["whodat950", "https://twitch.tv/whodat950"],
  ["yeonaii", "https://twitch.tv/yeonaii"],
];

function GithubIcon() {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="currentColor"
      aria-hidden="true"
    >
      <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
    </svg>
  );
}

function TwitchIcon() {
  return (
    <svg
      width="24"
      height="24"
      viewBox="0 0 24 24"
      xmlns="http://www.w3.org/2000/svg"
      fill="currentColor"
      aria-hidden="true"
    >
      <path d="M11.571 4.714h1.715v5.143H11.57zm4.715 0H18v5.143h-1.714zM6 0L1.714 4.286v15.428h5.143V24l4.286-4.286h3.428L22.286 12V0zm14.571 11.143l-3.428 3.428h-3.429l-3 3v-3H6.857V1.714h13.714Z" />
    </svg>
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

function TeamMemberCard(
  { state, member }: { state: State; member: TeamMember },
) {
  return (
    <div class="group border-base-content/10 bg-base-200/40 hover:border-purple-500/30 flex h-full w-full max-w-62 flex-col items-center rounded-2xl border p-5 text-center transition-all duration-200 hover:-translate-y-1 hover:shadow-lg hover:shadow-purple-500/10">
      <ExternalLink
        clientId={state.discordId}
        href={member.socials.twitch}
        target="_blank"
        class="flex justify-center"
        ariaLabel={`${member.name} on Twitch`}
      >
        <img
          class="ring-base-content/10 size-24 rounded-full object-cover ring-2 transition duration-200 group-hover:scale-105 group-hover:ring-purple-500/50 sm:size-28"
          src={member.image}
          alt={member.name}
          loading="lazy"
        />
      </ExternalLink>
      <h2 class="mt-3 text-lg font-bold sm:text-xl">{member.name}</h2>
      <p class="text-xs font-semibold tracking-widest text-purple-500 uppercase">
        {member.role}
      </p>
      <p class="mt-2 text-sm opacity-70">
        {member.bio}
      </p>
      <div class="mt-auto flex justify-center gap-2 pt-4">
        {member.socials.github && (
          <ExternalLink
            clientId={state.discordId}
            href={member.socials.github}
            target="_blank"
            class="btn btn-circle btn-ghost hover:bg-purple-500/10 hover:text-purple-500"
            ariaLabel={`${member.name} on GitHub`}
          >
            <GithubIcon />
          </ExternalLink>
        )}
        {member.socials.twitch && (
          <ExternalLink
            clientId={state.discordId}
            href={member.socials.twitch}
            target="_blank"
            class="btn btn-circle btn-ghost hover:bg-purple-500/10 hover:text-purple-500"
            ariaLabel={`${member.name} on Twitch`}
          >
            <TwitchIcon />
          </ExternalLink>
        )}
      </div>
    </div>
  );
}

export default define.page((ctx) => {
  return (
    <>
      <Head>
        <title>TTX - Meet the Team</title>
        <meta
          name="description"
          content="Meet the team behind TTX. We are a group of passionate individuals dedicated to building a better future for all."
        />
      </Head>
      <div class="mx-auto flex max-w-250 flex-col items-center gap-20 p-4">
        <section class="flex w-full flex-col items-center gap-8">
          <SectionHeading
            kicker="Who We Are"
            title="Meet the Team"
            subtitle="The crew building the streamer stock market — chronically online, mostly responsible."
          />
          <div class="grid w-full grid-cols-1 justify-items-center gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {TEAM_MEMBERS.map((member) => (
              <TeamMemberCard
                state={ctx.state}
                member={member}
                key={`member-${member.name}`}
              />
            ))}
          </div>
        </section>
        <section class="flex w-full flex-col items-center gap-8">
          <SectionHeading
            kicker="Shoutouts"
            title="Thank You"
            subtitle="The people who helped make TTX what it is."
          />
          <div class="mx-auto flex w-full max-w-150 flex-row flex-wrap items-center justify-center gap-2 text-center">
            {THANK_YOU.map(([name, href]) => (
              <ExternalLink
                clientId={ctx.state.discordId}
                target="_blank"
                href={href}
                class="badge badge-lg border-base-content/10 bg-base-200/40 transition hover:border-purple-500/40 hover:text-purple-500"
                key={`thanks-${name}`}
              >
                {name}
              </ExternalLink>
            ))}
            <span class="text-sm opacity-50">
              and the original ATX Team.
            </span>
          </div>
          <div class="flex w-full flex-row flex-wrap justify-center gap-3">
            <ExternalLink
              clientId={ctx.state.discordId}
              href={Deno.env.get("FRESH_PUBLIC_DISCORD_URL")!}
              target="_blank"
              class="btn rounded-lg border-none bg-purple-600 px-6 font-bold text-white shadow transition hover:bg-purple-700 active:scale-95"
            >
              Join our Discord!
            </ExternalLink>
            <ExternalLink
              clientId={ctx.state.discordId}
              href="https://codeberg.org/TTX/TTX"
              target="_blank"
              class="btn border-base-content/20 rounded-lg bg-transparent px-6 font-bold transition hover:bg-base-200 active:scale-95"
            >
              Source Code
            </ExternalLink>
          </div>
        </section>
      </div>
    </>
  );
});
