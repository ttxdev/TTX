<script lang="ts">
	import { Tween } from 'svelte/motion';
	import { quintOut } from 'svelte/easing';
	import type { CreatorRarityDto } from '$lib/api';
	import { Rarity } from '$lib/api';

	type Props = {
		items: CreatorRarityDto[];
		winnerIndex: number;
		gameState: 'idle' | 'spinning' | 'complete' | 'showing_modal';
	};

	const { items, winnerIndex, gameState }: Props = $props();

	const rarityColors: Record<Rarity, string> = {
		[Rarity.Pennies]: '#9e9e9e',
		[Rarity.Common]: '#00E676',
		[Rarity.Rare]: '#2979FF',
		[Rarity.Epic]: '#FFD700'
	};

	const rarityGlow: Record<Rarity, string> = {
		[Rarity.Pennies]: '0px 0px 8px 2px',
		[Rarity.Common]: '0px 0px 15px 5px',
		[Rarity.Rare]: '0px 0px 20px 8px',
		[Rarity.Epic]: '0px 0px 25px 10px'
	};

	let winnerRef: HTMLDivElement | null = $state(null);
	let isMobile = $state(false);
	let position = new Tween(0, {
		duration: 10_000,
		easing: quintOut
	});

	function checkMobile() {
		isMobile = window.innerWidth < 768;
	}

	$effect(() => {
		if (gameState === 'spinning' && winnerRef) {
			const targetPosition = winnerRef.offsetLeft - 150 * (isMobile ? 1 : 3);
			position.set(targetPosition);
		}
	});

	$effect(() => {
		if (window) {
			checkMobile();
		}
	});
</script>

<svelte:window onresize={checkMobile} />
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
		{#each items as item, i (i)}
			<div
				class="mx-4 flex h-44 basis-36 flex-col items-center justify-center rounded-xl border-2 bg-gradient-to-b from-slate-900 to-black p-3 transition-all"
				class:winner={i === winnerIndex && gameState !== 'idle' && gameState !== 'spinning'}
				style="border-color: {rarityColors[item.rarity]};"
			>
				{#if i === winnerIndex}
					<div bind:this={winnerRef}></div>
				{/if}
				<div
					class="mb-3 h-28 w-28 rounded-full border-2 bg-black p-1 transition-all"
					style="border-color: {rarityColors[item.rarity]}; box-shadow: {rarityGlow[
						item.rarity
					]} {rarityColors[item.rarity]};"
				>
					<img
						class="h-full w-full rounded-full object-cover"
						src={item.creator.avatar_url}
						alt={item.creator.name}
					/>
				</div>
				<div
					class="bg-gradient-to-r from-amber-200 via-yellow-300 to-amber-200 bg-clip-text text-xl font-bold text-transparent uppercase"
				>
					${item.creator.ticker}
				</div>
			</div>
		{/each}
	</div>
</div>

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
