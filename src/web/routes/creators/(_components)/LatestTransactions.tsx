import Card from "@/components/Card.tsx";
import { CreatorTransactionDto, TransactionAction } from "@/lib/api.ts";
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
            {[...transactions].sort((a, b) =>
              new Date(b.created_at).getTime() -
              new Date(a.created_at).getTime()
            ).map((tx) => {
              const href = `/players/${tx.player.slug}`;
              const actionClass = tx.action === TransactionAction.Buy
                ? "text-green-500"
                : tx.action === TransactionAction.Sell
                ? "text-red-500"
                : "";
              return (
                <tr
                  key={`tx-${tx.id}`}
                  class="hover:bg-base-300/40 flex flex-row justify-between rounded-lg py-1 transition-colors md:p-2"
                >
                  <td class="flex min-w-0 flex-1 items-center gap-3">
                    <a
                      href={href}
                      class="flex shrink-0 flex-col"
                    >
                      <img
                        alt=""
                        src={tx.player.avatar_url}
                        class="size-10 shrink-0 rounded-full object-cover"
                      />
                    </a>
                    <div class="flex min-w-0 flex-col">
                      <span class={`text-xl font-semibold ${actionClass}`}>
                        {formatTxAction(tx.action)}
                      </span>
                      <a
                        href={href}
                        class="truncate text-sm text-violet-500 hover:underline"
                      >
                        {tx.player.name}
                      </a>
                    </div>
                  </td>
                  <td class="flex shrink-0 flex-col items-end justify-center p-2 text-right font-bold">
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
