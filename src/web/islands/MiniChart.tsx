import { Chart } from "@/lib/chart.ts";
import type { ChartArea, ScriptableContext } from "chart.js";
import { useSignalEffect } from "@preact/signals";
import { useSignalRef } from "@preact/signals/utils";
import { formatToChart } from "@/lib/formatting.ts";

/** A point on the chart — both VoteDto and PortfolioSnapshotDto satisfy this. */
type Point = { time: Date | string; value: number };

const UP = "#22c55e";
const DOWN = "#ef4444";
const RGB: Record<string, string> = {
  [UP]: "34, 197, 94",
  [DOWN]: "239, 68, 68",
};

/** Vertical fade from the trend color down to transparent, for the area fill. */
function areaGradient(
  ctx: CanvasRenderingContext2D,
  area: ChartArea,
  color: string,
): CanvasGradient {
  const rgb = RGB[color];
  const gradient = ctx.createLinearGradient(0, area.top, 0, area.bottom);
  gradient.addColorStop(0, `rgba(${rgb}, 0.22)`);
  gradient.addColorStop(1, `rgba(${rgb}, 0)`);
  return gradient;
}

export default function MiniChart(
  { value, history }: { value: number; history: Point[] },
) {
  const canvas = useSignalRef<HTMLCanvasElement | null>(null);
  const data = formatToChart(value, history);
  const isUp = data.values.length < 2 ||
    data.values[data.values.length - 1] >= data.values[0];
  const lineColor = isUp ? UP : DOWN;

  useSignalEffect(() => {
    if (!canvas.current) {
      return;
    }

    const chart = new Chart(canvas.current, {
      type: "line",
      data: {
        labels: Array(data.labels.length).fill(""),
        datasets: [
          {
            data: data.values,
            borderColor: lineColor,
            backgroundColor: (ctx: ScriptableContext<"line">) => {
              const { chartArea } = ctx.chart;
              if (!chartArea) return "transparent";
              return areaGradient(ctx.chart.ctx, chartArea, lineColor);
            },
            borderWidth: 2,
            fill: true,
            tension: 0.3,
            pointRadius: 0,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: false,
        plugins: {
          legend: { display: false },
          tooltip: { enabled: false },
        },
        scales: {
          x: { display: false },
          y: { display: false },
        },
      },
    });

    return () => {
      chart.destroy();
    };
  });

  return <canvas ref={canvas}></canvas>;
}
