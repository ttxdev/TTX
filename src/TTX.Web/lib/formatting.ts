import { TransactionAction } from "./api.ts";

export function formatValue(value: number) {
  if (Math.abs(value) >= 1_000_000_000_000) {
    return `$${(value / 1_000_000_000_000).toFixed(2)} T`;
  } else if (Math.abs(value) >= 1_000_000_000) {
    return `$${(value / 1_000_000_000).toFixed(2)} B`;
  } else {
    return `$${
      Math.abs(value).toLocaleString("en-US", {
        minimumFractionDigits: 0,
        maximumFractionDigits: 0,
      })
    }`;
  }
}

export function formatTicker(ticker: string) {
  let result = "";
  if (ticker.startsWith("S")) {
    result += `$${ticker.slice(1)}`;
  } else {
    result += ticker;
  }

  return result.toUpperCase();
}

export function formatShareAmount(amount: number) {
  if (Math.abs(amount) >= 1_000_000_000) {
    return `${(amount / 1_000_000_000).toFixed(2)} B`;
  } else if (Math.abs(amount) >= 1_000_000) {
    return `${(amount / 1_000_000).toFixed(2)} M`;
  } else {
    return Math.abs(amount).toLocaleString("en-US", {
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    });
  }
}

export function formatCreatorString(creator: string) {
  if (creator.length > 12) {
    return `${creator.slice(0, 10)}...`;
  }
  return creator;
}

export function formatTxAction(action: TransactionAction): string {
  switch (action) {
    case TransactionAction.Buy:
      return "Bought";
    case TransactionAction.Sell:
      return "Sold";
    case TransactionAction.Open:
      return "Opened";
    default:
      return action;
  }
}
