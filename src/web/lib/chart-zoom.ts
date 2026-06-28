// Chart.js with the zoom/pan plugin, for the interactive "big" charts only.
//
// Kept separate from `lib/chart.ts` so the zoom plugin is bundled into its own
// chunk and only the detail-page charts that need wheel/pinch/drag zoom pull it
// in — the mini and featured charts don't.
import type { Plugin } from "chart.js";
import { Chart } from "./chart.ts";

// chartjs-plugin-zoom pulls in hammerjs, which reads `window` at import time and
// throws during SSR. Import and register it on the client only — the charts
// that use zoom create their Chart instances in client-side effects anyway, so
// the plugin is always registered before it's needed.
if (typeof window !== "undefined") {
  const zoomPlugin = (await import("chartjs-plugin-zoom")).default;
  // The default export isn't typed as a `ChartComponentLike`, so register it
  // through the `Plugin` type it actually satisfies.
  Chart.register(zoomPlugin as unknown as Plugin);
}

export { Chart };
