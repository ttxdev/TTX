<script lang="ts">
	import type { ICreatorDto } from '$lib/api';
	import { goto } from '$app/navigation';
	import toast from 'svelte-french-toast';
	type Props = {
		creator: ICreatorDto;
		onClose: () => void;
	};

	let { creator, onClose }: Props = $props();
	let isLoading = $state(false);

	async function handleClick() {
		if (isLoading) return;

		isLoading = true;
		try {
			const response = await fetch(`/creators/${creator.slug}/opt-out`, {
				method: 'POST'
			});

			if (!response.ok) {
				const errorData = await response.json().catch(() => null);
				throw new Error(
					errorData?.message || `Server error: ${response.status} ${response.statusText}`
				);
			}

			const res = await response.json();
			if (!res.success) {
				throw new Error(res.message || 'Failed to opt out');
			}

			toast.success('You have been opted out of TTX.');
			goto('/', { invalidateAll: true });
		} catch (err) {
			if (err instanceof Error) {
				toast.error(err.message);
			} else if (err instanceof TypeError && err.message === 'Failed to fetch') {
				toast.error('Network error. Please check your internet connection and try again.');
			} else {
				toast.error('An unexpected error occurred. Please try again later.');
			}
		} finally {
			isLoading = false;
			onClose();
		}
	}
</script>

<div class="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
	<div class="bg-base-200 rounded-lg p-6 shadow-lg">
		<h2 class="mb-4 text-xl font-bold">Opt Out of TTX</h2>
		<p class="mb-4">Are you sure you want to opt out of TTX? This will:</p>
		<ul class="mb-4 list-disc pl-5">
			<li>Remove your creator profile from TTX</li>
			<li>Stop tracking your channel's value</li>
			<li>Prevent users from trading your shares</li>
		</ul>
		<p class="mb-4 text-sm text-gray-500">
			Note: This action cannot be undone. If you change your mind later, you'll need to contact us
			to re-enable your profile.
		</p>
		<div class="flex justify-end gap-2">
			<button onclick={onClose} class="btn btn-ghost" disabled={isLoading}> Cancel </button>
			<button onclick={handleClick} class="btn btn-error" disabled={isLoading}>
				{#if isLoading}
					<span class="loading loading-spinner loading-sm"></span>
					Processing...
				{:else}
					Confirm Opt-Out
				{/if}
			</button>
		</div>
	</div>
</div>
