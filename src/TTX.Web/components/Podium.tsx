import { formatValue } from "../lib/formatting.ts";

export type Placement = {
  name: string;
  avatarUrl: string;
  value: number;
  href: string;
};

export function Podium({
  header,
  placements,
}: {
  header: string;
  placements: Placement[];
}) {
  const sorted = placements.sort((a, b) => b.value - a.value).slice(0, 3);
  const first = sorted[0];
  const second = sorted[1];
  const third = sorted[2];

  return (
    <div class="bg-base-200/50 relative rounded-xl p-6">
      <div class="bg-base-300 absolute top-4 left-4 rounded-lg px-3 py-1 text-sm font-medium">
        {header}
      </div>

      <div class="mx-auto flex h-full max-w-3xl items-end justify-center gap-8 max-md:gap-4">
        {second && (
          <div class="flex w-1/3 flex-col items-center max-md:w-full">
            <a href={second.href}>
              <img
                src={second.avatarUrl}
                alt=""
                class="mb-2 size-12 rounded-full"
              />
            </a>
            <div class="indicator w-full">
              <span class="indicator-item indicator-center badge font-semibold text-[#C0C0C0]">
                2nd
              </span>
              <div class="h-48 relative w-full rounded-t-lg bg-[#C0C0C0]/10">
                <div class="absolute inset-x-0 bottom-4 flex flex-col items-center max-md:inset-x-2">
                  <a
                    href={second.href}
                    class="font-medium hover:underline max-md:text-sm"
                  >
                    {second.name}
                  </a>
                  <span class="text-sm opacity-75">
                    {formatValue(second.value)}
                  </span>
                </div>
              </div>
            </div>
          </div>
        )}

        {first && (
          <div class="flex w-1/3 flex-col items-center max-md:w-full">
            <a href={first.href}>
              <img
                src={first.avatarUrl}
                alt=""
                class="mb-2 size-12 rounded-full"
              />
            </a>
            <div class="indicator w-full">
              <span class="indicator-item indicator-center badge font-semibold text-[#FFD700]">
                1st
              </span>
              <div class="h-64 relative w-full rounded-t-lg bg-[#FFD700]/10`">
                <div class="absolute inset-x-0 bottom-4 flex flex-col items-center max-md:inset-x-2">
                  <a
                    href={first.href}
                    class="font-medium hover:underline max-md:text-sm"
                  >
                    {first.name}
                  </a>
                  <span class="text-sm opacity-75 max-md:text-xs">
                    {formatValue(first.value)}
                  </span>
                </div>
              </div>
            </div>
          </div>
        )}

        {third && (
          <div class="flex w-1/3 flex-col items-center max-md:w-full">
            <a href={third.href}>
              <img
                src={third.avatarUrl}
                alt=""
                class="mb-2 size-12 rounded-full"
              />
            </a>
            <div class="indicator w-full">
              <span class="indicator-item indicator-center badge font-semibold text-[#CD7F32]">
                3rd
              </span>
              <div class="h-32 relative w-full rounded-t-lg bg-[#CD7F32]/10">
                <div class="absolute inset-x-0 bottom-4 flex flex-col items-center max-md:inset-x-2">
                  <a
                    href={third.href}
                    class="font-medium hover:underline max-md:text-sm"
                  >
                    {third.name}
                  </a>
                  <span class="text-sm opacity-75">
                    {formatValue(third.value)}
                  </span>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
