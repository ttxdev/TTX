<script lang="ts">
	import { enhance } from '$app/forms';
	import type { PageProps } from './$types';
	import toast from 'svelte-french-toast';

	const { data }: PageProps = $props();
	const user = $derived(data.user);

	let submitting = $state(false);
	let ticker = $state('');
	let error = $state<string | null>(null);

	function validateTicker(ticker: string): boolean {
		if (!ticker) {
			error = 'Ticker symbol is required';
			return false;
		}
		if (!/^[A-Z$]{1,5}$/.test(ticker)) {
			error = 'Ticker must be 1-5 uppercase letters';
			return false;
		}
		error = null;
		return true;
	}

	function handleTickerInput(e: Event) {
		const input = e.target as HTMLInputElement;
		ticker = input.value.toUpperCase();
		validateTicker(ticker);
	}

	function handleSubmit() {
		if (!user) return;
		if (!validateTicker(ticker)) return;
		submitting = true;
		error = null;
	}
</script>

<svelte:head>
	<title>TTX - Apply to be a Creator</title>
	<meta
		name="description"
		content="Apply to become a creator on TTX and let your community invest in your success"
	/>
</svelte:head>

<div class="mx-auto flex max-w-[1000px] flex-col space-y-12">
	<section class="flex flex-col items-center gap-4">
		<h1 class="font-display text-center text-5xl max-md:text-3xl">Become a Creator</h1>
		<p class="text-center text-gray-600 dark:text-gray-400">
			Join our community of creators and let your audience invest in your success
		</p>
	</section>

	{#if !user}
		<p class="text-center text-gray-600 dark:text-gray-400">
			Please login to apply to become a creator
		</p>
	{:else}
		<form
			method="POST"
			use:enhance={() => {
				handleSubmit();
				return async ({ result }) => {
					submitting = false;
					if (result.type === 'error') {
						error = result.error?.message;
						toast.error(error || 'Failed to submit application');
					} else if (result.type === 'redirect') {
						toast.success('Application submitted successfully');
					}
				};
			}}
			class="mx-auto flex w-full max-w-[600px] flex-col space-y-8"
		>
			<div class="flex flex-col space-y-2">
				<label for="name" class="font-display text-lg font-medium text-gray-700 dark:text-gray-300"
					>Your Name</label
				>
				<div class="relative">
					<input
						type="text"
						id="name"
						name="name"
						value={user.name}
						readonly
						disabled
						class="w-full cursor-not-allowed rounded-lg border-2 border-gray-300 bg-gray-200/50 py-3 pr-4 pl-12 text-gray-500 dark:border-gray-600 dark:bg-gray-700/50 dark:text-gray-400"
					/>
					<div class="absolute top-1/2 left-4 -translate-y-1/2 text-gray-400">
						<svg
							xmlns="http://www.w3.org/2000/svg"
							class="h-5 w-5"
							viewBox="0 0 20 20"
							fill="currentColor"
						>
							<path
								fill-rule="evenodd"
								d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z"
								clip-rule="evenodd"
							/>
						</svg>
					</div>
				</div>
			</div>

			<div class="flex flex-col space-y-2">
				<label for="slug" class="font-display text-lg font-medium text-gray-700 dark:text-gray-300"
					>Your URL Slug</label
				>
				<div class="relative">
					<input
						type="text"
						id="slug"
						name="slug"
						value={`ttx.gg/${user.slug}`}
						readonly
						disabled
						class="w-full cursor-not-allowed rounded-lg border-2 border-gray-300 bg-gray-200/50 py-3 pr-4 pl-12 text-gray-500 dark:border-gray-600 dark:bg-gray-700/50 dark:text-gray-400"
					/>
					<div class="absolute top-1/2 left-4 -translate-y-1/2 text-gray-400">
						<svg
							xmlns="http://www.w3.org/2000/svg"
							class="h-5 w-5"
							viewBox="0 0 20 20"
							fill="currentColor"
						>
							<path
								fill-rule="evenodd"
								d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z"
								clip-rule="evenodd"
							/>
						</svg>
					</div>
				</div>
			</div>

			<div class="flex flex-col space-y-2">
				<label
					for="ticker"
					class="font-display text-lg font-medium text-gray-700 dark:text-gray-300"
					>Desired Ticker Symbol</label
				>
				<input
					type="text"
					id="ticker"
					name="ticker"
					bind:value={ticker}
					oninput={handleTickerInput}
					required
					maxlength="5"
					class="rounded-lg border border-gray-300 px-4 py-3 dark:border-gray-600 dark:bg-gray-800 {error
						? 'border-red-500'
						: ''}"
					placeholder="e.g. BALD, CASE, PANTS"
				/>
				{#if error}
					<p class="text-sm text-red-500">{error}</p>
				{:else}
					<p class="text-sm text-gray-500 dark:text-gray-400">
						Enter a 1-5 letter ticker symbol in uppercase letters
					</p>
				{/if}
			</div>

			<button
				type="submit"
				disabled={submitting || !!error}
				class="btn mt-4 rounded-full border-purple-400 bg-purple-400 px-8 py-3 text-white transition-all duration-200 hover:scale-105 hover:bg-purple-500 disabled:bg-purple-300 disabled:hover:scale-100"
			>
				{#if submitting}
					<div class="flex items-center gap-2">
						<svg
							class="h-5 w-5 animate-spin"
							xmlns="http://www.w3.org/2000/svg"
							fill="none"
							viewBox="0 0 24 24"
						>
							<circle
								class="opacity-25"
								cx="12"
								cy="12"
								r="10"
								stroke="currentColor"
								stroke-width="4"
							/>
							<path
								class="opacity-75"
								fill="currentColor"
								d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
							/>
						</svg>
						Submitting...
					</div>
				{:else}
					Apply Now
				{/if}
			</button>
		</form>
	{/if}
</div>
