import { Chart } from "chart.js";
import type { PortfolioSnapshotDto } from "@/lib/api.ts";
import { useEffect, useRef } from "preact/hooks";

export default function MiniChart(
  { history }: { history: PortfolioSnapshotDto[] },
) {
  const canvas = useRef<HTMLCanvasElement | null>(null);
  const isUpward = history[history.length - 1]?.value > history[0]?.value;
  const lineColor = isUpward ? "#22c55e" : "#ef4444";

  useEffect(() => {
    if (!canvas.current) {
      return;
    }

    const chart = new Chart(canvas.current, {
      type: "line",
      data: {
        labels: Array(history.length).fill(""), // Create empty labels
        datasets: [
          {
            data: history.map((d) => d.value),
            borderColor: lineColor,
            borderWidth: 3,
            fill: false,
            tension: 0,
            pointRadius: 0,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: true,
        plugins: {
          legend: {
            display: false,
          },
        },
        scales: {
          x: {
            display: false,
          },
          y: {
            display: false,
          },
        },
      },
    });

    return () => {
      chart.destroy();
    };
  }, [canvas]);

  return (
    <canvas ref={canvas}>
    </canvas>
  );
}
