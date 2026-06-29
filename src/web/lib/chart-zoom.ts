import type { Plugin } from "chart.js";
import { Chart } from "./chart.ts";

if (typeof window !== "undefined") {
  const zoomPlugin = (await import("chartjs-plugin-zoom")).default;
  Chart.register(zoomPlugin as unknown as Plugin);
}

export { Chart };
