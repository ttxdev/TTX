import { useCallback, useEffect, useRef, useState } from "preact/hooks";
import {
  animate,
  AnimatePresence,
  motion,
  useMotionValue,
  useMotionValueEvent,
} from "motion/react";
import { toast } from "@/lib/toast.ts";
import { formatTicker, formatValue } from "@/lib/formatting.ts";
import { getApiClient } from "@/lib/index.ts";
import {
  CreateTransactionDto,
  type CreatorPartialDto,
  TransactionAction,
} from "@/lib/api.ts";
import { State } from "../../../utils.ts";

const FEE_RATE = 0.0;
const BUY_LIMIT = 1000;

const TweenedText = (
  { value, formatter = (v: number) => v.toString() }: {
    value: any;
    formatter?: (v: number) => string;
  },
) => {
  const ref = useRef<HTMLSpanElement>(null);
  useMotionValueEvent(value, "change", (latest) => {
    // @ts-ignore
    if (ref.current) ref.current.textContent = formatter(latest);
  });
  return <span ref={ref}>{formatter(value.get())}</span>;
};

export default function OrderModal(
  props: {
    creator: CreatorPartialDto;
    state: State;
    show: boolean;
    price: number;
    type: TransactionAction;
    onClose: () => void;
  },
) {
  const [userOwns, setUserOwns] = useState(0);
  const [userBalance, setUserBalance] = useState(0);
  const [amount, setAmount] = useState<number | undefined>(undefined);
  const [isLoading, setIsLoading] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [transactionData, setTransactionData] = useState({
    price: 0,
    amount: 0,
  });

  const mPrice = useMotionValue(props.price);
  const mCost = useMotionValue(0);
  const mFee = useMotionValue(0);
  const mTotal = useMotionValue(0);
  const mAmount = useMotionValue(0);
  const mUserOwns = useMotionValue(0);
  const mUserBalance = useMotionValue(0);

  const client = getApiClient(props.state.token);

  useEffect(() => {
    const init = async () => {
      setIsLoading(true);
      try {
        const user = await client.getSelf();
        const balance = user.credits;
        const owns = user.shares.find((s) =>
          s.creator.slug === props.creator.slug
        )?.quantity || 0;

        setUserBalance(balance);
        setUserOwns(owns);
        animate(mUserBalance, balance, { duration: 1 });
        animate(mUserOwns, owns, { duration: 1 });
      } catch (e) {
        toast.error("Error fetching user data.");
      } finally {
        setIsLoading(false);
      }
    };

    init();
  }, []);

  useEffect(() => {
    animate(mPrice, props.price, { duration: 0.3 });
  }, [props.price]);

  useEffect(() => {
    const amt = amount ?? 0;
    const currentPrice = props.price;

    animate(mAmount, amt, { duration: 0.3 });
    animate(mCost, amt * currentPrice, { duration: 0.3 });

    if (props.type === TransactionAction.Buy) {
      animate(mFee, amt * currentPrice * FEE_RATE, { duration: 0.3 });
      animate(mTotal, amt * currentPrice * (1 + FEE_RATE), { duration: 0.3 });
    } else {
      animate(mFee, 0, { duration: 0.3 });
      animate(mTotal, amt * currentPrice, { duration: 0.3 });
    }
  }, [amount, props.type, props.price]);

  const getMaxPossible = useCallback(() => {
    if (props.type === TransactionAction.Buy) {
      return Math.max(
        0,
        Math.min(
          Math.floor(userBalance / (props.price * (1 + FEE_RATE))),
          BUY_LIMIT - userOwns,
        ),
      );
    }
    return userOwns;
  }, [props.type, props.price, userBalance, userOwns]);

  const handleAmountUpdate = useCallback((val: number | undefined) => {
    if (val === undefined || isNaN(val)) {
      setAmount(undefined);
      return;
    }
    const max = getMaxPossible();
    const clamped = Math.max(0, Math.min(val, max));
    setAmount(clamped);
  }, [getMaxPossible]);

  useEffect(() => {
    if (amount !== undefined) {
      handleAmountUpdate(amount);
    }
  }, [props.price, handleAmountUpdate]);

  const cannotAfford = amount === undefined
    ? true
    : props.type === TransactionAction.Buy
    ? (amount * props.price * (1 + FEE_RATE) > userBalance ||
      userOwns + amount > BUY_LIMIT)
    : amount > userOwns;

  const handleTransaction = async () => {
    if (!amount) return;

    setIsLoading(true);
    try {
      await client.placeOrder(
        new CreateTransactionDto({
          creator: props.creator.slug,
          action: props.type,
          amount: amount,
        }),
      );

      const newBalance = props.type === TransactionAction.Buy
        ? userBalance - (props.price * amount)
        : userBalance + (props.price * amount);
      const newOwns = props.type === TransactionAction.Buy
        ? userOwns + amount
        : userOwns - amount;

      setUserBalance(newBalance);
      setUserOwns(newOwns);
      setTransactionData({ price: props.price, amount: amount });

      animate(mUserBalance, newBalance, { duration: 1 });
      animate(mUserOwns, newOwns, { duration: 1 });
      setIsSuccess(true);
    } catch (e) {
      toast.error("Transaction failed.");
      console.error(e);
    } finally {
      setIsLoading(false);
    }
  };

  const isBuy = props.type === TransactionAction.Buy;
  const maxPossible = getMaxPossible();
  const accentBtn = isBuy
    ? "bg-green-600 hover:bg-green-700"
    : "bg-red-600 hover:bg-red-700";
  const accentSoft = isBuy
    ? "bg-green-500/15 text-green-500"
    : "bg-red-500/15 text-red-500";

  const fillPercent = (pct: number) =>
    handleAmountUpdate(Math.floor(maxPossible * pct));

  let hint: string | null = null;
  if (amount !== undefined && amount > 0) {
    if (isBuy && amount * props.price * (1 + FEE_RATE) > userBalance) {
      hint = "Not enough credits for this order.";
    } else if (isBuy && userOwns + amount > BUY_LIMIT) {
      hint = `You can hold at most ${BUY_LIMIT} shares.`;
    } else if (!isBuy && amount > userOwns) {
      hint = `You only own ${userOwns} share${userOwns === 1 ? "" : "s"}.`;
    }
  }

  return (
    <AnimatePresence>
      <div className="modal modal-open z-[100]">
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          onClick={() => !isLoading && props.onClose()}
          className="fixed inset-0 bg-black/30 backdrop-blur-md"
        />

        <motion.div
          initial={{ scale: 0.9, opacity: 0, y: 20 }}
          animate={{ scale: 1, opacity: 1, y: 0 }}
          exit={{ scale: 0.9, opacity: 0, y: 20 }}
          className="modal-box relative z-10 w-full max-w-md rounded-2xl p-6 shadow-lg"
        >
          {!isLoading && (
            <button
              type="button"
              className="btn btn-sm btn-circle btn-ghost absolute top-3 right-3"
              onClick={() => props.onClose()}
            >
              ✕
            </button>
          )}

          {/* Creator header — shared by both states */}
          <div className="mb-5 flex items-center gap-3 pr-8">
            <img
              src={props.creator.avatar_url}
              alt=""
              className="size-11 shrink-0 rounded-full"
            />
            <div className="flex min-w-0 flex-col">
              <span className="truncate font-semibold">
                {props.creator.name}
              </span>
              <span className="font-mono text-xs opacity-60">
                {formatTicker(props.creator.ticker)}
              </span>
            </div>
            <span
              className={`badge ml-auto shrink-0 border-none font-semibold ${accentSoft}`}
            >
              {isBuy ? "Buy" : "Sell"}
            </span>
          </div>

          {isSuccess
            ? (
              <div className="flex flex-col items-center gap-4 py-2 text-center">
                <div className="flex size-14 items-center justify-center rounded-full bg-green-500/15 text-3xl text-green-500">
                  ✓
                </div>
                <h3 className="text-2xl font-bold">
                  {isBuy ? "Purchase" : "Sale"} Successful
                </h3>
                <p className="opacity-70">
                  You {isBuy ? "bought" : "sold"} {transactionData.amount}{" "}
                  {formatTicker(props.creator.ticker)} at{" "}
                  {formatValue(transactionData.price)}/sh.
                </p>
                <div className="grid w-full grid-cols-2 gap-3">
                  <div className="border-base-content/10 rounded-xl border p-3">
                    <p className="text-xs tracking-widest uppercase opacity-50">
                      Shares Owned
                    </p>
                    <p className="text-xl font-semibold">
                      <TweenedText
                        value={mUserOwns}
                        formatter={(v) => Math.round(v).toString()}
                      />
                    </p>
                  </div>
                  <div className="border-base-content/10 rounded-xl border p-3">
                    <p className="text-xs tracking-widest uppercase opacity-50">
                      New Balance
                    </p>
                    <p className="text-xl font-semibold">
                      <TweenedText
                        value={mUserBalance}
                        formatter={formatValue}
                      />
                    </p>
                  </div>
                </div>
                <button
                  type="button"
                  className="btn mt-2 w-full rounded-xl border-none font-semibold"
                  onClick={() => props.onClose()}
                >
                  Done
                </button>
              </div>
            )
            : (
              <div className="flex flex-col gap-4">
                {/* Context: what the visitor has to work with */}
                <div className="flex items-center justify-between text-sm">
                  <span className="opacity-60">
                    {isBuy ? "Available credits" : "Shares owned"}
                  </span>
                  <span className="font-semibold">
                    {isBuy ? formatValue(userBalance) : userOwns}
                  </span>
                </div>

                {/* Quantity stepper */}
                <div className="join w-full">
                  <button
                    className="btn join-item border-base-content/20"
                    type="button"
                    onClick={() => handleAmountUpdate((amount ?? 0) - 1)}
                    disabled={!amount || amount <= 0}
                    aria-label="Decrease"
                  >
                    −
                  </button>
                  <input
                    type="number"
                    className="input input-bordered join-item flex-1 text-center focus:outline-none [appearance:textfield] [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none"
                    value={amount ?? ""}
                    onInput={(e) =>
                      handleAmountUpdate(parseInt(e.currentTarget.value))}
                    placeholder="0"
                    min="0"
                  />
                  <button
                    className="btn join-item border-base-content/20"
                    type="button"
                    onClick={() => handleAmountUpdate((amount ?? 0) + 1)}
                    disabled={amount !== undefined && amount >= maxPossible}
                    aria-label="Increase"
                  >
                    +
                  </button>
                </div>

                {/* Quick fill */}
                <div className="grid grid-cols-4 gap-2">
                  {[0.25, 0.5, 0.75, 1].map((pct) => (
                    <button
                      key={pct}
                      type="button"
                      className="btn btn-sm border-base-content/20 hover:border-purple-400 hover:text-purple-400"
                      onClick={() => fillPercent(pct)}
                      disabled={maxPossible === 0}
                    >
                      {pct === 1 ? "Max" : `${pct * 100}%`}
                    </button>
                  ))}
                </div>

                {/* Order summary */}
                <div className="border-base-content/10 bg-base-200/40 space-y-2 rounded-xl border p-4">
                  <div className="flex justify-between text-sm">
                    <span className="opacity-70">Price per share</span>
                    <TweenedText value={mPrice} formatter={formatValue} />
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="opacity-70">Shares</span>
                    <span>
                      {amount === undefined ? "—" : (
                        <TweenedText
                          value={mAmount}
                          formatter={(v) => Math.round(v).toString()}
                        />
                      )}
                    </span>
                  </div>
                  <div className="border-base-content/10 border-t pt-2" />
                  <div className="flex justify-between text-lg font-bold">
                    <span>Total</span>
                    <TweenedText value={mTotal} formatter={formatValue} />
                  </div>
                </div>

                <button
                  type="button"
                  className={`btn w-full rounded-xl border-none font-bold text-white ${accentBtn}`}
                  disabled={isLoading || cannotAfford || !amount || amount < 1}
                  onClick={handleTransaction}
                >
                  {isLoading
                    ? <span className="loading loading-spinner" />
                    : `${isBuy ? "Buy" : "Sell"} Shares`}
                </button>

                {hint && (
                  <p className="-mt-1 text-center text-sm text-red-500">
                    {hint}
                  </p>
                )}
              </div>
            )}
        </motion.div>
      </div>
    </AnimatePresence>
  );
}
