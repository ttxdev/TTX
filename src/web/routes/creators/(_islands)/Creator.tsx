import {
  CreatorDto,
  CreatorShareDto,
  CreatorTransactionDto,
  TransactionAction,
  VoteDto,
} from "@/lib/api.ts";
import OrderModal from "./OrderModal.tsx";
import CreatorCard from "./CreatorCard.tsx";
import LatestTransactions from "../(_components)/LatestTransactions.tsx";
import Shares from "../(_components)/Shares.tsx";
import { State } from "../../../utils.ts";
import { useSignal } from "@preact/signals";
import { useEffect } from "preact/hooks";
import { createHub } from "../../../lib/ws.ts";
import { toast } from "../../../lib/toast.ts";
import { getApiClient } from "../../../lib/index.ts";

export type Interval = "24h" | "12h" | "6h" | "1h";
export type CreatorProps = {
  state: State;
  url: URL;
  creator: CreatorDto;
  shares: CreatorShareDto[];
  transactions: CreatorTransactionDto[];
  interval: Interval;
  isPlayer: boolean;
  currentUserIsCreator: boolean;
};

function IntervalSelector(props: { interval: Interval }) {
  const intervals: { label: string; value: Interval }[] = [
    { label: "24h", value: "24h" },
    { label: "12h", value: "12h" },
    { label: "6h", value: "6h" },
    { label: "1h", value: "1h" },
  ];

  return (
    <div class="join w-full p-0.5 sm:w-auto sm:p-1">
      {intervals.map(({ label, value }) => (
        <a
          href={`?interval=${value}`}
          class={`join-item btn btn-xs sm:btn-sm min-w-14 border-none text-xs transition-all duration-200 ease-in-out first:rounded-l-2xl last:rounded-r-2xl sm:min-w-[4rem] sm:text-sm
   			${
            props.interval === value
              ? "bg-purple-600 font-medium text-white shadow-md hover:bg-purple-700"
              : "hover:bg-base-300 text-base-content/70 hover:text-base-content bg-transparent"
          }`}
        >
          {label}
        </a>
      ))}
    </div>
  );
}

