import { formatValue } from "../lib/formatting.ts";

export type Placement = {
  name: string;
  avatarUrl: string;
  value: number;
  href: string;
};

type Rank = 1 | 2 | 3;

// Gold / silver / bronze, plus the per-rank pedestal height that makes the
// staircase. Full class strings (not interpolated) so Tailwind can see them.
const MEDALS: Record<
  Rank,
  { label: string; ring: string; bar: string; text: string; height: string }
> = {
  1: {
    label: "1st",
    ring: "ring-[#FFD700]",
    bar: "bg-[#FFD700]/10",
    text: "text-[#FFD700]",
    height: "h-52 max-md:h-40",
  },
  2: {
    label: "2nd",
    ring: "ring-[#C0C0C0]",
    bar: "bg-[#C0C0C0]/10",
    text: "text-[#C0C0C0]",
    height: "h-40 max-md:h-32",
  },
  3: {
    label: "3rd",
    ring: "ring-[#CD7F32]",
    bar: "bg-[#CD7F32]/10",
    text: "text-[#CD7F32]",
    height: "h-28 max-md:h-24",
  },
};

function PodiumColumn(
  { placement, rank }: { placement: Placement; rank: Rank },
) {
  const m = MEDALS[rank];

  return (
    <div class="flex w-1/3 flex-col items-center gap-3">
      <a href={placement.href} class="relative shrink-0">
        <img
          src={placement.avatarUrl}
          alt=""
          class={`size-12 rounded-full object-cover ring-2 max-md:size-10 ${m.ring}`}
        />
        <span
          class={`border-base-200 bg-base-100 absolute -bottom-1.5 left-1/2 -translate-x-1/2 rounded-full border-2 px-1.5 text-[10px] font-bold ${m.text}`}
        >
          {m.label}
        </span>
      </a>
      <div
        class={`border-base-content/10 relative w-full rounded-t-2xl border border-b-0 ${m.bar} ${m.height}`}
      >
        <div class="absolute inset-x-0 bottom-3 flex flex-col items-center gap-0.5 px-2 text-center">
          <a
            href={placement.href}
            class="max-w-full truncate font-semibold transition-colors hover:text-purple-500 max-md:text-sm"
          >
            {placement.name}
          </a>
          <span class="text-sm opacity-75 max-md:text-xs">
            {formatValue(placement.value)}
          </span>
        </div>
      </div>
    </div>
  );
}

export function Podium({
  header,
  placements,
}: {
  header: string;
  placements: Placement[];
}) {
  // toSorted: don't mutate the caller's array. Classic podium order: 2 · 1 · 3.
  const [first, second, third] = placements
    .toSorted((a, b) => b.value - a.value)
    .slice(0, 3);

  return (
    <div class="border-base-content/10 bg-base-200/40 flex flex-col gap-6 rounded-2xl border p-6">
      <h3 class="font-semibold">{header}</h3>
      <div class="mx-auto flex w-full max-w-md items-end justify-center gap-3 sm:gap-4">
        {second && <PodiumColumn placement={second} rank={2} />}
        {first && <PodiumColumn placement={first} rank={1} />}
        {third && <PodiumColumn placement={third} rank={3} />}
      </div>
    </div>
  );
}
