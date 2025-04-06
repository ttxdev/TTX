<script lang="ts"></script>

<svelte:head>
	<title>TTX - Privacy Policy</title>
	<meta
		name="description"
		content="Privacy Policy for TTX, a fantasy stock market for Twitch streamers."
	/>
</svelte:head>

<div class="mx-auto flex w-full max-w-[800px] flex-col gap-4 p-4">
	<h1 class="font-display my-2 text-2xl max-md:text-2xl">Privacy Policy</h1>
	<section>
		<p>
			<strong
				>TTX is a parody website and is not affiliated with Twitch or any other third-party
				services.</strong
			>
			This Privacy Policy outlines how we collect, use, and safeguard your information when you use our
			website and services. Our privacy policy is broken up into sections for your convenience.
		</p>
	</section>
	<hr />
	<section class="flex flex-col gap-4">
		<div>
			<h2 class="font-display text- my-2">TTX Account Privacy Policy</h2>
			<p>The following outlines how we handle your data when you create an account on TTX.</p>
		</div>
		<div>
			<h3 class="font-bold">Integration with Twitch</h3>
			<p>
				We collect your Twitch username, Twitch user ID, and Twitch avatar URL when you authenticate
				through Twitch. We use this information to create your profile on TTX. This data is securely
				stored and is not shared with third parties for marketing or other purposes.
			</p>
			<br />
			<p>
				When navigating to the Twitch Login page, it lists out the permissions we request. If any
				data was requested it would show "Authorizing will Allow TTX.gg to." Since we do not request
				any information this section is hidden.
			</p>
			<div
				class="my-4 flex w-full flex-col items-center justify-between gap-8 md:w-72 md:flex-row md:items-stretch"
			>
				<img src="/privacy/oauth with email.png" alt="" class="" />
				<img src="/privacy/oauth without email.png" alt="" class="" />
			</div>
		</div>
		<div>
			<h3 class="font-bold">Data Retention</h3>
			<p>
				We retain your Twitch profile data (username, user ID, avatar URL) for as long as your TTX
				account is active. You may request deletion at any time by contacting us.
			</p>
		</div>
		<div>
			<h3 class="font-bold">Your Rights</h3>
			<p>
				You may have rights under applicable data-protection laws, including the right to access or
				delete your personal data.
			</p>
		</div>
		<div>
			<h3 class="font-bold">Contact Us</h3>
			<p>
				If you have questions, concerns, or would like to exercise any rights related to your
				personal data then please reach us at <a
					href="mailto:contact@ttx.gg"
					class="text-violet-400 hover:underline">contact@ttx.gg</a
				>.
			</p>
		</div>
	</section>
	<hr />
	<section>
		<h2 class="font-display text- my-2">TTX Chat Value Monitoring Privacy Policy</h2>
		<p class="text-justify">
			We integrate with Twitch API to monitor and process Twitch channel messages. This is done to
			monitor the value of our Creators. We do not store any messages or personal data from Twitch
			users. We only store the sentiment value, an integer, that represents the sentiment of the
			message.
		</p>
	</section>
	<hr />
	<section>
		<div>
			<h2 class="font-display my-2 text-xl">Updates to This Policy</h2>
			<p>
				We may update this Privacy Policy from time to time. Any changes will be posted here with an
				updated revision date. This Privacy Policy was last updated on <strong
					>April 2nd, 2025</strong
				>.
			</p>
		</div>
	</section>
</div>
