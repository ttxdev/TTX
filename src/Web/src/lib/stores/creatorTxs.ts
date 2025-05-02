import { CreatorTransactionDto } from '$lib/api'
import { createOwnedStore } from './owned'

export const {
	store: creatorTransactionStore,
	add: addTransaction,
	set: setTransactions
} = createOwnedStore<CreatorTransactionDto>(t => t.creator_id);
