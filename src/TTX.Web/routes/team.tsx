import { Head } from "fresh/runtime";
import { define, State } from "@/utils.ts";
import ExternalLink from "@/components/ExternalLink.tsx";

type TeamMember = {
  name: string;
  image: string;
  role: string;
  bio: string;
  socials: {
    twitch: string;
    twitter?: string;
    github?: string;
  };
};

function TeamMemberCard(
  { state, member }: { state: State; member: TeamMember },
) {
  return (
    <div class="card w-full max-w-62 rounded-2xl bg-gray-500/20 bg-clip-padding p-2 shadow-md backdrop-blur-sm backdrop-filter">
      <ExternalLink
        clientId={state.discordId}
        href={member.socials.twitch!}
        target="_blank"
        class="flex justify-center"
      >
        <img
          class="size-24 rounded-full bg-gray-400/20 sm:size-32"
          src={member.image}
          alt=""
        />
      </ExternalLink>
      <div class="card-body p-2 sm:p-4">
        <div class="flex flex-col">
          <h2 class="card-title p-0 text-lg sm:text-xl">{member.name}</h2>
          <p class="p-0 text-sm font-medium text-gray-500">{member.role}</p>
        </div>
        <p class="text-sm sm:text-base">{member.bio}</p>
        <div class="card-actions mt-2 flex justify-end gap-2 sm:mt-4 sm:gap-4">
          {member.socials.github && (
            <ExternalLink
              clientId={state.discordId}
              href={member.socials.github}
              target="_blank"
              class="btn btn-circle btn-ghost text-black hover:bg-white dark:text-white dark:hover:bg-neutral-700"
              ariaLabel="GitHub"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="currentColor"
              >
                <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
              </svg>
            </ExternalLink>
          )}
          {member.socials.twitch && (
            <ExternalLink
              clientId={state.discordId}
              href={member.socials.twitch}
              target="_blank"
              class="btn btn-circle btn-ghost text-violet-400 hover:bg-white dark:text-white dark:hover:bg-purple-700"
              ariaLabel="Twitch"
            >
              <svg
                width="24"
                height="24"
                viewBox="0 0 24 24"
                xmlns="http://www.w3.org/2000/svg"
                fill="currentColor"
              >
                <path d="M11.571 4.714h1.715v5.143H11.57zm4.715 0H18v5.143h-1.714zM6 0L1.714 4.286v15.428h5.143V24l4.286-4.286h3.428L22.286 12V0zm14.571 11.143l-3.428 3.428h-3.429l-3 3v-3H6.857V1.714h13.714Z" />
              </svg>
            </ExternalLink>
          )}
        </div>
      </div>
    </div>
  );
}

export default define.page((ctx) => {
  const data = {
    thankYou: [
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
    ],
    teamMembers: [
      {
        name: "Chai",
        image: "/chai.jpg",
        role: "Chief Executive Officer",
        bio:
          "Passionate about building a platform that empowers users to achieve their financial goals while having fun along the way. With a background in finance and technology, I am dedicated to creating innovative solutions that enhance the trading and gaming experience for our users.",
        socials: {
          github: "https://github.com/dylhack",
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
        role: "Lead Frontend Developer",
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
        role: "Video Editor",
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
        image:
          "https://static-cdn.jtvnw.net/jtv_user_pictures/9c610ee2-369b-459d-867d-8d2dd47fc904-profile_image-300x300.png",
        role: "Graphic Designer",
        bio:
          "Creating visually stunning designs that capture the essence of our platform and resonate with our audience. I am passionate about creating engaging content that showcases our platform and its features.",
        socials: {
          twitch: "https://twitch.tv/highsecurity_",
        },
      },
    ] satisfies TeamMember[],
  };

  return (
    <>
      <Head>
        <title>TTX - Meet the Team</title>
        <meta
          name="description"
          content="Meet the team behind TTX. We are a group of passionate individuals dedicated to building a better future for all."
        />
      </Head>
      <div class="mx-auto flex max-w-250 flex-col items-center justify-center gap-8 p-4 md:gap-16">
        <section class="w-full">
          <h1 class="font-display my-4 text-center text-3xl md:text-left md:text-4xl">
            Meet the Team
          </h1>
          <div class="grid w-full grid-cols-1 justify-items-center gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {data.teamMembers.map((member) => (
              <TeamMemberCard
                state={ctx.state}
                member={member}
                key={`member-${member.name}`}
              />
            ))}
          </div>
        </section>
        <section class="w-full">
          <h1 class="font-display my-4 text-center text-3xl md:text-4xl">
            Thank You
          </h1>
          <div class="mx-auto flex w-full max-w-150 flex-row flex-wrap items-center gap-3 text-center">
            {data.thankYou.map(([name, href]) => (
              <ExternalLink
                clientId={ctx.state.discordId}
                target="_blank"
                href={href}
                class="hover:text-violet-400"
              >
                {name}
              </ExternalLink>
            ))}
            <span>and the original ATX Team.</span>
          </div>
          <div class="my-6 flex w-full flex-row justify-center md:my-10">
            <ExternalLink
              clientId={ctx.state.discordId}
              href={Deno.env.get("FRESH_PUBLIC_DISCORD_URL")!}
              target="_blank"
              class="rounded bg-violet-500 px-6 py-3 text-center font-bold text-white transition hover:bg-violet-600"
            >
              Join our Discord!
            </ExternalLink>
          </div>
        </section>
      </div>
    </>
  );
});
