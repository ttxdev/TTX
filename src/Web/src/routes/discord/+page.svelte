<script lang="ts">
	import { discordSdk } from "$lib/discord";
	import { onMount } from "svelte";
    import { PUBLIC_DISCORD_CLIENT_ID as discordClientId } from '$env/static/public';
	import { handleDiscordCallback } from "$lib/auth";
	import { RPCCloseCodes } from "@discord/embedded-app-sdk";
	import { goto } from "$app/navigation";
	import type { DiscordTwitchDto } from "$lib/api";
    let access_token = '';
    let users: DiscordTwitchDto[] = [];

    onMount(async () => {
        if (!discordSdk) {
            goto('/');
            return;
        }

        await discordSdk.ready();

        const { code } = await discordSdk.commands.authorize({
            client_id: discordClientId,
            response_type: "code",
            state: "",
            prompt: "none",
            scope: ["identify", "rpc.activities.write", "connections"],
        });

        const data = await handleDiscordCallback(code)

        access_token = data.access_token;
        users = data.users;
        
        const auth = await discordSdk.commands.authenticate({
            access_token,
        });

        if (auth == null) {
            discordSdk.close(
                RPCCloseCodes.CLOSE_ABNORMAL,
                "Failed to authenticate with Discord.",
            );
            return;
        }

        if (users.length === 0) {
            discordSdk.close(
                RPCCloseCodes.CLOSE_ABNORMAL,
                "Please connect your Twitch account to Discord.",
            );
            return;
        }
    });
</script>

<div class="modal visible w-full max-h-full bg-black text-white overflow-y-auto pointer-events-auto block">
    {#if users && users.length > 0 && access_token}
        <h1>Select your Twitch account:</h1>
        {#each users as user (user.id)}
            <button onclick={() => {
                goto(
                    '/api/discord/callback?' + 
                    new URLSearchParams({
                        access_token: access_token,
                        user: user.id
                    })
                );
            }} class="flex items-center justify-center gap-2 p-4 m-2 w-xl bg-gray-900 rounded-lg hover:bg-gray-800 transition">
                <img src={user.avatar_url} alt={user.display_name} class="rounded-full w-8 h-8" />
                <span>{user.display_name}</span>
            </button>
        {/each}
    {:else}
        <span class="w-full h-full flex items-center justify-center">
            Connecting to Discord...
        </span>
    {/if}
</div>