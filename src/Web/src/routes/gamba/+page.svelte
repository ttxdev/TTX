<script lang="ts">
	import type { PageProps } from './$types';
	import WinnerModal from '$lib/components/WinnerModal.svelte';
	import LootboxCard from '$lib/components/shared/LootboxCard.svelte';
	import GamblingWheel from '$lib/components/shared/GamblingWheel.svelte';
	import SpinButton from '$lib/components/shared/SpinButton.svelte';
	import { fade } from 'svelte/transition';

	const { data, form }: PageProps = $props();

	let gameState: 'idle' | 'spinning' | 'complete' | 'showing_modal' = $state('idle');

	function spin() {
		if (gameState !== 'idle') return;
		gameState = 'spinning';
		setTimeout(() => {
			gameState = 'complete';
			setTimeout(() => {
				gameState = 'showing_modal';
			}, 300);
		}, 10_000);
	}

	const winner = $derived(form?.rarities?.[form?.winnerIndex]);
</script>

<div class="my-auto flex flex-col items-center p-4 md:p-8">
	<h1
		class="mb-8 bg-gradient-to-r from-amber-300 via-yellow-500 to-amber-300 bg-clip-text text-3xl font-bold text-transparent md:text-5xl"
	>
		TTX GAMBA
	</h1>

	<div class="flex w-full flex-col items-center md:max-w-4xl">
		{#if form?.winner}
			<div class="mb-8 flex w-full flex-col items-center gap-6" transition:fade>
				<GamblingWheel items={form.rarities} winnerIndex={form.winnerIndex} {gameState} />
				<SpinButton {gameState} onSpin={spin} />
			</div>
		{:else}
			{#if data.unopenedBoxes.length > 0}
				<h2 class="mb-4 text-xl font-bold text-amber-400">Available Lootboxes</h2>
				<div class="mb-12 grid grid-cols-1 gap-6 sm:grid-cols-2 md:grid-cols-3">
					{#each data.unopenedBoxes as box (box.id)}
						<LootboxCard id={box.id} />
					{/each}
				</div>
			{/if}

			{#if data.openedBoxes.length > 0}
				<h2 class="mb-4 text-xl font-bold text-amber-400">Previous Rewards</h2>
				<div class="grid grid-cols-1 gap-6 sm:grid-cols-2 md:grid-cols-3">
					{#each data.openedBoxes as box (box.id)}
						<LootboxCard id={box.id} isOpen={true} result={box.result} />
					{/each}
				</div>
			{/if}

			{#if data.unopenedBoxes.length === 0 && data.openedBoxes.length === 0}
				<div class="text-center text-gray-400">
					<p class="text-xl">No lootboxes available</p>
					<p class="mt-2">Come back later to try your luck!</p>
				</div>
			{/if}
		{/if}
	</div>
</div>

{#if gameState === 'showing_modal' && winner}
	<WinnerModal winner={form?.winner!} rarity={winner.rarity} />
{/if}
