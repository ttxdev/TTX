<script lang="ts">
	import { getContext, onMount } from 'svelte';
	import { page } from '$app/state';
	import { Spring } from 'svelte/motion';
	import { discordSdk } from '$lib/discord';
	import type { IPlayerDto } from '$lib/api';

	const urls = [
		{
			url: '/creators',
			label: 'Creators'
		},
		{
			url: '/players',
			label: 'Leaderboard'
		},
		{
			url: '/team',
			label: 'About Us'
		}
	] as const;


	let { searchModal = $bindable() }: { searchModal: boolean } = $props();

	const user = getContext<IPlayerDto>('user');
	let isMenuOpen = $state(false);

	function toggleMenu() {
		isMenuOpen = !isMenuOpen;
	}

	let navContainer: HTMLElement | null = $state(null);
	let navLinks = $state(new Map<string, HTMLElement>());

	const indicatorLeft = new Spring(0, { stiffness: 0.2, damping: 0.4 });
	const indicatorWidth = new Spring(0, { stiffness: 0.2, damping: 0.4 });

	function updateIndicator() {
		const currentPath = page.url.pathname;
		if (navContainer) {
			const activeEl = navContainer.querySelector(`a[href="${currentPath}"]`) as HTMLElement;
			if (activeEl && navLinks.has(currentPath)) {
				navLinks.forEach((el, path) => {
					if (path === currentPath) {
						el.classList.add('active');
					} else {
						el.classList.remove('active');
					}
				});
			}
			if (activeEl) {
				const { left, width } = activeEl.getBoundingClientRect();
				const containerLeft = navContainer.getBoundingClientRect().left;
				indicatorLeft.set(left - containerLeft);
				indicatorWidth.set(width);
			} else {
				indicatorWidth.set(0);
			}
		}
	}

	onMount(() => {
		updateIndicator();
		window.addEventListener('resize', updateIndicator);
		return () => {
			window.removeEventListener('resize', updateIndicator);
		};
	});

	$effect(() => updateIndicator());
</script>

<div
	class="navbar fixed left-1/2 z-20 mx-auto mt-4 w-[95%] -translate-x-1/2 rounded-xl bg-clip-padding p-6 shadow-sm backdrop-blur backdrop-contrast-100 backdrop-saturate-100 backdrop-filter"
