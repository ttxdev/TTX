import {
  formatName,
  formatShareAmount,
  formatValue,
} from "@/lib/formatting.ts";
import Card from "../../../components/Card.tsx";
import PlayerPlacement, {
  Placement,
} from "../../players/(_components)/PlayerPlacement.tsx";
import { ICreatorShareDto } from "../../../lib/api.ts";

export default function Shares(
  { shares, price }: { shares: ICreatorShareDto[]; price: number },
) {
  const sortedHolders = shares.toSorted((a, b) => b.quantity - a.quantity);

  return (
    <Card title="Largest Holders">
      {shares.length === 0 && (
        <div class="flex items-center justify-center p-6">
          <b>No holders yet</b>
        </div>
      )}
      {shares.length > 0 && (
        <table class="table">
          <tbody>
            {sortedHolders.map((holder, index) => {
              const href = `/players/${holder.player.slug}`;
              return (
                <tr
                  key={`share-${holder.player.id}`}
                  class="flex flex-row justify-between rounded-md py-1 md:p-2"
                >
                  <td class="flex items-center justify-center gap-3">
                    <a href={href} class="flex flex-col">
                      <img
                        alt={holder.player.name}
                        src={holder.player.avatar_url}
                        class="size-10 rounded-full"
                      />
                    </a>
                    <div class="flex flex-col items-start">
                      <a
                        href={`/players/${holder.player.slug}`}
                        class="text-lg font-semibold text-violet-500 hover:underline"
                      >
                        {formatName(holder.player.name)}
                      </a>
                    </div>
                  </td>
                  <td class="flex flex-col items-center justify-center p-2 text-right font-bold">
                    <span class="text-md md:text-xl">
                      {formatValue(holder.quantity * price)}
                    </span>
                    <div class="w-full text-right opacity-55">
                      {formatShareAmount(holder.quantity)} shares
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
