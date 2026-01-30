// Import CSS files here for hot module reloading to work.
import "./assets/styles.css";
import { default as zoomPlugin } from "chartjs-plugin-zoom";
import { Chart, registerables } from "chart.js";

Chart.register(zoomPlugin, ...registerables);
