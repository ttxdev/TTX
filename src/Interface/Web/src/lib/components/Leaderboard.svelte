<script lang="ts">
	import Chart from 'chart.js/auto';
	import type { UserStats } from '../../routes/proxy+page.server';

	const props: { creators: UserStats[] } = $props();
	const creators = $derived(props.creators);

	function createMiniChart(element: HTMLCanvasElement, history: { time: number; value: number }[]) {
		const values = history.map((d) => d.value);
		const isUpward = values[values.length - 1] > values[0];
		const lineColor = isUpward ? '#22c55e' : '#ef4444'; // green-500 or red-500

		new Chart(element, {
			type: 'line',
			data: {
				labels: Array(values.length).fill(''), // Create empty labels for history points
				datasets: [
					{
						data: values,
						borderColor: lineColor,
						borderWidth: 2,
						fill: false,
						tension: 0,
						pointRadius: 0
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
					x: {
						display: false
					},
					y: {
						display: false
					}
				}
			}
		});
	}
</script>

<ul class="list rounded-box rounded-xl bg-gray-100/10 shadow-md">
	{#each creators as creator (creator.slug)}
		<li class="list-row flex items-center max-md:flex-col max-md:p-4">
			<div class="flex w-1/4 flex-col items-center justify-center gap-2 max-md:w-full">
				<a href={`/channels/${creator.slug}`}>
					<img src={creator.avatar_url} alt="" class="size-16 rounded-full" />
				</a>
				<a href={`/channels/${creator.slug}`} class="font-semibold hover:underline">
					{creator.name}
				</a>
				<div class="mb-1 flex items-center justify-between"></div>
			</div>
			<div class="flex-1">
				<div class="flex-1 max-md:mt-4 max-md:w-full">
					<div class="h-[40px] w-full">
						<canvas
							use:createMiniChart={creator.history.map((v) => ({
								time: v.time.getTime(),
								value: v.value
							}))}
						></canvas>
					</div>
				</div>
			</div>
		</li>
	{/each}
</ul>
