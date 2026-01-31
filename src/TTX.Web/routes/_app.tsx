import { define } from "@/utils.ts";
import ExternalLink from "@/components/ExternalLink.tsx";
import Nav from "./(_components)/Nav.tsx";
import Drawer from "./(_islands)/Drawer.tsx";

export default define.page((ctx) => {
  return (
    <html lang="en">
      <head>
        <meta charset="utf-8" />
        <link rel="icon" href="/favicon.png" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <link
          rel="apple-touch-icon"
          sizes="180x180"
          href="/apple-touch-icon.png"
        />
        <link
          rel="icon"
          type="image/png"
          sizes="32x32"
          href="/favicon-32x32.png"
        />
        <link
          rel="icon"
          type="image/png"
          sizes="16x16"
          href="/favicon-16x16.png"
        />
        <link rel="manifest" href="/site.webmanifest" />
        <title>TTX</title>
      </head>
      <body class="flex min-h-screen w-full flex-col from-40% via-transparent via-70%">
        <Nav state={ctx.state} url={ctx.url} />
        <div
          class="h-24"
          style="
				background: linear-gradient(
					0deg,
					hsla(277, 100%, 5%, 0) 5%,
					hsla(278, 100%, 50%, 0.5) 100%
				);
			"
        >
        </div>
        <main class="container mx-auto flex w-full grow flex-col pb-24 md:p-24">
          <ctx.Component />
        </main>

        <Drawer state={ctx.state} />
        <footer class="footer sm:footer-horizontal footer-center text-base-content bg-transparent p-4">
          <aside>
            <p>
              Copyright © {new Date().getFullYear()} - All right reserved.
            </p>
            <div class="flex flex-row gap-4">
              <ExternalLink
                clientId={ctx.state.discordId}
                href={Deno.env.get("FRESH_PUBLIC_DISCORD_URL")!}
                target="_blank"
                class="w-fit text-purple-500 hover:underline"
              >
                Join our Discord!
              </ExternalLink>
              <a href="/privacy" class="text-purple-500 hover:underline">
                Privacy Policy
              </a>
            </div>
          </aside>
        </footer>
      </body>
    </html>
  );
});
