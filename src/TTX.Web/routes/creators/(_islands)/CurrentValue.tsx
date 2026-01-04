export default function CurrentValue({ value }: { value: number }) {
  return <span>TODO {value}</span>;
}

// <script lang="ts">
// 	import { formatValue } from '$lib/util';
// 	import { onDestroy } from 'svelte';
// 	import { fade, fly } from 'svelte/transition';
// 	import { Tween } from 'svelte/motion';

// 	let { value }: { value: number } = $props();

// 	let displayedDiff = $state(0);
// 	let showDiff = $state(false);
// 	let diffTimeout: number | null = null;
// 	let valueDiff = $state(0);
// 	let valueDirection = $state<'up' | 'down' | 'none'>('none');
// 	let previousValue = $state(value);

// 	// Create a Tween instance to animate the display value.
// 	// Using a custom easing function similar to the one from your custom animation.
// 	let animatedDisplayValue = new Tween(value, {
// 		duration: 1000,
// 		easing: (t) => (t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2)
// 	});

// 	$effect(() => {
// 		// Calculate the difference before updating previousValue
// 		valueDiff = value - previousValue;
// 		previousValue = value;

// 		if (valueDiff !== 0) {
// 			// Clear any existing timeout
// 			if (diffTimeout !== null) {
// 				clearTimeout(diffTimeout);
// 			}

// 			displayedDiff = valueDiff;
// 			showDiff = true;
// 			diffTimeout = setTimeout(() => {
// 				showDiff = false;
// 				diffTimeout = null;
// 			}, 3000);

// 			// Set the pulse direction for styling
// 			if (valueDiff > 0) {
// 				valueDirection = 'up';
// 			} else if (valueDiff < 0) {
// 				valueDirection = 'down';
// 			}

// 			// Animate the value change with our Tween instance.
// 			animatedDisplayValue.set(value);

// 			// Reset pulse direction after animation completes.
// 			setTimeout(() => {
// 				valueDirection = 'none';
// 			}, 1500);
// 		}
// 	});

// 	onDestroy(() => {
// 		if (diffTimeout !== null) {
// 			clearTimeout(diffTimeout);
// 		}
// 	});
// </script>

// <div>
// 	<p
// 		class="text-lg font-bold"
// 		class:pulse-up={valueDirection === 'up'}
// 		class:pulse-down={valueDirection === 'down'}
// 	>
// 		{formatValue(animatedDisplayValue.current)}
// 	</p>
// 	{#if showDiff}
// 		<div
// 			class="absolute top-0 -right-8"
// 			in:fly={{ y: -20, duration: 300 }}
// 			out:fade={{ duration: 1000 }}
// 		>
// 			<span class={displayedDiff >= 0 ? 'text-green-500' : 'text-red-500'}>
// 				{displayedDiff >= 0 ? '+' : '-'}{formatValue(Math.abs(displayedDiff))}
// 			</span>
// 		</div>
// 	{/if}
// </div>

// <style>
// 	@keyframes pulse-up {
// 		0% {
// 			transform: scale(1);
// 			color: inherit;
// 		}
// 		20% {
// 			transform: scale(1.05);
// 			color: #22c55e;
// 		}
// 		50% {
// 			transform: scale(1.1);
// 			color: #22c55e;
// 		}
// 		80% {
// 			transform: scale(1.05);
// 			color: #22c55e;
// 		}
// 		100% {
// 			transform: scale(1);
// 			color: inherit;
// 		}
// 	}

// 	@keyframes pulse-down {
// 		0% {
// 			transform: scale(1);
// 			color: inherit;
// 		}
// 		20% {
// 			transform: scale(1.05);
// 			color: #ef4444;
// 		}
// 		50% {
// 			transform: scale(1.1);
// 			color: #ef4444;
// 		}
// 		80% {
// 			transform: scale(1.05);
// 			color: #ef4444;
// 		}
// 		100% {
// 			transform: scale(1);
// 			color: inherit;
// 		}
// 	}

// 	.pulse-up {
// 		animation: pulse-up 0.5s ease-in-out forwards;
// 	}

// 	.pulse-down {
// 		animation: pulse-down 0.5s ease-in-out forwards;
// 	}
// </style>
