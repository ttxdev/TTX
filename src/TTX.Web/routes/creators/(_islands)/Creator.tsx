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
import { useSignal, useSignalEffect } from "@preact/signals";
import { HubConnection } from "@microsoft/signalr";
import { createHub } from "../../../lib/signalr.ts";
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
          class={`join-item btn btn-xs sm:btn-sm min-w-14 border-none text-xs transition-all duration-200 ease-in-out first:rounded-l-3xl last:rounded-r-3xl sm:min-w-[4rem] sm:text-sm
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
  const hub = useSignal<HubConnection | null>(null);
  const shares = useSignal(props.shares);
  const transactions = useSignal(props.creator.transactions);
  const value = useSignal(props.creator.value);
  const history = useSignal<VoteDto[]>(props.creator.history);
  const client = getApiClient(props.state.token);

  const addVote = ({ vote }: { vote: VoteDto }) => {
    value.value = vote.value;
    history.value = [...history.value, vote];
  };

  useSignalEffect(() => {
    if (hub.value !== null) {
      return;
    }

    const newHub = createHub("votes", props.state.token);
    newHub.on("UpdateCreatorValueEvent", addVote);
    newHub.start()
      .then(() => {
        hub.value = newHub;
        return hub.value.invoke("SetCreator", props.creator.id);
      }).catch(console.error);
  });

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
          <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between">
            <h1 class="text-2xl font-bold">{props.creator.name}</h1>
            {props.isPlayer && (
              <div class="flex gap-2">
                <a
                  href={`/players/${props.creator.slug}`}
                  class="bg-primary hover:bg-primary/80 inline-flex items-center gap-1 rounded-md px-2 py-1 text-xs font-medium text-white transition-colors sm:text-sm"
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
              </div>
            )}
            <div class="flex justify-end">
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
                class="inline-flex cursor-pointer items-center gap-1 rounded-md bg-red-500 px-2 py-1 text-xs font-medium text-white transition-colors hover:bg-red-600 sm:text-sm"
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
          <div class="flex flex-col gap-4 md:flex-row">
            <div class="divider divider-vertical md:hidden"></div>
            <div class="join flex w-full flex-col items-center justify-center gap-2">
              {!props.creator.stream_status.is_live &&
                <div class="flex flex-col justify-center items-center gap-2">
                  <h1 class="text-2xl font-bold">{props.creator.name} is offline</h1>
                  <h2 class="text-sm text-center max-w-3/4">You cannot buy or sell a creators stock when they are offline.</h2>
                </div> 
              }
              <div class="join mt-2 md:mt-0">
                <button
                  disabled={!props.creator.stream_status.is_live}
                  class={`btn btn-lg h-10 rounded-l-2xl border-2 p-4 ${!props.creator.stream_status.is_live ? "text-gray-500" : "text-green-500"}`}
                  type="button"
                  onClick={() => setOrderModal(TransactionAction.Buy)}
                >
                  Buy
                </button>
                <button
                  disabled={!props.creator.stream_status.is_live}
                  class={`btn btn-lg h-10 rounded-r-2xl p-4 ${!props.creator.stream_status.is_live ? "text-gray-500" : "text-red-500"}`}
                  type="button"
                  onClick={() => setOrderModal(TransactionAction.Sell)}
                >
                  Sell
                </button>
              </div>
            </div>
          </div>
          <div class="divider divider-vertical md:hidden"></div>
          <div class="flex w-full items-center justify-between">
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
