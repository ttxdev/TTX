<script>
	import ExternalLink from "./ExternalLink.svelte";

	let {
		name = 'Team Member',
		image = 'https://img.daisyui.com/images/stock/photo-1606107557195-0e29a4b5b4aa.webp',
		role = '',
		bio = '',
		socials = {
			twitter: '',
			github: ''
		}
	} = $props();
</script>

<div
	class="card w-full max-w-[250px] rounded-2xl bg-gray-500/20 bg-clip-padding p-2 shadow-md backdrop-blur-sm backdrop-filter"
>
	<ExternalLink href={socials.twitch} target="_blank" className="flex justify-center">
		<img class="size-24 rounded-full bg-gray-400/20 sm:size-32" src={image} alt={name} />
	</ExternalLink>
	<div class="card-body p-2 sm:p-4">
		<div class="flex flex-col">
			<h2 class="card-title p-0 text-lg sm:text-xl">{name}</h2>
			<p class="p-0 text-sm font-medium text-gray-500">{role}</p>
		</div>
		<p class="text-sm sm:text-base">{bio}</p>
		<div class="card-actions mt-2 flex justify-end gap-2 sm:mt-4 sm:gap-4">
			{#if socials.github}
				<ExternalLink
					href={socials.github}
					target="_blank"
					className="btn btn-circle btn-ghost text-black hover:bg-white dark:text-white dark:hover:bg-neutral-700"
					ariaLabel="GitHub"
				>
					<svg
						xmlns="http://www.w3.org/2000/svg"
						width="24"
						height="24"
						viewBox="0 0 24 24"
						fill="currentColor"
					>
						<path
							d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"
						/>
					</svg>
				</ExternalLink>
			{/if}
			{#if socials.twitch}
				<ExternalLink
					href={socials.twitch}
					target="_blank"
					className="btn btn-circle btn-ghost text-violet-400 hover:bg-white dark:text-white dark:hover:bg-purple-700"
					ariaLabel="Twitch"
				>
					<svg
						width="24"
						height="24"
						viewBox="0 0 24 24"
						xmlns="http://www.w3.org/2000/svg"
						fill="currentColor"
					>
						<path
							d="M11.571 4.714h1.715v5.143H11.57zm4.715 0H18v5.143h-1.714zM6 0L1.714 4.286v15.428h5.143V24l4.286-4.286h3.428L22.286 12V0zm14.571 11.143l-3.428 3.428h-3.429l-3 3v-3H6.857V1.714h13.714Z"
						/>
					</svg>
				</ExternalLink>
			{/if}
		</div>
	</div>
</div>
