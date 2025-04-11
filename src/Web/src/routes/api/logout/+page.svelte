<script lang="ts">
	import { goto } from "$app/navigation";
	import { discordSdk } from "$lib/discord";
	import { onMount } from "svelte";
    onMount(() => { 
        if (!discordSdk) {
            window.location.href = '/';
            return;
        }

        goto('/api/logout/discord');
    });
</script>