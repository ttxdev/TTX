<script lang="ts">
	import { fade, slide } from 'svelte/transition';
	import { cubicOut } from 'svelte/easing';
	import type { Snippet } from 'svelte';

	let { children, closeDrawer }: { children: Snippet; closeDrawer: () => void } = $props();
</script>

<button
	class="fixed inset-0 z-40 bg-black/50 backdrop-blur-sm md:hidden"
	transition:fade={{ duration: 200 }}
	onclick={closeDrawer}
	aria-label="Close drawer"
></button>
<div
	class="fixed right-0 bottom-0 z-50 h-[85vh] w-full max-w-md overflow-hidden rounded-tl-2xl bg-white/80 shadow-2xl backdrop-blur backdrop-contrast-100 backdrop-saturate-100 backdrop-filter md:h-[50vh] dark:bg-black/80"
	transition:slide|local={{ duration: 300, easing: cubicOut, axis: 'y' }}
>
	<div class="flex h-full flex-col">
		<div
			class="flex items-center justify-between border-b border-gray-200 p-4 dark:border-gray-700"
		>
			<h2 class="text-xl font-semibold">Your Holdings</h2>
			<button
				class="rounded-full p-2 text-gray-500 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-700"
				onclick={closeDrawer}
				aria-label="Close drawer"
			>
				<svg
					xmlns="http://www.w3.org/2000/svg"
					class="h-5 w-5"
					fill="none"
					viewBox="0 0 24 24"
					stroke="currentColor"
				>
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M19 9l-7 7-7-7"
					/>
				</svg>
			</button>
		</div>
		<div class="flex-1 overflow-y-auto p-4">
			{@render children()}
		</div>
	</div>
</div>
