<script lang="ts">
	import { PUBLIC_DISCORD_URL } from '$env/static/public';
</script>

<footer class="footer sm:footer-horizontal footer-center text-base-content bg-transparent p-4">
	<aside>
		<p>Copyright © {new Date().getFullYear()} - All right reserved.</p>
		<div class="flex flex-row gap-4">
			<a href={PUBLIC_DISCORD_URL} target="_blank" class="w-fit text-purple-500 hover:underline">
				Join our Discord!
			</a>
			<a href="/privacy" class="text-purple-500 hover:underline">Privacy Policy</a>
		</div>
	</aside>
</footer>
