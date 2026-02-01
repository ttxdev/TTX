// deno-lint-ignore-file no-explicit-any
import { PortfolioSnapshotDto } from "@/lib/api.ts";
import { formatToChart, formatValue } from "@/lib/formatting.ts";
import { useSignalRef } from "@preact/signals/utils";
import { Chart } from "chart.js";
import { useEffect, useMemo, useRef } from "preact/hooks";

export default function BigChart(
  { value, history }: {
    value: number;
    history: PortfolioSnapshotDto[];
  },
) {
  const canvas = useSignalRef<HTMLCanvasElement | null>(null);
  const chart = useRef<Chart | null>(null);
  const data = useMemo(() => formatToChart(value, history), [history]);

  useEffect(() => {
    if (!chart.current) {
      return;
    }

    chart.current.data.labels = data.labels;
    chart.current.data.datasets[0].data = data.values;
    chart.current.update();
  }, [chart, history]);

  useEffect(() => {
    if (!canvas.current) {
      return;
    }

    chart.current = new Chart(canvas.current, {
      type: "line",
      data: {
        labels: data.labels,
        datasets: [
          {
            label: "Value",
            data: data.values,
            segment: {
              borderColor: (ctx) => {
                const difference = ctx.p0DataIndex > 0
                  ? Number(ctx.p0.parsed.y) - Number(ctx.p1.parsed.y)
                  : 0;

                return difference <= 0 ? "#22c55e" : "#ef4444";
              },
            },
            tension: 0,
            fill: false,
            borderWidth: 2,
            pointRadius: 0,
            pointHoverRadius: 4,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false,
          },
          tooltip: {
            enabled: true,
            mode: "index",
            intersect: false,
            callbacks: {
              title: (tooltipItems) => {
                const date = new Date(tooltipItems[0].label);
                return date.toLocaleString();
              },
              label: (tooltipItem) => {
                return `Value: $${
                  Number(tooltipItem.parsed.y).toLocaleString()
                }`;
              },
              labelColor: (tooltipItem) => {
                const currentValue = Number(tooltipItem.parsed.y);
                const previousIndex = tooltipItem.dataIndex > 0
                  ? tooltipItem.dataIndex - 1
                  : 0;
                const previousValue =
                  typeof tooltipItem.dataset.data[previousIndex] === "object"
                    ? (tooltipItem.dataset.data[previousIndex] as any).value
                    : Number(tooltipItem.dataset.data[previousIndex]);
                const difference = currentValue - previousValue;
                const color = difference >= 0 ? "#22c55e" : "#ef4444";

                return {
                  backgroundColor: color,
                  borderColor: color,
                };
              },
            },
          },
          zoom: {
            zoom: {
              wheel: {
                enabled: true,
              },
              pinch: {
                enabled: true,
              },
              drag: {
                enabled: true,
                modifierKey: "ctrl",
              },
              mode: "x",
            },
            pan: {
              enabled: true,
              mode: "x",
            },
          },
        },
        scales: {
          x: {
            ticks: {
              display: false,
            },
            grid: {
              display: false,
            },
            border: {
              display: false,
            },
          },
          y: {
            beginAtZero: false,
            grid: {
              display: false,
            },
            border: {
              display: false,
            },
            ticks: {
              display: true,
              callback: function (_, value: number) {
                return formatValue(value);
              },
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
