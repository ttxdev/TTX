<script lang="ts">
	import type { PageData } from './$types';
	import { Tween } from 'svelte/motion';
	import { quintOut } from 'svelte/easing';
	import WinnerModal from '$lib/components/WinnerModal.svelte';
	import { onMount } from 'svelte';
	import type { Rarity } from './+page.server';

	type GameState = 'idle' | 'spinning' | 'complete' | 'showing_modal';

	const { data }: { data: PageData } = $props();

	const rarityColors: Record<Rarity, string> = {
		pennies: '#9e9e9e',
		normal: '#00E676',
		rare: '#2979FF',
		epic: '#FFD700'
	};

	const rarityGlow: Record<Rarity, string> = {
		pennies: '0px 0px 8px 2px',
		normal: '0px 0px 15px 5px',
		rare: '0px 0px 20px 8px',
		epic: '0px 0px 25px 10px'
	};

	let winnerRef: HTMLDivElement | null = $state(null);
	let gameState: GameState = $state('idle');
	let isMobile = $state(false);

	let position = new Tween(0, {
		duration: 7000,
		easing: quintOut
	});

	function checkMobile() {
		if (!window) return;
		isMobile = window.innerWidth < 768;
	}

	onMount(() => {
		if (!window) return;
		window.addEventListener('resize', checkMobile);
		checkMobile();
		return () => window.removeEventListener('resize', checkMobile);
	});

	const spinnerItems = $derived(data.choices);

	function spin() {
		if (!winnerRef) return;
		if (gameState !== 'idle') return;

		gameState = 'spinning';
		// This just offsets the winner ref to the left so the winner is always in the center of the screen and card
		const targetPosition = winnerRef.offsetLeft - 150 * (isMobile ? 1 : 3);

		position.set(targetPosition).then(() => {
			gameState = 'complete';
			setTimeout(() => {
				gameState = 'showing_modal';
			}, 300);
		});
	}

	// Just makes easier to pass the winner to the modal
	const winner = $derived(data.choices[data.winnerIndex]);
</script>

<div class="my-auto flex flex-col items-center p-4 md:p-8">
	<h1
		class="mb-8 bg-gradient-to-r from-amber-300 via-yellow-500 to-amber-300 bg-clip-text text-3xl font-bold text-transparent md:text-5xl"
	>
		TTX GAMBA
	</h1>

	<div class="flex w-full flex-col items-center md:max-w-4xl">
		<div
			class="relative mb-8 h-52 w-full overflow-hidden rounded-2xl border-2 border-violet-600 bg-black shadow-[0_0_30px_rgba(124,58,237,0.7)]"
		>
			<div
				class="pulse-glow absolute top-0 left-1/2 z-10 h-full w-2 -translate-x-1/2 bg-gradient-to-b from-yellow-300 via-amber-400 to-yellow-300"
			></div>
			<div
				class="absolute flex py-4 transition-transform"
				style="transform: translateX({-position.current}px);"
			>
				{#each spinnerItems as item, i (i)}
					<div
						class="mx-4 flex h-44 basis-36 flex-col items-center justify-center rounded-xl border-2 bg-gradient-to-b from-slate-900 to-black p-3 transition-all"
						class:winner={i === data.winnerIndex &&
							gameState !== 'idle' &&
							gameState !== 'spinning'}
						style="border-color: {rarityColors[item.rarity_class]};"
					>
						{#if i === data.winnerIndex}
							<div bind:this={winnerRef}></div>
						{/if}
						<div
							class="mb-3 h-28 w-28 rounded-full border-2 bg-black p-1 transition-all"
							style="
								border-color: {rarityColors[item.rarity_class]};
								box-shadow: {rarityGlow[item.rarity_class]} {rarityColors[item.rarity_class]};
							"
						>
							<img
								class="h-full w-full rounded-full object-cover"
								src={item.avatar_url}
								alt={item.name}
							/>
						</div>
						<div
							class="bg-gradient-to-r from-amber-200 via-yellow-300 to-amber-200 bg-clip-text text-xl font-bold text-transparent uppercase"
						>
							${item.ticker}
						</div>
					</div>
				{/each}
			</div>
		</div>

		<div class="mb-8 flex gap-6">
			<button
				class="cursor-pointer rounded-lg bg-gradient-to-r from-amber-400 to-yellow-500 px-8 py-5 text-xl font-bold text-black transition-all hover:not-disabled:scale-105 disabled:cursor-not-allowed disabled:opacity-50"
				onclick={spin}
				disabled={gameState !== 'idle'}
			>
				{gameState === 'spinning' ? 'Spinning...' : gameState !== 'idle' ? 'Complete' : 'SPIN'}
			</button>
		</div>
	</div>
</div>

{#if gameState === 'showing_modal'}
	<WinnerModal {winner} {rarityColors} {rarityGlow} />
{/if}

<style>
	.winner {
		transform: scale(1.08);
		box-shadow: 0 0 30px rgba(251, 191, 36, 0.7);
		z-index: 5;
	}

	.pulse-glow {
		animation: pulseGlow 1.5s infinite;
		box-shadow: 0 0 15px 5px rgba(251, 191, 36, 0.7);
	}

	@keyframes pulseGlow {
		0% {
			box-shadow: 0 0 15px 5px rgba(251, 191, 36, 0.7);
		}
		50% {
			box-shadow: 0 0 25px 8px rgba(251, 191, 36, 0.9);
		}
		100% {
			box-shadow: 0 0 15px 5px rgba(251, 191, 36, 0.7);
		}
	}
</style>
