import Card from "@/components/Card.tsx";
import { CreatorTransactionDto } from "@/lib/api.ts";
import {
  formatShareAmount,
  formatTxAction,
  formatValue,
} from "@/lib/formatting.ts";
import TimeStamp from "../../../components/TimeStamp.tsx";

export default function LatestTransactions(
  { transactions }: { transactions: CreatorTransactionDto[] },
) {
  return (
    <Card title="Latest Transactions">
      {transactions.length === 0 && (
        <div class="flex items-center justify-center p-6">
          <b>No transactions yet</b>
        </div>
      )}
      {transactions.length > 0 && (
        <table class="table">
          <tbody>
            {transactions.map((tx) => {
              const href = `/players/${tx.player.slug}`;
              return (
                <tr
                  key={`tx-${tx.id}`}
                  class="flex flex-row justify-between rounded-md py-1 md:p-2"
                >
                  <td class="flex items-center justify-center gap-3">
                    <a
                      href={href}
                      class="flex flex-col"
                    >
                      <img
                        alt=""
                        src={tx.player.avatar_url}
                        class="size-10 rounded-full"
                      />
                    </a>
                    <div class="flex flex-col">
                      <span class="text-xl font-semibold">
                        {formatTxAction(tx.action)}
                      </span>
                      <a
                        href={href}
                        class="text-sm text-violet-500 hover:underline"
                      >
                        {tx.player.name}
                      </a>
                    </div>
                  </td>
                  <td class="flex flex-col items-center justify-center p-2 text-right font-bold">
                    <span class="text-md md:text-xl">
                      {formatShareAmount(tx.quantity)} @ {formatValue(tx.value)}
                    </span>
                    <div class="w-full text-right opacity-55">
                      <TimeStamp date={tx.created_at.toString()} />
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
