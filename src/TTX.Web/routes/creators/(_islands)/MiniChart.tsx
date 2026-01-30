import { Chart } from "chart.js";
import type { VoteDto } from "@/lib/api.ts";
import { useEffect, useRef } from "preact/hooks";

export default function MiniChart(
  { value, history }: { value: number; history: VoteDto[] },
) {
  const canvas = useRef<HTMLCanvasElement | null>(null);
  const values = history.map((v) => v.value);
  const isUpward = values[values.length - 1] > values[0];
  const lineColor = isUpward ? "#22c55e" : "#ef4444";

  if (values.length === 0) {
    values.push(value);
  }

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

  return <canvas ref={canvas}></canvas>;
}