export default function Creator(props: CreatorProps) {
  const showOrderModal = useSignal<
    TransactionAction | null
  >(null);
  const shares = useSignal(props.shares);
  const transactions = useSignal(props.creator.transactions);
  const value = useSignal(props.creator.value);
  const history = useSignal<VoteDto[]>(props.creator.history);
  const client = getApiClient(props.state.token);

  const addVote = ({ vote }: { vote: VoteDto }) => {
    value.value = vote.value;
    history.value = [...history.value, vote];
  };

  useEffect(() => {
    const hub = createHub("votes", props.state.token);
    hub.on("UpdateCreatorValueEvent", addVote);
    hub.invoke("SetCreator", props.creator.id);
    hub.start().catch(console.error);
    return () => {
      hub.stop();
    };
  }, [props.creator.id, props.state.token]);

  function setOrderModal(action: TransactionAction | null) {
    if (!props.state.token) {
      toast.error("Please login to place an order.");
      return;
    }

    showOrderModal.value = action;
  }

  async function optOut() {
    await client.optOutCreator(props.creator.slug);
    toast.success("Creator removed.");
    globalThis.location.replace("/");
  }

  return (
    <div>
      {showOrderModal.value && (
        <OrderModal
          show
          type={showOrderModal.value}
          creator={props.creator}
          state={props.state}
          price={value.value}
          onClose={() => setOrderModal(null)}
        />
      )}

      <div>
        <section class="mx-auto flex w-full max-w-250 flex-col gap-4 p-4">
          <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <h1 class="text-2xl font-bold">{props.creator.name}</h1>
            <div class="flex flex-col items-stretch gap-2 sm:flex-row sm:items-center">
              {props.isPlayer && (
                <a
                  href={`/players/${props.creator.slug}`}
                  class="bg-primary hover:bg-primary/80 inline-flex items-center justify-center gap-1 rounded-lg px-2 py-1 text-xs font-medium text-white transition-colors sm:text-sm"
                >
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    class="h-3 w-3 sm:h-4 sm:w-4"
                    viewBox="0 0 20 20"
                    fill="currentColor"
                  >
                    <path
                      fill-rule="evenodd"
                      d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-8.707l-3-3a1 1 0 00-1.414 0l-3 3a1 1 0 001.414 1.414L9 9.414V13a1 1 0 102 0V9.414l1.293 1.293a1 1 0 001.414-1.414z"
                      clip-rule="evenodd"
                    />
                  </svg>
                  <p>Switch to player profile</p>
                </a>
              )}
              <IntervalSelector interval={props.interval} />
            </div>
          </div>
          <CreatorCard
            state={props.state}
            creator={props.creator}
            value={value.value}
            history={history.value}
          />
          <div class="flex justify-end">
            {props.currentUserIsCreator && (
              <button
                onClick={() => optOut()}
                type="button"
                class="inline-flex cursor-pointer items-center gap-1 rounded-lg bg-red-500 px-2 py-1 text-xs font-medium text-white transition-colors hover:bg-red-600 sm:text-sm"
              >
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  class="h-3 w-3 sm:h-4 sm:w-4"
                  viewBox="0 0 20 20"
                  fill="currentColor"
                >
                  <path
                    fill-rule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                    clip-rule="evenodd"
                  />
                </svg>
                <p>Opt out of TTX</p>
              </button>
            )}
            {!props.currentUserIsCreator && !props.state.token && (
              <div class="flex flex-row gap-2 text-sm text-gray-500 items-center">
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  class="h-4 w-4"
                  viewBox="0 0 20 20"
                  fill="currentColor"
                >
                  <path
                    fill-rule="evenodd"
                    d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z"
                    clip-rule="evenodd"
                  />
                </svg>
                <div class="flex flex-col text-center">
                  <p>
                    Are you {props.creator.name}?
                  </p>
                  <p>
                    <a
                      href={`/login?from=${
                        encodeURIComponent(props.url.pathname)
                      }`}
                      class="text-violet-400 hover:underline"
                    >
                      Log in
                    </a>{" "}
                    to opt out of TTX.
                  </p>
                </div>
              </div>
            )}
          </div>
          <div class="bg-base-200/40 flex w-full flex-col items-center gap-3 rounded-2xl p-6">
            {!props.creator.stream_status.is_live && (
              <div class="flex flex-col items-center justify-center gap-1 text-center">
                <h2 class="text-xl font-bold">
                  {props.creator.name} is offline
                </h2>
                <p class="max-w-md text-sm opacity-70">
                  You can only buy or sell a creator's stock while they are
                  live.
                </p>
              </div>
            )}
            <div class="join">
              <button
                disabled={!props.creator.stream_status.is_live}
                class={`btn btn-lg join-item h-10 rounded-l-2xl border-none px-8 ${
                  props.creator.stream_status.is_live
                    ? "bg-green-600 text-white hover:bg-green-700"
                    : "text-gray-500"
                }`}
                type="button"
                onClick={() => setOrderModal(TransactionAction.Buy)}
              >
                Buy
              </button>
              <button
                disabled={!props.creator.stream_status.is_live}
                class={`btn btn-lg join-item h-10 rounded-r-2xl border-none px-8 ${
                  props.creator.stream_status.is_live
                    ? "bg-red-600 text-white hover:bg-red-700"
                    : "text-gray-500"
                }`}
                type="button"
                onClick={() => setOrderModal(TransactionAction.Sell)}
              >
                Sell
              </button>
            </div>
            {props.creator.stream_status.is_live && (
              <p class="text-xs opacity-60">
                Trades execute at the current market price.
              </p>
            )}
          </div>
          <div class="mt-4 flex w-full items-center justify-between">
            <h2 class="text-2xl font-bold">Investors</h2>
          </div>
        </section>

        <section class="mx-auto flex w-full max-w-250 flex-col gap-4 px-4">
          <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
            <Shares shares={shares.value} price={props.creator.value} />
            <LatestTransactions transactions={transactions.value} />
          </div>
        </section>
      </div>
    </div>
  );
}
