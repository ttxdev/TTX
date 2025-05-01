<script lang="ts">
	interface Props {
		gameState: 'idle' | 'spinning' | 'complete' | 'showing_modal';
		onSpin: () => void;
	}

	const { gameState, onSpin }: Props = $props();
</script>

<button
	class="cursor-pointer rounded-lg bg-gradient-to-r from-amber-400 to-yellow-500 px-8 py-5 text-xl font-bold text-black transition-all hover:not-disabled:scale-105 disabled:cursor-not-allowed disabled:opacity-50"
	onclick={onSpin}
	disabled={gameState !== 'idle'}
>
	{gameState === 'spinning' ? 'Spinning...' : gameState !== 'idle' ? 'Complete' : 'SPIN'}
</button>
