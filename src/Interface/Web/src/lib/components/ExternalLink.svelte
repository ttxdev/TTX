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
                await discordSdk.commands.openExternalLink({
                    url: href,
                });
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

