import {
  formatName,
  formatShareAmount,
  formatValue,
} from "@/lib/formatting.ts";
import { PlayerShareDto } from "@/lib/api.ts";
import Card from "@/components/Card.tsx";

export default function Shares(props: { shares: PlayerShareDto[] }) {
  const shares = props.shares.toSorted((a, b) =>
    b.creator.value * b.quantity - a.creator.value * a.quantity
  );
  return (
    <Card title="Holdings">
      {shares.length === 0 && (
        <div class="flex items-center justify-center p-6">
          <b>No shares yet</b>
        </div>
      )}
      {shares.length > 0 && (
        <table class="table">
          <tbody>
            {shares.map((share, index) => {
              const href = `/creators/${share.creator.slug}`;
              return (
                <tr
                  key={`share-${share.creator.id}`}
                  class="hover:bg-base-300/40 flex flex-row items-center justify-between rounded-lg py-1 transition-colors md:p-2"
                >
                  <td class="flex items-center justify-center gap-3">
                    <span class="w-5 text-center text-sm font-bold opacity-40">
                      {index + 1}
                    </span>
                    <a href={href} class="flex shrink-0 flex-col">
                      <img
                        alt={share.creator.name}
                        src={share.creator.avatar_url}
                        class="size-10 shrink-0 rounded-full object-cover"
                      />
                    </a>
                    <a
                      href={href}
                      class="text-lg font-semibold text-violet-500 hover:underline"
                    >
                      {formatName(share.creator.name)}
                    </a>
                  </td>
                  <td class="flex flex-col items-center justify-center p-2 text-right font-bold">
                    <span class="text-md md:text-xl">
                      {formatValue(share.quantity * share.creator.value)}
                    </span>
                    <div class="w-full text-right opacity-55">
                      {formatShareAmount(share.quantity)} @{" "}
                      {formatValue(share.creator.value)}
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}
    </Card>
  );
}
