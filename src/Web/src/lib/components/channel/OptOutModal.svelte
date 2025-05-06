<script lang="ts">
	import type { ICreatorDto } from '$lib/api';
	import { goto } from '$app/navigation';
	export let creator: ICreatorDto;
	export let onClose: () => void;
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
			<button onclick={onClose} class="btn btn-ghost">Cancel</button>
			<button
				onclick={async () => {
					try {
						const response = await fetch(`/creators/${creator.slug}/opt-out`, {
							method: 'POST'
						});

						if (!response.ok) {
							const error = await response.text();
							throw new Error(error);
						}

						goto('/', { invalidateAll: true });
					} catch (err) {
						console.error('Failed to opt out:', err);
						alert('Failed to opt out. Please try again later.');
					} finally {
						onClose();
					}
				}}
				class="btn btn-error"
			>
				Confirm Opt-Out
			</button>
		</div>
	</div>
</div>
