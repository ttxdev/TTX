<script lang="ts">
	import { CreatorApplicationStatus } from '$lib/api';
	import type { PageProps } from './$types';

	const { data }: PageProps = $props();
	const { app, user } = $derived(data);
</script>

<svelte:head>
	<title>TTX - Creator Application</title>
	<meta name="description" content="View your creator application status on TTX" />
</svelte:head>

<div class="mx-auto flex max-w-[1000px] flex-col space-y-12">
	<section class="flex flex-col items-center gap-4">
		<h1 class="font-display text-center text-5xl max-md:text-3xl">Your Creator Application</h1>
	</section>

	<div class="mx-auto w-full max-w-[600px] space-y-8">
		<div
			class="rounded-lg border-2 border-gray-200 bg-white p-6 shadow-sm dark:border-gray-700 dark:bg-gray-900"
		>
			<div class="mb-4 flex items-center justify-between">
				<h2 class="font-display text-xl font-semibold">Application Status</h2>
				{#if app.status === CreatorApplicationStatus.Pending}
					<span
						class="rounded-full bg-yellow-100 px-3 py-1 text-sm font-medium text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200"
					>
						Pending Review
					</span>
				{:else if app.status === CreatorApplicationStatus.Approved}
					<span
						class="rounded-full bg-green-100 px-3 py-1 text-sm font-medium text-green-800 dark:bg-green-900 dark:text-green-200"
					>
						Approved
					</span>
				{:else if app.status === CreatorApplicationStatus.Rejected}
					<span
						class="rounded-full bg-red-100 px-3 py-1 text-sm font-medium text-red-800 dark:bg-red-900 dark:text-red-200"
					>
						Rejected
					</span>
				{/if}
			</div>
			<div class="space-y-4">
				<div
					class="flex items-center justify-between border-b border-gray-200 pb-2 dark:border-gray-700"
				>
					<span class="text-gray-600 dark:text-gray-400">Ticker Symbol</span>
					<span class="font-medium">{app.ticker}</span>
				</div>
				<div
					class="flex items-center justify-between border-b border-gray-200 pb-2 dark:border-gray-700"
				>
					<span class="text-gray-600 dark:text-gray-400">Submitted By</span>
					<span class="font-medium">{app.submitter.name}</span>
				</div>
				<div
					class="flex items-center justify-between border-b border-gray-200 pb-2 dark:border-gray-700"
				>
					<span class="text-gray-600 dark:text-gray-400">Submitted On</span>
					<span class="font-medium">{new Date(app.created_at).toLocaleString()}</span>
				</div>
				<div
					class="flex items-center justify-between border-b border-gray-200 pb-2 dark:border-gray-700"
				>
					<span class="text-gray-600 dark:text-gray-400">Status Updated On</span>
					<span class="font-medium">{new Date(app.updated_at).toLocaleString()}</span>
				</div>
			</div>
		</div>

		<div
			class="rounded-lg border-2 border-gray-200 bg-white p-6 shadow-sm dark:border-gray-700 dark:bg-gray-900"
		>
			<h2 class="font-display mb-4 text-xl font-semibold">Your Creator Profile</h2>
			<div class="space-y-4">
				<div
					class="flex items-center justify-between border-b border-gray-200 pb-2 dark:border-gray-700"
				>
					<span class="text-gray-600 dark:text-gray-400">Profile URL</span>
					<span class="font-medium">ttx.gg/{user.slug}</span>
				</div>
				<div
					class="flex items-center justify-between border-b border-gray-200 pb-2 dark:border-gray-700"
				>
					<span class="text-gray-600 dark:text-gray-400">Display Name</span>
					<span class="font-medium">{user.name}</span>
				</div>
			</div>
		</div>
	</div>
</div>
