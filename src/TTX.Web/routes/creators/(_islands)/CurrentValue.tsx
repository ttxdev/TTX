import { useEffect, useRef, useState } from "preact/hooks";
import {
  animate,
  AnimatePresence,
  motion,
  useMotionValue,
  useMotionValueEvent,
} from "motion/react";
import { formatValue } from "@/lib/formatting.ts";

type Props = {
  value: number;
};

const AnimatedNumber = ({ mv }: { mv: any }) => {
  const ref = useRef<HTMLSpanElement>(null);

  useMotionValueEvent(mv, "change", (latest) => {
    if (ref.current) {
      // @ts-ignore
      ref.current.textContent = formatValue(latest);
    }
  });

  return <span ref={ref}>{formatValue(mv.get())}</span>;
};

// Svelte's custom easing re-implemented for Motion
const customEase = (t: number) =>
  t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2;

export default function CurrentValue({ value }: Props) {
  const [showDiff, setShowDiff] = useState(false);
  const [displayedDiff, setDisplayedDiff] = useState(0);
  const [direction, setDirection] = useState<"up" | "down" | "none">("none");

  const previousValue = useRef(value);
  const diffTimeout = useRef<number | null>(null);

  // Create the MotionValue for the number tween
  const displayValueMv = useMotionValue(value);

  useEffect(() => {
    const current = value;
    const prev = previousValue.current;
    const diff = current - prev;
    previousValue.current = current;

    if (diff !== 0) {
      // 1. Handle Difference Popup
      if (diffTimeout.current) clearTimeout(diffTimeout.current);
      setDisplayedDiff(diff);
      setShowDiff(true);
      diffTimeout.current = globalThis.setTimeout(
        () => setShowDiff(false),
        3000,
      );

      // 2. Trigger Pulse Direction
      setDirection(diff > 0 ? "up" : "down");
      // Reset direction state after animation duration
      setTimeout(() => setDirection("none"), 1500);

      // 3. Animate the display value (Svelte's Tween equivalent)
      animate(displayValueMv, current, {
        duration: 1,
        ease: customEase,
      });
    }
  }, [value]);

  // Animation variants for the pulse effect
  const pulseVariants = {
    up: {
      scale: [1, 1.1, 1],
      color: ["#fff", "#22c55e", "#fff"], // Adjust colors to match your theme
      transition: { duration: 0.5 },
    },
    down: {
      scale: [1, 1.1, 1],
      color: ["#fff", "#ef4444", "#fff"],
      transition: { duration: 0.5 },
    },
    none: { scale: 1, color: "inherit" },
  };

  return (
    <div className="relative inline-block">
      {/* Main Animated Number */}
      <motion.p
        className="text-lg font-bold"
        animate={direction}
        variants={pulseVariants}
      >
        <AnimatedNumber mv={displayValueMv} />
      </motion.p>

      {/* Diff Popup (Svelte in:fly/out:fade equivalent) */}
      <AnimatePresence>
        {showDiff && (
          <motion.div
            key="diff"
            className="absolute top-0 -right-10 whitespace-nowrap"
            initial={{ y: -20, opacity: 0 }}
            animate={{ y: -30, opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{
              initial: { duration: 0.3 },
              exit: { duration: 1 },
            }}
          >
            <span
              className={displayedDiff >= 0 ? "text-green-500" : "text-red-500"}
            >
              {displayedDiff >= 0 ? "+" : "-"}
              {formatValue(Math.abs(displayedDiff))}
            </span>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
