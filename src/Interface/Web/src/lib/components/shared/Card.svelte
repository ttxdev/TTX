<script lang="ts">
	type Props = { title: string; children: any };
	let { title, children }: Props = $props();
</script>

<div class="bg-base-200/50 rounded-lg p-4 shadow-md">
	<span class="bg-base-300 top-4 left-4 rounded-lg px-3 py-1 text-sm font-medium">
		{title}
	</span>
	<div class="h-[28rem] overflow-x-auto">
		{@render children()}
	</div>
</div>
