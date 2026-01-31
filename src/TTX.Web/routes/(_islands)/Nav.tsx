import Search from "./Search.tsx";
import { State } from "@/utils.ts";
import { motion } from "motion/react";

export default function Nav({ url, state }: { url: URL; state: State }) {
  const from = encodeURIComponent(url.pathname);
  const user = state.user;
  const urls = Object.freeze([
    {
      url: "/",
      label: "Home",
    },
    {
      url: "/creators",
      label: "Creators",
    },
    {
      url: "/players",
      label: "Leaderboard",
    },
    {
      url: "/team",
      label: "About Us",
    },
  ]);

  return (
    <header>
      <div class="navbar fixed left-1/2 z-20 mx-auto w-full -translate-x-1/2 rounded-xl bg-clip-padding p-6 shadow-sm backdrop-blur backdrop-contrast-100 backdrop-saturate-100 backdrop-filter">
        <div class="navbar-start">
          <a href="/" class="btn btn-ghost rounded-2xl text-xl">
            <img class="w-8" src="/ttx-logo-only.png" alt="TTX Logo" />
          </a>
          <div class="badge badge-ghost rounded-xl">BETA</div>
        </div>

        <nav class="navbar-center hidden lg:flex">
          <ul class="menu menu-horizontal relative px-1">
            <div
              class="absolute rounded-2xl bg-purple-400/20 duration-100 animate-all"
              style="left: {indicatorLeft.current}px; width: {indicatorWidth.current}px; top: 0; bottom: 0; z-index: 0;"
            >
            </div>

            {urls.map(({ url: u, label }) => {
              const isActive = u === "/"
                ? url.pathname == u
                : url.pathname.startsWith(u);

              return (
                <li key={u} className="relative">
                  <a
                    href={u}
                    class="relative z-10 hover:bg-transparent"
                  >
                    {label}
                  </a>

                  {isActive && (
                    <motion.div
                      layoutId="nav-pill"
                      className="absolute inset-0 z-0 rounded-2xl bg-purple-400/20"
                      transition={{
                        type: "spring",
                        stiffness: 350,
                        damping: 30,
                      }}
                    />
                  )}
                </li>
              );
            })}

            <li>
              <Search state={state} />
            </li>
          </ul>
        </nav>

        <div class="navbar-end">
          <div class="flex gap-2">
            {user && (
              <div class="join">
                <a href={"/players/" + user.slug}>
                  <div class="btn rounded-lg bg-black px-3 py-2 text-white shadow md:rounded-l-lg md:rounded-r-none">
                    <div class="flex flex-row items-center justify-between gap-2">
                      <img
                        src={user.avatarUrl}
                        alt=""
                        class="size-6 rounded-full"
                      />
                      {user.name}
                    </div>
                  </div>
                </a>
                <a href="/api/logout" class="hidden md:block">
                  <div class="btn rounded-r-lg bg-black py-2 text-white shadow hover:text-red-400">
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      viewBox="0 0 16 16"
                      fill="currentColor"
                      class="size-4"
                    >
                      <path
                        fill-rule="evenodd"
                        d="M14 4.75A2.75 2.75 0 0 0 11.25 2h-3A2.75 2.75 0 0 0 5.5 4.75v.5a.75.75 0 0 0 1.5 0v-.5c0-.69.56-1.25 1.25-1.25h3c.69 0 1.25.56 1.25 1.25v6.5c0 .69-.56 1.25-1.25 1.25h-3c-.69 0-1.25-.56-1.25-1.25v-.5a.75.75 0 0 0-1.5 0v.5A2.75 2.75 0 0 0 8.25 14h3A2.75 2.75 0 0 0 14 11.25v-6.5Zm-9.47.47a.75.75 0 0 0-1.06 0L1.22 7.47a.75.75 0 0 0 0 1.06l2.25 2.25a.75.75 0 1 0 1.06-1.06l-.97-.97h7.19a.75.75 0 0 0 0-1.5H3.56l.97-.97a.75.75 0 0 0 0-1.06Z"
                        clip-rule="evenodd"
                      />
                    </svg>
                    <span>Logout</span>
                  </div>
                </a>
              </div>
            )}
            {!user && (
              <a
                href={`/api/login?from=${from}`}
                class="flex items-center justify-center gap-2"
              >
                <div class="btn rounded-md bg-black px-3 py-2 text-white shadow">
                  <svg
                    width="16"
                    height="16"
                    viewBox="0 0 24 24"
                    xmlns="http://www.w3.org/2000/svg"
                  >
                    <path
                      fill="white"
                      d="M11.571 4.714h1.715v5.143H11.57zm4.715 0H18v5.143h-1.714zM6 0L1.714 4.286v15.428h5.143V24l4.286-4.286h3.428L22.286 12V0zm14.571 11.143l-3.428 3.428h-3.429l-3 3v-3H6.857V1.714h13.714Z"
                    />
                  </svg>
                  Login
                </div>
              </a>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}
