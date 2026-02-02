import type { PlayerTransactionDto } from "@/lib/api.ts";
import {
  formatCreatorString,
  formatShareAmount,
  formatTxAction,
  formatValue,
} from "@/lib/formatting.ts";
import Card from "../../../components/Card.tsx";
import TimeStamp from "../../../components/TimeStamp.tsx";

export default function LatestTransactions(
  { transactions }: { transactions: PlayerTransactionDto[] },
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
              return (
                <tr
                  key={`transaction-${tx.id}`}
                  class="flex flex-row justify-between rounded-md py-1"
                >
                  <td class="flex items-center justify-center gap-3 p-0 py-3">
                    <a
                      href={`/creators/${tx.creator.slug}`}
                      class="flex flex-col"
                    >
                      <img
                        alt={tx.creator.name}
                        src={tx.creator.avatar_url}
                        class="size-10 rounded-full"
                      />
                    </a>
                    <div class="flex flex-col">
                      <span class="text-xl font-semibold">
                        {formatTxAction(tx.action)}
                      </span>
                      <a
                        href={`/creators/${tx.creator.slug}`}
                        class="text-sm text-violet-500 hover:underline"
                      >
                        {formatCreatorString(tx.creator.name)}
                      </a>
                    </div>
                  </td>
                  <td class="flex flex-col items-center justify-center p-2 font-bold">
                    <span class="w-full text-right text-lg opacity-55">
                      {formatShareAmount(tx.quantity)} / {formatValue(tx.value)}
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
