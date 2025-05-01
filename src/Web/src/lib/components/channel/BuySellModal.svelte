<script lang="ts">
	import { onMount } from 'svelte';
	import { fly, fade } from 'svelte/transition';
	import { errorToast } from '$lib/toast';
	import { formatTicker, formatValue } from '$lib/util';
	import { getApiClient } from '$lib';
	import { invalidateAll } from '$app/navigation';
	import { TransactionAction, type TTXClient } from '$lib/api';
	import { Tween } from 'svelte/motion';
	import { token, user } from '$lib/stores/data';

	type ModalProps = {
		type: TransactionAction | null;
		slug: string;
		ticker: string;
		price: number;
	};

	const FEE_RATE = 0.0;
	const buyLimit = 1000;
	const tweenOpts = { duration: 300, easing: (t: number) => t };
	const longTweenOpts = { duration: 1000, easing: (t: number) => t };

	let userOwns: number = $state(0);
	let userBalance: number = $state(0);
	let client: TTXClient | null = null;
	let amount: number | undefined = $state(undefined);

	let { type = $bindable(), slug, ticker, price: priceProp }: ModalProps = $props();

	let animatedPrice = new Tween(priceProp, tweenOpts);
	let animatedCost = new Tween(0, tweenOpts);
	let animatedFee = new Tween(0, tweenOpts);
	let animatedTotal = new Tween(0, tweenOpts);
	let animatedAmount = new Tween(0, tweenOpts);

	let animatedUserOwns = new Tween(userOwns, longTweenOpts);
	let animatedUserBalance = new Tween(userBalance, longTweenOpts);

	let visible: boolean = $state(false);

	let transactionPrice: number = $state(0);
	let transactionAmount: number = $state(0);

	$effect(() => {
		animatedPrice.set(priceProp);
	});

	$effect(() => {
		const amt = amount === undefined ? 0 : amount;
		if (amount !== undefined) {
			const max = type === TransactionAction.Buy ? getMaxBuyable() : getMaxSellable();
			amount = Math.min(Math.max(amt, 1), max);
		}
		animatedCost.set(amt * animatedPrice.target);
		animatedAmount.set(amt);
		if (type === TransactionAction.Buy) {
			animatedFee.set(amt * animatedPrice.target * FEE_RATE);
			animatedTotal.set(amt * animatedPrice.target * (1 + FEE_RATE));
		} else {
			animatedFee.set(0);
			animatedTotal.set(amt * animatedPrice.target);
		}
	});

	let cannotAfford: boolean = $derived(
		amount === undefined
			? true
			: type === TransactionAction.Buy
				? amount * animatedPrice.target * (1 + FEE_RATE) > userBalance ||
					userOwns + amount > buyLimit
				: amount > userOwns
	);

	function getMaxBuyable(): number {
		if (animatedPrice.target <= 0) return 0;
		const effectivePricePerShare = animatedPrice.target * (1 + FEE_RATE);
		const balanceMax = Math.floor(userBalance / effectivePricePerShare);
		const capMax = buyLimit - userOwns;
		return Math.max(0, Math.min(balanceMax, capMax));
	}

	function getMaxSellable(): number {
		return userOwns;
	}

	let isLoading: boolean = $state(false);
	let isSuccess: boolean = $state(false);

	async function handleTransaction(): Promise<void> {
		if (!client || amount === undefined) {
			errorToast('Please enter an amount.');
			return;
		}
		try {
			const data = await client.placeOrder({
				creator: slug,
				action: type!,
				amount: amount,
			});
			if (type === TransactionAction.Buy) {
				userBalance -= data.value * data.quantity;
				userOwns += data.quantity;
			} else if (type === TransactionAction.Sell) {
				userBalance += data.value * data.quantity;
				userOwns -= data.quantity;
			}

			transactionPrice = data.value;
			transactionAmount = data.quantity;

			animatedUserOwns.set(userOwns);
			animatedUserBalance.set(userBalance);
			isSuccess = true;
		} catch (error) {
			console.error('Error creating transaction:', error);
			errorToast('An error occurred while processing your request. Please try again.');
			type = null;
		} finally {
			invalidateAll();
			isLoading = false;
		}
	}

	async function handleBuySell(): Promise<void> {
		isLoading = true;
		isSuccess = false;
		await handleTransaction();
	}

	function setAmount(percentage: 'max' | 'half' | 'quarter'): void {
		const max = type === TransactionAction.Buy ? getMaxBuyable() : getMaxSellable();
		if (percentage === 'max') {
			amount = max;
		} else if (percentage === 'half') {
			amount = Math.floor(max / 2);
		} else if (percentage === 'quarter') {
			amount = Math.floor(max / 4);
		}
		if (amount && amount < 1) amount = 1;
	}

	async function initModal(): Promise<void> {
		if ($user && $token) {
			isLoading = true;
			try {
				client = getApiClient($token);
				const user = await client.getSelf();
				userBalance = user.credits;
				userOwns = user.shares.find((share) => share.creator.slug === slug)?.quantity || 0;
				animatedUserBalance.set(userBalance);
				animatedUserOwns.set(userOwns);
			} catch (error) {
				console.error('Error fetching user data:', error);
				errorToast('Error fetching user data. Please try again.');
				type = null;
			} finally {
				isLoading = false;
			}
		} else {
			errorToast(`You need to be logged in to ${type} shares`);
			type = null;
		}
	}

	function cancel(): void {
		if (isLoading) return;
		type = null;
	}

	onMount(() => {
		initModal();
		visible = true;
	});
