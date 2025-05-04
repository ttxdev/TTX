import type { PortfolioDto } from '$lib/api';
import { createOwnedStore } from './owned';

export const { store: portfolioStore, add: addPortfolio, set: setPortfolio } = createOwnedStore<PortfolioDto>(
  p => p.player_id
);
