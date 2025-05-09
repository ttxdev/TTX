<script lang="ts">
	import Leaderboard from '$lib/components/Leaderboard.svelte';
	import Podium from '$lib/components/Podium.svelte';
	import CreatorChart from '$lib/components/CreatorChart.svelte';
	import type { PageProps } from './$types';
	import { PUBLIC_DISCORD_URL } from '$env/static/public';
	import ExternalLink from '$lib/components/ExternalLink.svelte';
	let { data }: PageProps = $props();
</script>

<svelte:head>
	<title>TTX</title>
	<meta name="description" content="TTX Homepage" />
</svelte:head>

<div class="mx-auto flex max-w-[1000px] flex-col space-y-12">
	<section class="flex columns-2 max-md:flex-col max-md:gap-8">
		<div class="flex w-1/2 flex-row items-center justify-center max-md:w-full">
			<div class="flex flex-col items-center">
				<img class="w-96 dark:hidden" src="/ttx-full-logo-dark.png" alt="TTX Logo" />
				<img class="hidden w-96 dark:block" src="/ttx-full-logo-light.png" alt="TTX Logo" />
				<p class="font-display text-2xl max-md:text-sm">Invest in your favorite Creators</p>
			</div>
		</div>
		<div class="flex w-1/2 flex-row items-center justify-center px-10 max-md:w-full md:px-0">
			<div class="flex h-full w-full p-4 max-md:p-0">
				<CreatorChart creator={data.featuredCreator} />
			</div>
		</div>
	</section>
	<section class="flex columns-2 flex-row-reverse max-md:flex-col max-md:gap-8">
		<div
			class="flex w-1/2 flex-col gap-4 p-8 max-md:w-full max-md:flex-col max-md:p-0 max-md:text-center"
		>
			<h1 class="font-display mb-2 text-4xl max-md:text-3xl">How it Works</h1>
			<p class="w-full px-7 text-justify md:px-0">
				TTX is a fantasy stock market for Twitch streamers. You can buy and sell shares of your
				favorite creators. <strong>Not real money, not crypto-related, and never will be.</strong>
				<br /><br />
				Perform up to the minute statistical pricing analysis to time your purchases and sales to maximize
				your investment potential (all tax free, as we are free from pesky regulations).
				<br /><br />
				Oh.. how does the pricing work? You don't know how the NYSE works, ya know... so just pump and
				dump! I mean... buy low, sell high... I mean dollar cost average! Be responsible, have fun,
				<span class="italic">don't call the SEC</span>. TTX, be truly invested in your streamer.
			</p>
			<div class="flex flex-row justify-center">
				<ExternalLink
					href={PUBLIC_DISCORD_URL}
					target="_blank"
					className="w-fit text-purple-500 hover:underline"
				>
					Join our Discord!
				</ExternalLink>
			</div>
		</div>
		<div class="flex w-1/2 flex-col items-center justify-center max-md:w-full">
			<Leaderboard creators={data.featuredCreators} />
			<a class="mt-2 hover:underline hover:decoration-purple-500" href="/creators">
				<span class="font-black text-purple-500">View all creators →</span>
			</a>
		</div>
	</section>
	<section class="flex flex-col items-center justify-center gap-4 py-8">
		<p class="font-display text-center text-2xl max-md:text-xl">Want to become a Creator?</p>
		<a
			href="/apply"
			class="btn rounded-full border-purple-400 bg-purple-400 px-8 text-white transition-all duration-200 hover:scale-105 hover:bg-purple-500"
		>
			Apply Now
		</a>
	</section>
	<section class="flex max-md:flex-col">
		<div class="flex w-full flex-col items-center justify-center gap-12">
			<p class="font-display text-center text-4xl font-bold max-md:text-3xl">Leaderboard</p>
			<div class="flex w-full flex-row gap-4 px-7 max-md:flex-col md:px-0">
				<div class="w-1/2 max-md:w-full">
					<Podium type="creators" data={data.topCreators} />
				</div>
				<div class="w-1/2 max-md:w-full">
					<Podium type="users" data={data.topPlayers} />
				</div>
			</div>
		</div>
	</section>
</div>
