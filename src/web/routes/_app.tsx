import { define } from "@/utils.ts";
import ExternalLink from "@/components/ExternalLink.tsx";
import Nav from "./(_islands)/Nav.tsx";
import Drawer from "./(_islands)/Drawer.tsx";
import ToastContainer from "../islands/ToastContainer.tsx";

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
      <body class="flex min-h-screen w-full flex-col">
        <Nav state={ctx.state} url={ctx.url} />
        <main class="container mx-auto flex w-full grow flex-col px-4 pt-32 pb-24 md:px-24">
          <ctx.Component />
        </main>

        <ToastContainer />
        <Drawer state={ctx.state} />
        <footer class="footer sm:footer-horizontal footer-center text-base-content bg-transparent p-4">
          <aside>
            <p>
              Copyright © {new Date().getFullYear()} - All right reserved.
            </p>
            <div class="flex flex-row gap-4">
              <a href="/privacy" class="text-purple-500 hover:underline">
                Privacy Policy
              </a>
              <ExternalLink
                clientId={ctx.state.discordId}
                href={Deno.env.get("FRESH_PUBLIC_DISCORD_URL")!}
                target="_blank"
                class="w-fit text-purple-500 hover:underline"
              >
                Join our Discord!
              </ExternalLink>
              <ExternalLink
                clientId={ctx.state.discordId}
                href="https://codeberg.org/TTX/TTX"
                target="_blank"
                class="w-fit text-purple-500 hover:underline"
              >
                We're open-source!
              </ExternalLink>
            </div>
          </aside>
        </footer>
      </body>
    </html>
  );
});
