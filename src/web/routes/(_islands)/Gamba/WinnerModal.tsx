import { useEffect, useRef } from "preact/hooks";
import { AnimatePresence, motion } from "motion/react";
import Confetti from "confetti-js";
import { formatTicker, formatValue } from "@/lib/formatting.ts";
import {
  type CreatorPartialDto,
  type LootBoxResultDto,
  Rarity,
} from "@/lib/api.ts";
import Modal from "../../../islands/Modal.tsx";

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

export default function WinnerModal({ result, onClose }: {
  result: LootBoxResultDto;
  onClose: () => void;
}) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const rarity = result.result.rarity;
  const creator = result.result.creator;

  useEffect(() => {
    if (canvasRef.current) {
      const confetti = Confetti({
        target: canvasRef.current,
        max: 80,
        size: 1.2,
        animate: true,
        props: ["circle", "square", "triangle", "line"],
        colors: [[165, 104, 246], [230, 61, 135], [0, 199, 235], [
          253,
          224,
          71,
        ]], // Match your theme
        clock: 25,
      });
      confetti.render();

      return () => confetti.clear();
    }
  }, []);

  return (
    <Modal isOpen onClose={onClose}>
      <div className="fixed inset-0 z-50 flex items-center justify-center overflow-hidden">
        {/* Backdrop Fade */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          onClick={onClose}
          className="fixed inset-0 bg-black/80 backdrop-blur-sm"
        />

        <canvas
          ref={canvasRef}
          id="winner-modal-canvas"
          className="pointer-events-none absolute inset-0 h-full w-full"
        />

        {/* Modal Scale (Elastic) */}
        <motion.div
          role="dialog"
          aria-modal="true"
          initial={{ scale: 0.8, opacity: 0 }}
          animate={{
            scale: 1,
            opacity: 1,
            transition: {
              type: "spring",
              damping: 12, // Lower damping = more "elastic" bounce
              stiffness: 100,
              delay: 0.2,
            },
          }}
          exit={{ scale: 0.8, opacity: 0 }}
          className="relative mx-4 flex w-full max-w-md flex-col items-center"
        >
          <h2 className="mb-6 bg-gradient-to-r from-amber-300 via-yellow-500 to-amber-300 bg-clip-text text-5xl font-black text-transparent italic tracking-tighter">
            YOU WON!
          </h2>

          <div
            className="flex w-full flex-col items-center rounded-3xl border-2 bg-gradient-to-b from-slate-900 to-black p-8 shadow-[0_0_50px_rgba(124,58,237,0.4)]"
            style={{ borderColor: rarityColors[rarity] }}
          >
            <div className="mb-6 bg-gradient-to-r from-amber-300 via-yellow-500 to-amber-300 bg-clip-text text-4xl font-bold text-transparent">
              {formatValue(creator.value)}
            </div>

            <div
              className="animate-pulse-slow mb-6 h-40 w-40 rounded-full border-4 bg-black p-1"
              style={{
                borderColor: rarityColors[rarity],
                boxShadow: `${rarityGlow[rarity]} ${rarityColors[rarity]}`,
              }}
            >
              <img
                className="h-full w-full rounded-full object-cover"
                src={creator.avatar_url}
                alt={creator.name}
              />
            </div>

            <div className="mb-2 w-full overflow-hidden text-center text-2xl font-bold text-ellipsis whitespace-nowrap text-white">
              {creator.name}
            </div>

            <div className="bg-gradient-to-r from-amber-200 via-yellow-300 to-amber-200 bg-clip-text text-xl font-bold text-transparent uppercase tracking-widest">
              {formatTicker(creator.ticker)}
            </div>
          </div>

          <a
            className="mt-10 cursor-pointer rounded-2xl bg-gradient-to-r from-violet-600 to-purple-700 px-10 py-5 text-xl font-black text-white shadow-lg transition-transform hover:scale-110 active:scale-95 hover:shadow-[0_0_30px_rgba(124,58,237,0.6)]"
            href={`/creators/${creator.slug}`}
          >
            VIEW CREATOR
          </a>
        </motion.div>

        <style>
          {`
          .animate-pulse-slow {
            animation: pulseSlow 3s ease-in-out infinite;
          }
          @keyframes pulseSlow {
            0%, 100% { transform: scale(1); filter: brightness(1); }
            50% { transform: scale(1.05); filter: brightness(1.2); }
          }
        `}
        </style>
      </div>
    </Modal>
  );
}
