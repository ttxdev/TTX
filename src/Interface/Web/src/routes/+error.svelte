<script lang="ts">
	import { page } from '$app/state';
</script>

<svelte:head>
	<title>TTX - {page.status} Error</title>
</svelte:head>

<div
	class="container mx-auto flex h-full min-h-[60vh] flex-col items-center justify-center px-4 py-12 text-center"
>
	<h1 class="mb-2 text-6xl font-bold">{page.status}</h1>
	<h2 class="mb-4 text-3xl font-semibold">
		{#if page.error?.message}
			{page.error.message}
		{:else if page.status === 404}
			Creator Not Found
		{:else}
			Something went wrong
		{/if}
	</h2>

	<p class="mb-8 text-xl opacity-80">
		{#if page.status === 404}
			We couldn't find the page you're looking for.<br />
			Please check the URL and try again.
		{:else}
			An unexpected error occurred while trying to access this page.<br />
			Please try again later.
		{/if}
	</p>

	<div class="mt-6">
		<a
			href="/"
			class="rounded-lg bg-purple-600 px-8 py-3 font-medium text-white shadow-lg transition-colors hover:bg-purple-700 focus:ring-2 focus:ring-purple-500 focus:ring-offset-2 focus:outline-none"
		>
			Back to Home
		</a>
	</div>
</div>
