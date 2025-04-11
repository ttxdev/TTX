<script lang="ts">
	import { PUBLIC_DISCORD_URL } from '$env/static/public';
	import ExternalLink from './ExternalLink.svelte';
</script>

<footer class="footer sm:footer-horizontal footer-center text-base-content bg-transparent p-4">
	<aside>
		<p>Copyright © {new Date().getFullYear()} - All right reserved.</p>
		<div class="flex flex-row gap-4">
			<ExternalLink href={PUBLIC_DISCORD_URL} target="_blank" className="w-fit text-purple-500 hover:underline">
				Join our Discord!
			</ExternalLink>
			<a href="/privacy" class="text-purple-500 hover:underline">Privacy Policy</a>
		</div>
	</aside>
</footer>
