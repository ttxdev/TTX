import type { UserData } from '$lib/auth';
import { writable } from 'svelte/store';

export const token = writable<string | null>(null);
export const user = writable<UserData | null>(null);
