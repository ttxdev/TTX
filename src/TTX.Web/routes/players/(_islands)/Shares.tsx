import {
  formatName,
  formatShareAmount,
  formatValue,
} from "@/lib/formatting.ts";
import { PlayerShareDto } from "@/lib/api.ts";
import Card from "@/components/Card.tsx";

export default function Shares(props: { shares: PlayerShareDto[] }) {
  const shares = props.shares.sort((a, b) =>
    b.creator.value * b.quantity - a.creator.value * a.quantity
  );
  return (
    <Card title="Shares">
      {shares.length > 0 && (
        <table class="table">
          <tbody>
            {shares.map((share) => {
              return (
                <tr
                  key={`share-${share.creator.id}`}
                  class="flex flex-row justify-between rounded-md py-1"
                >
                  <td class="flex items-center justify-center gap-3 p-0 py-3">
                    <a
                      href={`/creators/${share.creator.slug}`}
                      class="flex flex-col"
                    >
                      <img
                        alt={share.creator.name}
                        src={share.creator.avatar_url}
                        class="size-10 rounded-full"
                      />
                    </a>
                    <div class="flex flex-col">
                      <a
                        href={`/creators/${share.creator.slug}`}
                        class="text-lg font-semibold text-violet-500 hover:underline"
                      >
                        {formatName(share.creator.name)}
                      </a>
                      <span class="text-sm">
                        {formatShareAmount(share.quantity)} @{" "}
                        {formatValue(share.creator.value)}
                      </span>
                    </div>
                  </td>
                  <td class="flex flex-col items-center justify-center p-2 font-bold">
                    <div class="w-full text-right text-lg opacity-55">
                      {formatValue(share.quantity * share.creator.value)}
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}
      {shares.length === 0 && (
        <div class="flex items-center justify-center p-6">
          <b>No shares yet</b>
        </div>
      )}
    </Card>
  );
}
