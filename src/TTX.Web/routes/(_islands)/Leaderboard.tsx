import { useEffect, useRef } from "preact/hooks";
import { CreatorPartialDto } from "../../lib/api.ts";
import { Chart } from "chart.js";
import { formatToChart } from "../../lib/formatting.ts";

function LeaderboardItem({ creator }: { creator: CreatorPartialDto }) {
  const canvas = useRef<HTMLCanvasElement | null>(null);
  const href = `/creators/${creator.slug}`;
  const data = formatToChart(creator.value, creator.history);
  const isUpward = data.values[data.values.length - 1] > data.values[0];
  const lineColor = isUpward ? "#22c55e" : "#ef4444"; // green-500 or red-500

  useEffect(() => {
    if (!canvas.current) {
      return;
    }

    const chart = new Chart(canvas.current, {
      type: "line",
      data: {
        labels: Array(data.labels.length).fill(""), // Create empty labels for history points
        datasets: [
          {
            data: data.values,
            borderColor: lineColor,
            borderWidth: 2,
            fill: false,
            tension: 0,
            pointRadius: 0,
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

    return () => chart.destroy();
  }, [canvas]);

  return (
    <li class="list-row flex items-center max-lg:flex-col max-md:p-4">
      <div class="flex w-1/4 flex-col items-center justify-center gap-2 max-md:w-full">
        <a href={href}>
          <img src={creator.avatar_url} alt="" class="size-16 rounded-full" />
        </a>
        <a href={href} class="font-semibold hover:underline">
          {creator.name}
        </a>
        <div class="mb-1 flex items-center justify-between"></div>
      </div>
      <div class="flex-1">
        <div class="flex-1 max-md:mt-4 max-md:w-full">
          <a href={href} class="h-[40px] w-full">
            <canvas ref={canvas}></canvas>
          </a>
        </div>
      </div>
    </li>
  );
}

export default function Leaderboard(
  { creators }: { creators: CreatorPartialDto[] },
) {
  return (
    <ul class="list rounded-box rounded-xl bg-gray-100/10 shadow-md">
      {creators.map((creator) => (
        <LeaderboardItem key={`leaderboard-${creator.id}`} creator={creator} />
      ))}
    </ul>
  );
}
