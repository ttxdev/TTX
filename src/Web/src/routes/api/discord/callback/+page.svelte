<script lang="ts">
	import { goto, invalidateAll } from "$app/navigation";
	import { discordSdk } from "$lib/discord";
	import { onMount, setContext } from "svelte";
	import type { LayoutProps } from "../../../$types";
	import { writable } from "svelte/store";
	import { token, user } from "$lib/stores/data";

    let { data }: LayoutProps = $props();
    
	user.set(data.user);
	token.set(data.token);

    onMount(async () => { 
        if (discordSdk) {
            await discordSdk.commands.setActivity({
                activity: { 
                    type: 0,
                    state: 'TTX',
                    assets: {
                        large_image: 'ttx',
                        large_text: 'TTX',
                    }
                }
            });
        }

        goto('/', {
            replaceState: true,
            invalidateAll: true,
        });
    });
</script>

<div class="modal visible w-full max-h-full bg-black text-white overflow-y-auto pointer-events-auto block">
    <span class="w-full h-full flex items-center justify-center">
        Connecting to Twitch...
    </span>
</div>