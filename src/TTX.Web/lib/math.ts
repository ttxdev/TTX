export function calculatePercentChange(history: { value: number }[]) {
  const oldest = history[0]?.value;
  const current = history[history.length - 1]?.value;
  if (!oldest) return 0;

  const change = ((current - oldest) / oldest) * 100;
  if (!isFinite(change)) return 99999;

  return isNaN(change) ? 0 : change;
}
