// deno-lint-ignore-file no-explicit-any
import { Chart } from "chart.js";
import { useEffect, useRef, useState } from "preact/hooks";
import { VoteDto } from "../../../lib/api.ts";
import { formatValue } from "../../../lib/formatting.ts";

export default function BigChart({ history }: { history: VoteDto[] }) {
  const canvas = useRef<HTMLCanvasElement | null>(null);
  const [chart, setChart] = useState<Chart | null>(null);

  useEffect(() => {
    if (chart) {
      chart.data.labels = history.map((d) => d.time);
      chart.data.datasets[0].data = history.map((d) => d.value);
      chart.update();
    }
  }, [chart, history]);

  useEffect(() => {
    if (!canvas.current) {
      return;
    }

    const chart = new Chart(canvas.current, {
      type: "line",
      data: {
        labels: history.map((d) => d.time),
        datasets: [
          {
            label: "Price",
            data: history.map((d) => d.value),
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

    setChart(chart);
    return () => {
      setChart(null);
      chart.destroy();
    };
  }, [chart, canvas]);

  return <canvas ref={canvas}></canvas>;
}
