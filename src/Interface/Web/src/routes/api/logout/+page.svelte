<script lang="ts">
	import { goto } from "$app/navigation";
	import { discordSdk } from "$lib/discord";
	import { RPCCloseCodes } from "@discord/embedded-app-sdk";
	import { onMount } from "svelte";
    onMount(() => { 
        if (!discordSdk) {
            window.location.href = '/';
            return;
        }

        discordSdk.close(RPCCloseCodes.CLOSE_NORMAL, "Logged out.");
    });
</script>