<script lang="ts">
	import { discordSdk } from "$lib/discord";
	import { onMount } from "svelte";
    import { PUBLIC_DISCORD_CLIENT_ID as discordClientId } from '$env/static/public';
	import { RPCCloseCodes } from "@discord/embedded-app-sdk";
	import { goto } from "$app/navigation";
	import type { TwitchUserDto } from "$lib/api";
	import { getApiClient } from "$lib";

    let link_token = '';
    let users: TwitchUserDto[] = [];

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

        const client = getApiClient('');
        const resp = await client.discordCallback(code);

        link_token = resp.link_token;
        users = resp.twitch_users;

        const auth = await discordSdk.commands.authenticate({
            access_token: resp.access_token,
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
    {#if users && users.length > 0 && link_token}
        <h1>Select your Twitch account:</h1>
        {#each users as user (user.id)}
            <button onclick={() => {
                goto(
                    '/api/discord/link?' +
                    new URLSearchParams({
                        link_token: link_token,
                        twitch_id: user.id
                    })
                );
            }} class="flex items-center justify-center gap-2 p-4 m-2 w-xl bg-gray-900 rounded-lg hover:bg-gray-800 transition cursor-pointer">
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
