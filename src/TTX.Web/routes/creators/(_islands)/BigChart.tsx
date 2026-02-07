// deno-lint-ignore-file no-explicit-any
import { Chart } from "chart.js";
import { VoteDto } from "@/lib/api.ts";
import { formatToChart, formatValue } from "@/lib/formatting.ts";
import { useSignalRef } from "@preact/signals/utils";
import { useEffect, useMemo, useRef } from "preact/hooks";

export default function BigChart(
  { value, history }: { value: number; history: VoteDto[] },
) {
  const canvas = useSignalRef<HTMLCanvasElement | null>(null);
  const data = useMemo(() => formatToChart(value, history), [history]);
  const chart = useRef<Chart | null>(null);

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
            label: "Price",
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
              title: (tooltipItems: { label: string | number | Date }[]) => {
                const date = new Date(tooltipItems[0].label);
                return date.toLocaleString();
              },
              label: (tooltipItem: { parsed: { y: any } }) => {
                return `Value: ${formatValue(tooltipItem.parsed.y)}`;
              },
              labelColor: (
                tooltipItem: {
                  parsed: { y: any };
                  dataIndex: number;
                  dataset: { data: any[] };
                },
              ) => {
                const currentValue = tooltipItem.parsed.y;
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
              callback: function (value: string | number, _) {
                return formatValue(Number(value));
              },
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
