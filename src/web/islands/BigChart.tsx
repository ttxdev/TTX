import { Chart } from "@/lib/chart-zoom.ts";
import type { ChartArea, ScriptableContext, TooltipItem } from "chart.js";
import { formatToChart, formatValue } from "@/lib/formatting.ts";
import { useSignalRef } from "@preact/signals/utils";
import { useEffect, useMemo, useRef } from "preact/hooks";

type Point = { time: Date | string; value: number };

const UP = "#22c55e";
const DOWN = "#ef4444";
const RGB: Record<string, string> = {
  [UP]: "34, 197, 94",
  [DOWN]: "239, 68, 68",
};

function trendColor(values: (number | null)[]): string {
  const nums = values.filter((v): v is number => typeof v === "number");
  if (nums.length < 2) return UP;
  return nums[nums.length - 1] >= nums[0] ? UP : DOWN;
}

function areaGradient(
  ctx: CanvasRenderingContext2D,
  area: ChartArea,
  color: string,
): CanvasGradient {
  const rgb = RGB[color];
  const gradient = ctx.createLinearGradient(0, area.top, 0, area.bottom);
  gradient.addColorStop(0, `rgba(${rgb}, 0.28)`);
  gradient.addColorStop(0.85, `rgba(${rgb}, 0.02)`);
  gradient.addColorStop(1, `rgba(${rgb}, 0)`);
  return gradient;
}

export default function BigChart(
  { value, history }: { value: number; history: Point[] },
) {
  const canvas = useSignalRef<HTMLCanvasElement | null>(null);
  const chart = useRef<Chart | null>(null);
  const data = useMemo(() => formatToChart(value, history), [history]);

  useEffect(() => {
    if (!chart.current) return;
    chart.current.data.labels = data.labels;
    chart.current.data.datasets[0].data = data.values;
    chart.current.update("none");
  }, [chart, history]);

  useEffect(() => {
    if (!canvas.current) return;

    chart.current = new Chart(canvas.current, {
      type: "line",
      data: {
        labels: data.labels,
        datasets: [
          {
            label: "Value",
            data: data.values,
            borderColor: (ctx: ScriptableContext<"line">) =>
              trendColor(ctx.chart.data.datasets[0].data as (number | null)[]),
            backgroundColor: (ctx: ScriptableContext<"line">) => {
              const { chartArea } = ctx.chart;
              if (!chartArea) return "transparent";
              const color = trendColor(
                ctx.chart.data.datasets[0].data as (number | null)[],
              );
              return areaGradient(ctx.chart.ctx, chartArea, color);
            },
            fill: true,
            tension: 0.25,
            borderWidth: 2,
            pointRadius: 0,
            pointHoverRadius: 5,
            pointHoverBorderColor: "#fff",
            pointHoverBorderWidth: 2,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        interaction: { mode: "index", intersect: false },
        plugins: {
          legend: { display: false },
          tooltip: {
            displayColors: false,
            padding: 10,
            cornerRadius: 8,
            callbacks: {
              title: (items: TooltipItem<"line">[]) =>
                new Date(items[0].label).toLocaleString(),
              label: (item: TooltipItem<"line">) =>
                `Value: ${formatValue(item.parsed.y ?? 0)}`,
            },
          },
          zoom: {
            zoom: {
              wheel: { enabled: false },
              pinch: { enabled: true },
              drag: {
                enabled: true,
                modifierKey: "ctrl",
                backgroundColor: "rgba(124, 58, 237, 0.15)",
              },
              mode: "x",
            },
            pan: { enabled: true, mode: "x" },
          },
        },
        scales: {
          x: {
            ticks: { display: false },
            grid: { display: false },
            border: { display: false },
          },
          y: {
            beginAtZero: false,
            grid: { color: "rgba(128, 128, 128, 0.1)" },
            border: { display: false },
            ticks: {
              maxTicksLimit: 5,
              callback: (tickValue: string | number) =>
                formatValue(Number(tickValue)),
            },
          },
        },
      },
    });

    return () => {
      chart.current?.destroy();
      chart.current = null;
    };
  }, [chart]);

  return <canvas ref={canvas}></canvas>;
}
