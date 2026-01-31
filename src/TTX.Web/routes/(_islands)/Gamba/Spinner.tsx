import { useEffect, useRef, useState } from "preact/hooks";
import { animate, motion, useMotionValue } from "motion/react";
import { LootBoxResultDto, Rarity } from "@/lib/api.ts";

const rarityColors: Record<Rarity, string> = {
  [Rarity.Pennies]: "#9e9e9e",
  [Rarity.Common]: "#00E676",
  [Rarity.Rare]: "#2979FF",
  [Rarity.Epic]: "#FFD700",
};

const rarityGlow: Record<Rarity, string> = {
  [Rarity.Pennies]: "0px 0px 8px 2px",
  [Rarity.Common]: "0px 0px 15px 5px",
  [Rarity.Rare]: "0px 0px 20px 8px",
  [Rarity.Epic]: "0px 0px 25px 10px",
};

export default function Spinner(
  { result, onComplete }: { result: LootBoxResultDto; onComplete: () => void },
) {
  const [isFinished, setIsFinished] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const winnerRef = useRef<HTMLDivElement>(null);
  const x = useMotionValue(0);

  const winnerIndex = result.rarities.findIndex(
    (r) => r.creator.id === result.result.creator.id,
  );

  useEffect(() => {
    if (!winnerRef.current || !containerRef.current) {
      return;
    }

    setIsFinished(false);
    x.set(0);

    const containerMid = containerRef.current.offsetWidth / 2;
    const winnerLeft = winnerRef.current.offsetLeft;
    const winnerMid = winnerRef.current.offsetWidth / 2;

    const targetX = -(winnerLeft + winnerMid - containerMid);

    animate(x, targetX, {
      duration: 8,
      ease: [0.2, 0.8, 0.2, 1],
      onComplete: () => {
        setIsFinished(true);
        onComplete();
      },
    });
  }, [winnerRef, containerRef]);

  return (
    <div
      ref={containerRef}
      className="relative mb-8 h-52 w-full overflow-hidden rounded-2xl border-2 border-violet-600 bg-black shadow-[0_0_30px_rgba(124,58,237,0.7)]"
    >
      {/* Center Needle Indicator */}
      <div className="pulse-glow absolute top-0 left-1/2 z-20 h-full w-1 -translate-x-1/2 bg-gradient-to-b from-yellow-200 via-yellow-400 to-yellow-200 shadow-[0_0_15px_rgba(251,191,36,0.8)]" />

      {/* Spinning Track */}
      <motion.div
        style={{ x }}
        className="absolute flex items-center h-full will-change-transform"
      >
        {result.rarities.map((item, i) => {
          const isWinner = i === winnerIndex;

          return (
            <div
              key={`${result.result.creator.id}-${i}`}
              ref={isWinner ? winnerRef : null}
              className={`mx-3 flex h-40 w-36 flex-shrink-0 flex-col items-center justify-center rounded-xl border-2 bg-gradient-to-b from-slate-900 to-black p-3 transition-all duration-700 ${
                isWinner && isFinished
                  ? "scale-110 z-10 shadow-[0_0_40px_rgba(251,191,36,0.6)] border-yellow-400"
                  : "grayscale-[0.3]"
              }`}
              style={{
                borderColor: isWinner && isFinished
                  ? "#fbbf24"
                  : rarityColors[item.rarity],
                opacity: isFinished && !isWinner ? 0.5 : 1,
              }}
            >
              <div
                className="mb-2 h-24 w-24 rounded-full border-2 bg-black p-1"
                style={{
                  borderColor: rarityColors[item.rarity],
                  boxShadow: `${rarityGlow[item.rarity]} ${
                    rarityColors[item.rarity]
                  }`,
                }}
              >
                <img
                  className="h-full w-full rounded-full object-cover"
                  src={item.creator.avatar_url}
                  alt={item.creator.name}
                />
              </div>
              <div className="bg-gradient-to-r from-white to-gray-400 bg-clip-text text-lg font-bold text-transparent uppercase">
                ${item.creator.ticker}
              </div>
            </div>
          );
        })}
      </motion.div>
      {/* TODO: tailwind */}
      <style>
        {`
        .pulse-glow {
          animation: needlePulse 2s infinite;
        }
        @keyframes needlePulse {
          0%, 100% { opacity: 0.8; transform: translateX(-50%) scaleY(1); }
          50% { opacity: 1; transform: translateX(-50%) scaleY(1.05); }
        }
      `}
      </style>
    </div>
  );
}
