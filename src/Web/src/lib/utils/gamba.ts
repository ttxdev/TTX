import { Rarity } from "$lib/api";

	export const rarityColors: Record<Rarity, string> = {
		[Rarity.Pennies]: '#9e9e9e',
		[Rarity.Common]: '#00E676',
		[Rarity.Rare]: '#2979FF',
		[Rarity.Epic]: '#FFD700'
	};

export const rarityGlow: Record<Rarity, string> = {
	[Rarity.Pennies]: '0px 0px 8px 2px',
	[Rarity.Common]: '0px 0px 15px 5px',
	[Rarity.Rare]: '0px 0px 20px 8px',
	[Rarity.Epic]: '0px 0px 25px 10px'
};