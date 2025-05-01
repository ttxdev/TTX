import type { CreatorTransactionDto } from '$lib/api';
import { createCreatorStore } from './creators';

export const {
	store: transactionStore,
	add: addTransaction,
	set: setTransactions
} = createCreatorStore<CreatorTransactionDto>();
