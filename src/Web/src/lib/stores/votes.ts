import type { VoteDto } from '$lib/api';
import { createCreatorStore } from './creators';

export const { store: voteStore, add: addVote, set: setVotes } = createCreatorStore<VoteDto>();
