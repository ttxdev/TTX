<script>
	import { onMount, onDestroy } from 'svelte';

	let { date } = $props();

	let displayTime = $state('');

	let dateObj = $derived(new Date(date));

	function updateDisplay() {
		const now = new Date();
		// @ts-expect-error Because you're telling me that I need a reason...
		const diffSeconds = Math.floor((now - dateObj) / 1000);

		if (diffSeconds < 60) {
			displayTime = `${diffSeconds} second${diffSeconds !== 1 ? 's' : ''} ago`;
		} else if (diffSeconds < 3600) {
			const minutes = Math.floor(diffSeconds / 60);
			displayTime = `${minutes} minute${minutes !== 1 ? 's' : ''} ago`;
		} else if (diffSeconds < 86400) {
			const hours = Math.floor(diffSeconds / 3600);
			displayTime = `${hours} hour${hours !== 1 ? 's' : ''} ago`;
		} else {
			const days = Math.floor(diffSeconds / 86400);
			displayTime = `${days} day${days !== 1 ? 's' : ''} ago`;
		}
	}

	let interval = $state();
	onMount(() => {
		updateDisplay();
		interval = setInterval(updateDisplay, 1000);
	});

	onDestroy(() => {
		clearInterval(interval);
	});
</script>

<span>{displayTime}</span>