>
	<div class="navbar-start">
		<button class="btn btn-ghost lg:hidden" onclick={toggleMenu} aria-label="Toggle menu">
			<svg
				xmlns="http://www.w3.org/2000/svg"
				class="h-5 w-5"
				fill="none"
				viewBox="0 0 24 24"
				stroke="currentColor"
			>
				<path
					stroke-linecap="round"
					stroke-linejoin="round"
					stroke-width="2"
					d="M4 6h16M4 12h8m-8 6h16"
				/>
			</svg>
		</button>
		<a href="/" class="btn btn-ghost rounded-2xl text-xl">
			<img class="w-8" src="/ttx-logo-only.png" alt="TTX Logo" />
		</a>
		<div class="badge badge-ghost rounded-xl">BETA</div>
	</div>

	<div class="navbar-center hidden lg:flex">
		<ul bind:this={navContainer} class="menu menu-horizontal relative px-1">
			<div
				class="absolute rounded-2xl bg-purple-400/20 duration-100 animate-all"
				style="left: {indicatorLeft.current}px; width: {indicatorWidth.current}px; top: 0; bottom: 0; z-index: 0;"
			></div>

			{#each urls as {url, label}}
				<li
					class:active={page.url.pathname === url}
				>
					<a
						href={url}
						class="relative z-10 hover:bg-transparent"
					>
						{label}
					</a>
				</li>
			{/each}

			<li>
				<button onclick={() => (searchModal = true)} class="relative z-10 rounded-2xl">
					Search
					<svg
						xmlns="http://www.w3.org/2000/svg"
						fill="none"
						viewBox="0 0 24 24"
						stroke-width="1.5"
						stroke="currentColor"
						class="size-5"
					>
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z"
						/>
					</svg>
				</button>
			</li>
		</ul>
	</div>

	<div class="navbar-end">
		<div class="flex gap-2">
			{#if user}
				<div class="join">
					<div class=""></div>
					<a href={'/players/' + user.name}>
						<div
							class="btn rounded-lg bg-black px-3 py-2 text-white shadow md:rounded-l-lg md:rounded-r-none"
						>
							<div class="flex flex-row items-center justify-between gap-2">
								<img src={user.avatar_url} alt="" class="size-6 rounded-full" />
								{user.name}
							</div>
						</div>
					</a>
					<a href="/api/logout" class="hidden md:block">
						<div class="btn rounded-r-lg bg-black py-2 text-white shadow hover:text-red-400">
							<svg
								xmlns="http://www.w3.org/2000/svg"
								viewBox="0 0 16 16"
								fill="currentColor"
								class="size-4"
							>
								<path
									fill-rule="evenodd"
									d="M14 4.75A2.75 2.75 0 0 0 11.25 2h-3A2.75 2.75 0 0 0 5.5 4.75v.5a.75.75 0 0 0 1.5 0v-.5c0-.69.56-1.25 1.25-1.25h3c.69 0 1.25.56 1.25 1.25v6.5c0 .69-.56 1.25-1.25 1.25h-3c-.69 0-1.25-.56-1.25-1.25v-.5a.75.75 0 0 0-1.5 0v.5A2.75 2.75 0 0 0 8.25 14h3A2.75 2.75 0 0 0 14 11.25v-6.5Zm-9.47.47a.75.75 0 0 0-1.06 0L1.22 7.47a.75.75 0 0 0 0 1.06l2.25 2.25a.75.75 0 1 0 1.06-1.06l-.97-.97h7.19a.75.75 0 0 0 0-1.5H3.56l.97-.97a.75.75 0 0 0 0-1.06Z"
									clip-rule="evenodd"
								/>
							</svg>
							<span>Logout</span>
						</div>
					</a>
				</div>
			{:else}
				<a
					href="/api/login?from={page.url.pathname}"
					class="flex items-center justify-center gap-2"
				>
					<div class="btn rounded-md bg-black px-3 py-2 text-white shadow">
						<svg width="16" height="16" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
							<path
								fill="white"
								d="M11.571 4.714h1.715v5.143H11.57zm4.715 0H18v5.143h-1.714zM6 0L1.714 4.286v15.428h5.143V24l4.286-4.286h3.428L22.286 12V0zm14.571 11.143l-3.428 3.428h-3.429l-3 3v-3H6.857V1.714h13.714Z"
							/>
						</svg>
						Login with Twitch
					</div>
				</a>
			{/if}
		</div>
	</div>
</div>

{#if isMenuOpen}
	<button
		class="opaci fixed inset-0 z-50 bg-black transition-opacity duration-300"
		class:opacity-70={isMenuOpen}
		class:opacity-0={!isMenuOpen}
		onclick={toggleMenu}
		aria-label="Close menu"
	></button>
{/if}

<div
	class="fixed left-0 top-0 z-50 h-screen w-64 bg-white shadow-lg backdrop-blur backdrop-contrast-100 backdrop-saturate-200 backdrop-filter transition-all duration-500 ease-in-out lg:hidden dark:bg-black"
	class:translate-x-0={isMenuOpen}
	class:-translate-x-full={!isMenuOpen}
>
	<div class="mt-4 flex h-full flex-col">
		<div class="flex items-center justify-between p-4">
			<a href="/" class="btn btn-ghost rounded-2xl text-xl" onclick={toggleMenu}>
				<img class="w-8" src="/ttx-logo-only.png" alt="TTX Logo" />
			</a>
			<button class="btn btn-ghost" onclick={toggleMenu} aria-label="Close menu">
				<svg
					xmlns="http://www.w3.org/2000/svg"
					class="h-6 w-6"
					fill="none"
					viewBox="0 0 24 24"
					stroke="currentColor"
				>
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M6 18L18 6M6 6l12 12"
					/>
				</svg>
			</button>
		</div>
		<nav class="flex-1 overflow-y-auto p-4">
			<ul class="space-y-4">
				{#each urls as {url, label}}
					<li>
						<a
							href={url}
							class="block rounded-lg px-4 py-2 text-lg font-medium underline-offset-2 hover:bg-gray-100"
							class:underline={page.url.pathname === url}
							onclick={toggleMenu}
						>
							{label}
						</a>
					</li>
				{/each}
				<li>
					<button
						class="block rounded-lg px-4 py-2 text-lg font-medium underline-offset-2 hover:bg-gray-100"
						onclick={() => {
							searchModal = true;
							toggleMenu();
						}}
					>
						Search
						<svg
							xmlns="http://www.w3.org/2000/svg"
							fill="none"
							viewBox="0 0 24 24"
							stroke-width="1.5"
							stroke="currentColor"
							class="ml-2 inline-block size-5"
						>
							<path
								stroke-linecap="round"
								stroke-linejoin="round"
								d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z"
							/>
						</svg>
					</button>
				</li>
			</ul>
		</nav>
		<div class="mb-2 border-t p-4">
			<div class="flex flex-col gap-2">
				{#if user}
					<a href={'/players/' + user.name}>
						<div class="btn w-full rounded-md bg-black px-3 py-2 text-white shadow">
							<div class="flex flex-row items-center justify-between gap-2">
								<img src={user.avatar_url} alt="" class="size-6 rounded-full" />
								{user.name}
							</div>
						</div>
					</a>
					<a href="/api/logout">
						<div class="btn w-full rounded-lg bg-black py-2 text-white shadow">
							<svg
								xmlns="http://www.w3.org/2000/svg"
								viewBox="0 0 16 16"
								fill="currentColor"
								class="size-4"
							>
								<path
									fill-rule="evenodd"
									d="M14 4.75A2.75 2.75 0 0 0 11.25 2h-3A2.75 2.75 0 0 0 5.5 4.75v.5a.75.75 0 0 0 1.5 0v-.5c0-.69.56-1.25 1.25-1.25h3c.69 0 1.25.56 1.25 1.25v6.5c0 .69-.56 1.25-1.25 1.25h-3c-.69 0-1.25-.56-1.25-1.25v-.5a.75.75 0 0 0-1.5 0v.5A2.75 2.75 0 0 0 8.25 14h3A2.75 2.75 0 0 0 14 11.25v-6.5Zm-9.47.47a.75.75 0 0 0-1.06 0L1.22 7.47a.75.75 0 0 0 0 1.06l2.25 2.25a.75.75 0 1 0 1.06-1.06l-.97-.97h7.19a.75.75 0 0 0 0-1.5H3.56l.97-.97a.75.75 0 0 0 0-1.06Z"
									clip-rule="evenodd"
								/>
							</svg>
							<span>Logout</span>
						</div>
					</a>
				{:else if !discordSdk}
					<a
						href="/api/login?from={page.url.pathname}"
						class="flex items-center justify-center gap-2"
					>
						<div class="btn w-full rounded-md bg-black px-3 py-2 text-white shadow">
							<svg width="16" height="16" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
								<path
									fill="black"
									d="M11.571 4.714h1.715v5.143H11.57zm4.715 0H18v5.143h-1.714zM6 0L1.714 4.286v15.428h5.143V24l4.286-4.286h3.428L22.286 12V0zm14.571 11.143l-3.428 3.428h-3.429l-3 3v-3H6.857V1.714h13.714Z"
								/>
							</svg>
							Login with Twitch
						</div>
					</a>
				{/if}
			</div>
		</div>
	</div>
</div>

<style>
	.menu li > *:active,
	.menu li > details > summary:active,
	.menu li > *.menu-active {
		background-color: transparent !important;
		color: inherit !important;
		box-shadow: none !important;
	}
</style>
