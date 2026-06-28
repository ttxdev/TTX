// Shared Chart.js setup for the line charts used across the app.
//
// Registering only the components the charts actually use (a line chart on
// linear/category scales with tooltips) instead of `...registerables` keeps the
// chart bundle small. Importing this from the chart islands — rather than the
// global `client.ts` entry — means Chart.js loads lazily, only on pages that
// render a chart, instead of on every page.
import {
  CategoryScale,
  Chart,
  Filler,
  LinearScale,
  LineController,
  LineElement,
  PointElement,
  Tooltip,
} from "chart.js";

Chart.register(
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Filler,
  Tooltip,
);

export { Chart };
