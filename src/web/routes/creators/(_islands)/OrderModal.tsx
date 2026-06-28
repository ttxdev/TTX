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
  // --- Reactive State ---
  const [userOwns, setUserOwns] = useState(0);
  const [userBalance, setUserBalance] = useState(0);
  const [amount, setAmount] = useState<number | undefined>(undefined);
  const [isLoading, setIsLoading] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [transactionData, setTransactionData] = useState({
    price: 0,
    amount: 0,
  });

  // --- Motion Values ---
  const mPrice = useMotionValue(props.price);
  const mCost = useMotionValue(0);
  const mFee = useMotionValue(0);
  const mTotal = useMotionValue(0);
  const mAmount = useMotionValue(0);
  const mUserOwns = useMotionValue(0);
  const mUserBalance = useMotionValue(0);

  const client = getApiClient(props.state.token);

  // Initialize Data
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

  // Update motion price
  useEffect(() => {
    animate(mPrice, props.price, { duration: 0.3 });
  }, [props.price]);

  // Handle derived animations
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

  // Logic: Calculate maximum shares allowed
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

  // Handler: Standardized input with clamping
  const handleAmountUpdate = useCallback((val: number | undefined) => {
    if (val === undefined || isNaN(val)) {
      setAmount(undefined);
      return;
    }
    const max = getMaxPossible();
    const clamped = Math.max(0, Math.min(val, max));
    setAmount(clamped);
  }, [getMaxPossible]);

  // Effect: Update Clamp when price changes
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
              className="btn btn-sm btn-circle btn-ghost absolute top-2 right-2"
              onClick={() => props.onClose()}
            >
              ✕
            </button>
          )}

          {isSuccess
            ? (
              <div className="mt-4 flex flex-col items-center space-y-4 text-center">
                <div className="text-green-500 text-5xl">✓</div>
                <h3 className="text-2xl font-bold">
                  {props.type === TransactionAction.Buy ? "Purchase" : "Sale"}
                  {" "}
                  Successful
                </h3>
                <p>
                  You {props.type === TransactionAction.Buy ? "bought" : "sold"}
                  {" "}
                  {transactionData.amount} of{" "}
                  {formatTicker(props.creator.ticker)} at{" "}
                  {formatValue(transactionData.price)}/sh.
                </p>
                <div className="grid grid-cols-2 gap-4 w-full">
                  <div className="rounded-lg border p-2">
                    <p className="text-sm text-gray-500">Shares Owned</p>
                    <p className="text-xl font-semibold">
                      <TweenedText
                        value={mUserOwns}
                        formatter={(v) => Math.round(v).toString()}
                      />
                    </p>
                  </div>
                  <div className="rounded-lg border p-2">
                    <p className="text-sm text-gray-500">New Balance</p>
                    <p className="text-xl font-semibold">
                      <TweenedText
                        value={mUserBalance}
                        formatter={formatValue}
                      />
                    </p>
                  </div>
                </div>
              </div>
            )
            : (
              <>
                <h3 className="mb-4 text-2xl font-bold">
                  {props.type === TransactionAction.Buy ? "Buy" : "Sell"}{" "}
                  {formatTicker(props.creator.ticker)}
                </h3>
                <div className="flex flex-col space-y-4">
                  <div className="text-sm text-gray-500">
                    {props.type === TransactionAction.Buy
                      ? (
                        <>
                          Available: <strong>{formatValue(userBalance)}</strong>
                        </>
                      )
                      : (
                        <>
                          Owned: <strong>{userOwns}</strong>
                        </>
                      )}
                  </div>

                  <div className="join w-full">
                    <input
                      type="number"
                      className="input input-bordered join-item flex-1 border-purple-400 focus:outline-none rounded-l-2xl [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
                      value={amount ?? ""}
                      onInput={(e) =>
                        handleAmountUpdate(parseInt(e.currentTarget.value))}
                      placeholder="0"
                      min="0"
                    />

                    {/* Decrement Button with high-contrast divider */}
                    <button
                      className="btn join-item bg-purple-400 text-white border-none border-l-2 border-purple-600 disabled:bg-purple-900 disabled:text-white/40"
                      type="button"
                      onClick={() => handleAmountUpdate((amount ?? 0) - 1)}
                      disabled={!amount || amount <= 0}
                    >
                      −
                    </button>

                    {/* Increment Button with high-contrast divider */}
                    <button
                      className="btn join-item bg-purple-400 text-white border-none border-l-2 border-purple-600 disabled:bg-purple-900 disabled:text-white/40"
                      type="button"
                      onClick={() => handleAmountUpdate((amount ?? 0) + 1)}
                      disabled={amount !== undefined &&
                        amount >= getMaxPossible()}
                    >
                      +
                    </button>

                    {/* Max Button with high-contrast divider */}
                    <button
                      className="btn join-item bg-purple-400 text-white border-none border-l-2 border-purple-600 rounded-r-2xl"
                      type="button"
                      onClick={() => handleAmountUpdate(getMaxPossible())}
                    >
                      Max
                    </button>
                  </div>

                  <div className="rounded-lg border p-4 space-y-2">
                    <div className="flex justify-between text-sm">
                      <span>Price per Share</span>
                      <TweenedText value={mPrice} formatter={formatValue} />
                    </div>
                    <div className="flex justify-between text-sm">
                      <span>Shares</span>
                      <span>
                        {amount === undefined ? "—" : (
                          <TweenedText
                            value={mAmount}
                            formatter={(v) => Math.round(v).toString()}
                          />
                        )}
                      </span>
                    </div>
                    <hr />
                    <div className="flex justify-between text-lg font-bold">
                      <span>Total</span>
                      <TweenedText value={mTotal} formatter={formatValue} />
                    </div>
                  </div>

                  <button
                    type="button"
                    className="btn w-full rounded-2xl bg-purple-400 text-white font-black border-none"
                    disabled={isLoading || cannotAfford || !amount ||
                      amount < 1}
                    onClick={handleTransaction}
                  >
                    {isLoading
                      ? <span className="loading loading-spinner" />
                      : `${props.type} Shares`}
                  </button>
                </div>
              </>
            )}
        </motion.div>
      </div>
    </AnimatePresence>
  );
}
