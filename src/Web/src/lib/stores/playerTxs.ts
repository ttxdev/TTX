import { PlayerTransactionDto } from '$lib/api'
import { createOwnedStore } from './owned'

export const {
	store: playerTransactionStore,
	add: addTransaction,
	set: setTransactions
} = createOwnedStore<PlayerTransactionDto>(t => t.player_id);
