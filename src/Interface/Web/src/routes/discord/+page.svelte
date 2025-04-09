<style>
    :global(* :not(#discord-auth)) {
        display:none;
    }
</style>

<script lang="ts">
	import { discordSdk } from "$lib/discord";
	import { onMount } from "svelte";
    import { PUBLIC_DISCORD_CLIENT_ID as discordClientId } from '$env/static/public';
	import { handleDiscordCallback, parseJwt } from "$lib/auth";
	import { patchUrlMappings, RPCCloseCodes } from "@discord/embedded-app-sdk";
    import { PUBLIC_API_BASE_URL as apiBaseUrl } from "$env/static/public";

    onMount(async () => {
        if (!discordSdk) {
            return;
        }

        await discordSdk.ready();

        const url = new URL(apiBaseUrl)

        patchUrlMappings([{prefix: '/external-api', target: url.hostname}]);

        const { code } = await discordSdk.commands.authorize({
            client_id: discordClientId,
            response_type: "code",
            state: "",
            prompt: "none",
            scope: ["identify", "rpc.activities.write", "connections"],
        });

        const {access_token, users} = await handleDiscordCallback(code)
        
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

        // TODO: Create a UI to select which user to use
        const user = users[0];
        
        window.location.href = 
            '/api/discord/callback?' + 
            new URLSearchParams({
                access_token,
                user: user.id
            });
    });
</script>