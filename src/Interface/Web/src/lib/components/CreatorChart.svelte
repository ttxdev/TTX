<script lang="ts">
	import { onMount } from 'svelte';
	import { Chart } from 'chart.js/auto';
	import type { UserStats } from '../../routes/+page.server';

	const props: { creator: UserStats } = $props();
	const creator = $derived(props.creator);

	let canvas: HTMLCanvasElement | null = null;
	let chart: Chart | null = null;

	let isPositiveTrend = $derived(
		creator.history[creator.history.length - 1]?.value > creator.history[0]?.value
	);

	onMount(() => {
		const ctx = canvas!.getContext('2d');

		chart = new Chart(ctx!, {
			type: 'line',
			data: {
				labels: Array(creator.history.length).fill(''), // Empty labels since we don't have time data
				datasets: [
					{
						label: 'Price',
						data: creator.history.map((d) => d.value), // Direct array of values
						borderColor: isPositiveTrend ? '#22c55e' : '#ef4444',
						tension: 0,
						fill: false,
						borderWidth: 2,
						pointRadius: 0 // Remove the dots
					}
				]
			},
			options: {
				responsive: true,
				maintainAspectRatio: false,
				plugins: {
					legend: {
						display: false
					}
				},
				scales: {
					y: {
						beginAtZero: false,
						grid: {
							display: false
						},
						border: {
							display: false
						},
						ticks: {
							display: false // Remove Y axis values
						}
					},
					x: {
						grid: {
							display: false
						},
						border: {
							display: false
						},
						ticks: {
							display: false // Remove X axis values
						}
					}
				}
			}
		});

		return () => {
			chart!.destroy();
		};
	});
</script>

<div class="relative flex h-full min-h-[400px] w-full items-center justify-center">
	<div class="absolute h-3/4 w-full rounded-lg border border-gray-200/30 bg-gray-500/20 p-4">
		<canvas bind:this={canvas}></canvas>
	</div>
	<a href="/channels/{creator.slug}" class="absolute top-4 -left-8 h-16 w-16">
		<img
			src={creator.avatar_url}
			alt={creator.name}
			class="h-full w-full rounded-full border-2 border-white shadow-lg"
		/>
	</a>
</div>
