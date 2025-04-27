<script>
	import { formatTicker } from '$lib/util';

	let { ticker, slug, avatar_url } = $props();
</script>

<a href="/creators/{slug}">
	<div class=" flex items-center justify-center gap-2">
		<img src={avatar_url} alt="Avatar" class="h-8 w-8 rounded-full" />
		<div class="flex flex-col text-violet-500 hover:underline">
			<p class="text-lg font-bold">{formatTicker(ticker)}</p>
		</div>
	</div>
</a>