</script>

{#if type !== null}
	<div class="modal modal-open z-100">
		<button onclick={cancel} aria-label="close">
			<div class="fixed inset-0 bg-black/30 backdrop-blur-md"></div>
		</button>
		{#if visible}
			<div
				class="modal-box relative z-10 w-full max-w-md rounded-2xl p-6 shadow-lg"
				in:fly={{ y: 1000, duration: 800 }}
				out:fade
			>
				{#if !isLoading}
					<button class="btn btn-sm btn-circle btn-ghost absolute top-2 right-2" onclick={cancel}>
						✕
					</button>
				{/if}
				{#if isSuccess}
					<div class="mt-4 flex flex-col items-center justify-center space-y-4">
						<svg
							xmlns="http://www.w3.org/2000/svg"
							class="mb-4 h-12 w-12 text-green-500"
							fill="none"
							viewBox="0 0 24 24"
							stroke="currentColor"
						>
							<path
								stroke-linecap="round"
								stroke-linejoin="round"
								stroke-width="2"
								d="M5 13l4 4L19 7"
							/>
						</svg>
						<div class="rounded-xl p-6 text-center shadow-lg">
							<h3 class="mb-2 text-2xl font-bold">
								{type === TransactionAction.Buy ? 'Purchase Successful' : 'Sale Successful'}
							</h3>
							<p class="mb-4">
								You {type === TransactionAction.Buy ? 'bought' : 'sold'}
								{transactionAmount} share{transactionAmount !== 1 ? 's' : ''} of {formatTicker(
									ticker
								)} at {formatValue(transactionPrice)} per share.
							</p>
							<div class="grid grid-cols-2 gap-4">
								<div class="rounded-lg border p-2">
									<p class="text-sm text-gray-500">Shares Owned</p>
									<p class="text-xl font-semibold">{Math.round(animatedUserOwns.current)}</p>
								</div>
								<div class="rounded-lg border p-2">
									<p class="text-sm text-gray-500">New Balance</p>
									<p class="text-xl font-semibold">{formatValue(animatedUserBalance.current)}</p>
								</div>
							</div>
						</div>
					</div>
				{:else}
					<h3 class="mb-4 text-2xl font-bold">
						{type === TransactionAction.Buy ? 'Buy Shares' : 'Sell Shares'} - {formatTicker(ticker)}
					</h3>
					<div class="flex flex-col space-y-4">
						<div class="flex justify-between text-sm text-gray-500">
							{#if type === TransactionAction.Buy}
								<span>You have <strong>{formatValue(userBalance)}</strong> available</span>
							{:else}
								<span>You own <strong>{userOwns}</strong> shares</span>
							{/if}
						</div>
						<div>
							<label class="mb-2 block text-sm font-semibold text-gray-700" for="amount-input">
								{type === TransactionAction.Buy ? 'Amount to Buy' : 'Amount to Sell'}
							</label>
							<div class="join flex items-center justify-center" id="amount-input">
								<div class="relative">
									<input
										type="number"
										class="input-bordered input custom-number-input flex-1 rounded-l-2xl rounded-r-none border-purple-400 focus:outline-none"
										bind:value={amount}
										min="1"
										step="1"
										placeholder="Enter amount"
									/>
									<div
										class="custom-spinner absolute inset-y-0 right-2 flex flex-col justify-center"
									>
										<button
											type="button"
											aria-label="Increase amount"
											class="spinner-btn"
											onclick={() => (amount = (amount || 0) + 1)}
										>
											<svg
												xmlns="http://www.w3.org/2000/svg"
												viewBox="0 0 16 16"
												fill="currentColor"
												class="size-4"
											>
												<path
													fill-rule="evenodd"
													d="M11.78 9.78a.75.75 0 0 1-1.06 0L8 7.06 5.28 9.78a.75.75 0 0 1-1.06-1.06l3.25-3.25a.75.75 0 0 1 1.06 0l3.25 3.25a.75.75 0 0 1 0 1.06Z"
													clip-rule="evenodd"
												/>
											</svg>
										</button>
										<button
											type="button"
											aria-label="Decrease amount"
											class="spinner-btn"
											onclick={() => (amount = Math.max((amount || 1) - 1, 1))}
										>
											<svg
												xmlns="http://www.w3.org/2000/svg"
												viewBox="0 0 16 16"
												fill="currentColor"
												class="size-4"
											>
												<path
													fill-rule="evenodd"
													d="M4.22 6.22a.75.75 0 0 1 1.06 0L8 8.94l2.72-2.72a.75.75 0 1 1 1.06 1.06l-3.25 3.25a.75.75 0 0 1-1.06 0L4.22 7.28a.75.75 0 0 1 0-1.06Z"
													clip-rule="evenodd"
												/>
											</svg>
										</button>
									</div>
								</div>

								<button
									type="button"
									class="btn rounded-l-none border-purple-400 bg-purple-400 text-xl font-bold text-white hover:bg-purple-400/90 focus:outline-none"
									onclick={() => setAmount('quarter')}>¼</button
								>
								<button
									type="button"
									class="btn rounded-l-none border-x-purple-900 border-y-purple-400 bg-purple-400 text-xl font-bold text-white hover:bg-purple-400/90 focus:outline-none"
									onclick={() => setAmount('half')}>½</button
								>
								<button
									type="button"
									class="btn rounded-l-none rounded-r-2xl border-purple-400 bg-purple-400 font-bold text-white hover:bg-purple-400/90 focus:outline-none"
									onclick={() => setAmount('max')}>Max</button
								>
							</div>
						</div>
						<div class="rounded-lg border p-4">
							<div class="mb-2 flex justify-between text-sm">
								<span>Price per Share</span>
								<span>{formatValue(animatedPrice.current)}</span>
							</div>
							<div class="mb-2 flex justify-between text-sm">
								<span>Number of Shares</span>
								<span>{amount === undefined ? '—' : Math.round(animatedAmount.current)}</span>
							</div>
							<hr class="my-2" />
							<div class="flex justify-between text-lg font-bold">
								<span>Total</span>
								<span>{formatValue(animatedTotal.current)}</span>
							</div>
						</div>
					</div>
				{/if}
				<div class="mt-6 flex flex-col space-y-2">
					{#if !isSuccess}
						<button
							class="btn w-full rounded-2xl bg-purple-400 font-black text-white disabled:bg-gray-300"
							onclick={handleBuySell}
							disabled={isLoading || cannotAfford || amount === undefined || amount < 1}
						>
							{#if isLoading}
								<span class="loading loading-spinner loading-sm"></span>
							{:else if amount === undefined}
								Please enter an amount
							{:else if amount === 0}
								Cant buy less than 1 share
							{:else}
								{type === TransactionAction.Buy ? 'Buy Shares' : 'Sell Shares'}
							{/if}
						</button>
						{#if !isLoading && amount !== undefined}
							<p class="text-center text-sm text-red-500">
								{#if type === TransactionAction.Buy && userOwns === buyLimit}
									You have reached the maximum share limit ({buyLimit} shares)
								{:else if cannotAfford && type === TransactionAction.Buy}
									You do not have enough balance to complete this purchase.
								{:else if cannotAfford && type === TransactionAction.Sell}
									You do not own enough shares to sell this amount.
								{/if}
							</p>
						{/if}
					{/if}
				</div>
			</div>
		{/if}
	</div>
{/if}

<style>
	/* Hide native spinner buttons in Chrome, Edge, Safari */
	.custom-number-input::-webkit-inner-spin-button,
	.custom-number-input::-webkit-outer-spin-button {
		-webkit-appearance: none;
		margin: 0;
	}

	/* Hide the spinner in Firefox */
	.custom-number-input {
		-moz-appearance: textfield;
	}

	.custom-spinner {
		pointer-events: auto; /* allow clicks on the buttons */
	}

	.spinner-btn {
		background: transparent;
		border: none;
		padding: 0.2rem;
		cursor: pointer;
	}

	/* Ensure the SVG arrows use purple-400 */
	.spinner-btn svg {
		stroke: #c27aff;
	}
</style>
