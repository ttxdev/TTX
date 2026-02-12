import { PortfolioSnapshotDto, TransactionAction, VoteDto } from "./api.ts";

export function formatToChart(
  value: number,
  history: VoteDto[] | PortfolioSnapshotDto[],
) {
  const labels: string[] = [];
  const values: number[] = [];
  history.forEach((item) => {
    labels.push(item.time.toString());
    values.push(item.value);
  });

  if (history.length === 0) {
    const now = new Date().toString();
    values.push(value);
    labels.push(now);
    values.push(value);
    labels.push(now);
  }

  return { labels, values };
}

export function formatValue(value: number) {
  const abs = Math.abs(value);
  if (abs >= 1_000_000_000_000) {
    return `$${(value / 1_000_000_000_000).toPrecision(2)} T`;
  } else if (abs >= 1_000_000_000) {
    return `$${(value / 1_000_000_000).toPrecision(2)} B`;
  } else if (abs >= 1_000_000) {
    return `$${(value / 1_000_000).toPrecision(2)} M`;
  } else {
    return `$${
      abs.toLocaleString("en-US", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
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

export function formatName(creator: string) {
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
