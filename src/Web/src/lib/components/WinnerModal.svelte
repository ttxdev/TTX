<script lang="ts">
	import { fade, scale } from 'svelte/transition';
	import { elasticOut } from 'svelte/easing';
	// @ts-expect-error Confetti is not a module
	import Confetti from 'confetti-js';
	import { onMount } from 'svelte';
	import type { CreatorBox } from '../../routes/gamba/+page.server';
	import { formatTicker, formatValue } from '$lib/util';

	type WinnerModalProps = {
		winner: CreatorBox;
		rarityColors: Record<string, string>;
		rarityGlow: Record<string, string>;
	};

	const { winner, rarityColors, rarityGlow }: WinnerModalProps = $props();

	onMount(() => {
		const confetti = new Confetti({
			target: 'winner-modal-canvas'
		});
		confetti.render();

		return () => confetti.clear();
	});
</script>

<div
	class="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm"
	transition:fade={{ duration: 300 }}
	id="winner-modal"
>
	<canvas id="winner-modal-canvas" class="absolute inset-0 h-full w-full"></canvas>
	<div
		class="relative mx-4 flex w-full max-w-md flex-col items-center"
		transition:scale={{ duration: 400, delay: 200, opacity: 0, start: 0.8, easing: elasticOut }}
		aria-modal="true"
	>
		<h2
			class="mb-6 bg-gradient-to-r from-amber-300 via-yellow-500 to-amber-300 bg-clip-text text-4xl font-bold text-transparent"
		>
			YOU WON!
		</h2>

		<div
			class="flex w-full flex-col items-center rounded-2xl border-2 bg-gradient-to-b from-slate-900 to-black p-8 shadow-[0_0_30px_rgba(124,58,237,0.5)]"
			style="border-color: {rarityColors[winner.rarity_class]};"
		>
			<div
				class="mb-6 bg-gradient-to-r from-amber-300 via-yellow-500 to-amber-300 bg-clip-text text-3xl font-bold text-transparent"
			>
				{formatValue(winner.value)}
			</div>
			<div
				class="animate-pulse-slow mb-6 h-36 w-36 rounded-full border-3 bg-black p-1 transition-all"
				style="
                    border-color: {rarityColors[winner.rarity_class]};
                    box-shadow: {rarityGlow[winner.rarity_class]} {rarityColors[
					winner.rarity_class
				]};
                "
			>
				<img
					class="h-full w-full rounded-full object-cover"
					src={winner.avatar_url}
					alt={winner.name}
				/>
			</div>
			<div
				class="mb-2 w-full overflow-hidden text-center text-xl font-bold text-ellipsis whitespace-nowrap text-white"
			>
				{winner.name}
			</div>
			<div
				class="bg-gradient-to-r from-amber-200 via-yellow-300 to-amber-200 bg-clip-text text-xl font-bold text-transparent uppercase"
			>
				{formatTicker(winner.ticker)}
			</div>
		</div>

		<a
			class="mt-8 cursor-pointer rounded-lg bg-gradient-to-r from-violet-600 to-purple-700 px-8 py-5 text-xl font-bold text-white transition-all hover:scale-105 hover:shadow-[0_0_20px_rgba(124,58,237,0.7)]"
			href="/creators/{winner.slug}"
		>
			Visit Creator
		</a>
	</div>
</div>

<style>
	.animate-pulse-slow {
		animation: pulseSlow 3s infinite;
	}

	@keyframes pulseSlow {
		0% {
			transform: scale(1);
		}
		50% {
			transform: scale(1.05);
		}
		100% {
			transform: scale(1);
		}
	}
</style>
