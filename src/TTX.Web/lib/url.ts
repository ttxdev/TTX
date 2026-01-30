interface ToString {
  toString(): string;
}

export function nav(
  url: URL,
  query: Record<string, ToString>,
): string {
  const newUrl = new URL(url);
  Object.entries(query).forEach(([key, value]) => {
    newUrl.searchParams.set(key, value.toString());
  });

  return newUrl.toString();
}
