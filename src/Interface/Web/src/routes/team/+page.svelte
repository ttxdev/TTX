<script>
	import { PUBLIC_DISCORD_URL } from '$env/static/public';
	import ExternalLink from '$lib/components/ExternalLink.svelte';
	/** @type {{ data: import('./$types').PageData }} */
	import TeamMemberCard from '$lib/components/TeamMemberCard.svelte';

	let { data } = $props();
	let { teamMembers, thankYou } = data;
</script>

<svelte:head>
	<title>TTX - Meet the Team</title>
	<meta
		name="description"
		content="Meet the team behind TTX. We are a group of passionate individuals dedicated to building a better future for all."
	/>
</svelte:head>
<div class="mx-auto flex max-w-[1000px] flex-col items-center justify-center gap-8 p-4 md:gap-16">
	<section class="w-full">
		<h1 class="font-display my-4 text-center text-3xl md:text-left md:text-4xl">Meet the Team</h1>
		<div class="grid w-full grid-cols-1 justify-items-center gap-4 sm:grid-cols-2 lg:grid-cols-4">
			{#each teamMembers as member (member.name)}
				<TeamMemberCard
					name={member.name}
					image={member.image}
					role={member.role}
					bio={member.bio}
					socials={member.socials}
				/>
			{/each}
		</div>
	</section>
	<section class="w-full">
		<h1 class="font-display my-4 text-center text-3xl md:text-4xl">Thank You</h1>
		<div
			class="mx-auto flex w-full max-w-[600px] flex-row flex-wrap items-center gap-3 text-center"
		>
			{#each thankYou as entry (entry[0])}
				<a target="_blank" href={entry[1]} class="hover:text-violet-400">
					{entry[0]}
				</a>
			{/each}
			<span>and the original ATX Team.</span>
		</div>
		<div class="my-6 flex w-full flex-row justify-center md:my-10">
			<ExternalLink
				href={PUBLIC_DISCORD_URL}
				target="_blank"
				className="rounded bg-violet-500 px-6 py-3 text-center font-bold text-white transition hover:bg-violet-600"
			>
				Join our Discord!
			</ExternalLink>
		</div>
	</section>
</div>
