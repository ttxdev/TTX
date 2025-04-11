<script lang="ts">
	let { place }: { place: number } = $props();

	const getPlaceClass = (place: number) => {
		if (place === 1) return 'bg-[#FFD700]/30';
		if (place === 2) return 'bg-gray-600/60';
		if (place === 3) return 'bg-[#CD7F32]/30';
		return 'bg-base-300';
	};
</script>

<div class="{getPlaceClass(place)}  items-center justify-center rounded-lg p-1 px-2">
	<span># {place}</span>
</div>
