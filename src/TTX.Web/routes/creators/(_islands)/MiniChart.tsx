import { Chart } from "chart.js";
import type { VoteDto } from "@/lib/api.ts";
import { useSignalEffect } from "@preact/signals";
import { useSignalRef } from "@preact/signals/utils";
import { formatToChart } from "../../../lib/formatting.ts";

export default function MiniChart(
  { value, history }: { value: number; history: VoteDto[] },
) {
  const canvas = useSignalRef<HTMLCanvasElement | null>(null);
  const data = formatToChart(value, history);
  const isUpward = data.values[data.values.length - 1] > data.values[0];
  const lineColor = isUpward ? "#22c55e" : "#ef4444";

  useSignalEffect(() => {
    if (!canvas.current) {
      return;
    }

    const chart = new Chart(canvas.current, {
      type: "line",
      data: {
        labels: Array(data.labels.length).fill(""), // Create empty labels
        datasets: [
          {
            data: data.values,
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
  });

  return <canvas ref={canvas}></canvas>;
}
