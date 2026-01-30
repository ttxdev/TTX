import { CreatorPartialDto } from "./api.ts";

export type SearchResult = {
  id: number;
  name: string;
  ticker?: string;
  type: "player" | "creator";
  slug: string;
  avatar_url: string;
};

export function getRecentCreators(): SearchResult[] {
  const recent = globalThis.localStorage.getItem("recent");
  if (!recent) return [];

  return JSON.parse(recent);
}

export function addRecentCreator(creator: CreatorPartialDto) {
  const recent = getRecentCreators();
  if (recent.length === 10) {
    recent.shift();
  }

  recent.push({
    id: creator.id,
    name: creator.name,
    ticker: creator.ticker,
    type: "creator",
    slug: creator.slug,
    avatar_url: creator.avatar_url,
  });

  globalThis.localStorage.setItem("recent", JSON.stringify(recent));
}
