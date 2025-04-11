export type Creator = {
	id: number;
	created_at: string; // ISO date string
	updated_at: string; // ISO date string
	name: string;
	slug: string;
	ticker: string;
	url: string;
	avatar_url: string;
	value: number;
	history: { time: number; value: number }[];
};
