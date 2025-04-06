import { browser } from '$app/environment';

const STORAGE_KEY = 'recentStreamers';
const MAX_RECENT_STREAMERS = 5;

export interface RecentStreamer {
	id: number;
	name: string;
	slug: string;
	avatar_url: string;
	ticker?: string;
	lastVisited: number;
}

export function addRecentStreamer(streamer: Omit<RecentStreamer, 'lastVisited'>) {
	if (!browser) return;

	const recentStreamers = getRecentStreamers();

	const filteredStreamers = recentStreamers.filter((s) => s.id !== streamer.id);

	const updatedStreamers = [{ ...streamer, lastVisited: Date.now() }, ...filteredStreamers].slice(
		0,
		MAX_RECENT_STREAMERS
	);

	localStorage.setItem(STORAGE_KEY, JSON.stringify(updatedStreamers));
}

export function getRecentStreamers(): RecentStreamer[] {
	if (!browser) return [];

	try {
		const stored = localStorage.getItem(STORAGE_KEY);
		return stored ? JSON.parse(stored) : [];
	} catch (error) {
		console.error('Error reading recent streamers:', error);
		return [];
	}
}

export function clearRecentStreamers() {
	if (!browser) return;
	localStorage.removeItem(STORAGE_KEY);
}
