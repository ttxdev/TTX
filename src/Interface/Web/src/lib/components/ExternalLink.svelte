<script>
	import { discordSdk } from "$lib/discord";

	let {
		href,
        className = undefined,
        target = '_blank',
        children,
        ariaLabel = undefined,
	} = $props();
</script>

{#if discordSdk}
    <button
        class={className ? className + ' cursor-pointer' : 'cursor-pointer'}
        aria-label={ariaLabel}
        type="button"
        onclick={async () => {
            if (discordSdk) {
                try {
                    new URL(href);

                    await discordSdk.commands.openExternalLink({
                        url: href,
                    });
                } catch {
                    try {
                        new URL(href, window.location.origin);

                        await discordSdk.commands.openExternalLink({
                            url: window.location.origin + href,
                        });
                    }  catch {}
                }
            }
        }}
    >
        {@render children()}
    </button>
{:else}
    <a href={href} target={target} class={className} aria-label={ariaLabel}>
        {@render children()}
    </a>
{/if}

