<script lang="ts">
	import { enhance } from '$app/forms';

	type Result = {
		avatar_url: string;
		name: string;
		ticker: string;
	};

	const {
		id,
		isOpen = false,
		result
	}: { id: number; isOpen?: boolean; result?: Result } = $props();
</script>

<div
	class="lootbox-card group relative flex h-44 w-36 flex-col items-center justify-center overflow-hidden rounded-xl border-2 border-amber-500 bg-gradient-to-b from-slate-900 to-black p-3 transition-all"
>
	<div
		class="absolute -inset-1 bg-gradient-to-r from-amber-500/50 to-yellow-500/50 opacity-0 blur transition-opacity group-hover:opacity-100"
	></div>
	<div class="relative z-10 flex flex-col items-center">
		<div class="mb-2 text-center">
			<span class="text-lg font-bold text-amber-400">Lootbox #{id}</span>
		</div>
		{#if !isOpen}
			<form method="POST" action="?/openLootbox" use:enhance>
				<input type="hidden" name="lootboxId" value={id} />
				<button
					type="submit"
					class="cursor-pointer rounded bg-gradient-to-r from-amber-500 to-yellow-500 px-6 py-2 font-bold text-black transition-all hover:scale-105 hover:shadow-[0_0_15px_rgba(251,191,36,0.5)]"
				>
					Open
				</button>
			</form>
		{:else if result}
			<div class="mb-3 flex flex-col items-center">
				<div class="h-20 w-20 rounded-full border-2 border-amber-500 bg-black p-1">
					<img
						class="h-full w-full rounded-full object-cover"
						src={result.avatar_url}
						alt={result.name}
					/>
				</div>
				<div class="mt-2 text-center">
					<span class="text-sm font-bold text-amber-400">{result.name}</span>
					<div class="text-xs text-amber-500">${result.ticker}</div>
				</div>
			</div>
		{/if}
	</div>
</div>

<style>
	.lootbox-card {
		box-shadow: 0 0 15px rgba(251, 191, 36, 0.3);
	}

	.lootbox-card:hover {
		box-shadow: 0 0 25px rgba(251, 191, 36, 0.5);
	}
</style>
