<script lang="ts">
	import LatestTransactions from '$lib/components/player/LatestPlayerTransactions.svelte';
	import PlayerCard from '$lib/components/player/PlayerCard.svelte';
	import BiggestShares from '$lib/components/player/MostCreatorShares.svelte';

	let { data } = $props();
	const place = Math.floor(Math.random() * 4) + 1;

	let player = $derived(data.player);
</script>

<svelte:head>
	<title>TTX - {data.player.name}</title>
	<meta name="description" content={`Player profile for ${data.player.name}`} />
	<meta property="og:title" content={data.player.name} />
	<meta property="og:description" content={`Player profile for ${data.player.name}`} />
	<meta property="og:image" content={data.player.avatar_url} />
</svelte:head>

<div class="mx-auto w-full max-w-[1000px]">
	<div class="mx-2 my-5">
		<PlayerCard {player} {place} />
	</div>

	<div class="m-2 flex flex-col gap-4 md:flex-row">
		<div class="md:w-1/2">
			<BiggestShares shares={data.shares} />
		</div>
		<div class="md:w-1/2">
			<LatestTransactions transactions={data.transactions} />
		</div>
	</div>
</div>
