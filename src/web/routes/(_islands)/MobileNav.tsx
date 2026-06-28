import { Signal } from "@preact/signals";
import { State } from "../../utils.ts";
import { AnimatePresence, motion } from "motion/react";

export default function MobileNav(
  { urls, state, url, isSearchOpen, isMobileMenuOpen, toggleMobileMenu }: {
    urls: readonly { url: string; label: string }[];
    state: State;
    url: URL;
    isSearchOpen: Signal<boolean>;
    isMobileMenuOpen: Signal<boolean>;
    toggleMobileMenu: () => void;
  },
) {
  const from = encodeURIComponent(url.pathname);
  const user = state.user;

  return (
    <>
      <AnimatePresence>
        {isMobileMenuOpen.value && (
          <motion.div
            initial={{ scale: .1, x: 135, y: -280 }}
            animate={{ scale: 1, x: 0, y: 0 }}
            transition={{ duration: .20 }}
            exit={{ scale: .1, x: 135, y: -280 }}
            class="fixed left-0 top-0 z-50 h-screen w-full bg-white shadow-lg backdrop-blur backdrop-contrast-100 backdrop-saturate-200 backdrop-filter lg:hidden dark:bg-black"
          >
            <div class="mt-4 flex h-full flex-col">
              <div class="flex items-center justify-between p-4">
                <a
                  href="/"
                  class="btn btn-ghost rounded-2xl text-xl"
                  onClick={toggleMobileMenu}
                >
                  <img class="w-8" src="/ttx-logo-only.png" alt="TTX Logo" />
                </a>
                <button
                  class="btn btn-ghost"
                  onClick={toggleMobileMenu}
                  type="button"
                  aria-label="Close menu"
                >
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    class="h-6 w-6"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M6 18L18 6M6 6l12 12"
                    />
                  </svg>
                </button>
              </div>
              <nav class="flex-1 overflow-y-auto p-4">
                <ul class="space-y-4">
                  {urls.map(({ url, label }) => (
                    <li>
                      <a
                        href={url}
                        class="block rounded-lg px-4 py-2 text-lg font-medium underline-offset-2 hover:bg-gray-100"
                        onClick={toggleMobileMenu}
                      >
                        {label}
                      </a>
                    </li>
                  ))}
                  <li>
                    <button
                      type="button"
                      onClick={() => {
                        isSearchOpen.value = true;
                      }}
                      class="block rounded-lg px-4 py-2 text-lg font-medium underline-offset-2 hover:bg-gray-100 flex items-center justify-center gap-2 cursor-pointer"
                    >
                      Search
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        fill="none"
                        viewBox="0 0 24 24"
                        stroke-width="1.5"
                        stroke="currentColor"
                        class="size-5"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z"
                        />
                      </svg>
                    </button>
                  </li>
                </ul>
              </nav>
              <div class="mb-2 border-t p-4">
                {user && (
                  <div class="flex flex-col gap-2">
                    <a href={"/players/" + user.slug}>
                      <div class="btn w-full rounded-md bg-black px-3 py-2 text-white shadow">
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
                    <a href="/api/logout">
                      <div class="btn w-full rounded-lg bg-black py-2 text-white shadow">
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
                {!state.discordId && (
                  <a
                    href={`/api/login?from=${from}`}
                    class="flex items-center justify-center gap-2"
                  >
                    <div class="btn w-full rounded-md bg-black px-3 py-2 text-white shadow">
                      <svg
                        width="16"
                        height="16"
                        viewBox="0 0 24 24"
                        xmlns="http://www.w3.org/2000/svg"
                      >
                        <path
                          fill="black"
                          d="M11.571 4.714h1.715v5.143H11.57zm4.715 0H18v5.143h-1.714zM6 0L1.714 4.286v15.428h5.143V24l4.286-4.286h3.428L22.286 12V0zm14.571 11.143l-3.428 3.428h-3.429l-3 3v-3H6.857V1.714h13.714Z"
                        />
                      </svg>
                      Login
                    </div>
                  </a>
                )}
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  );
}
