// Import CSS files here for hot module reloading to work.
import "./assets/styles.css";

// NOTE: Chart.js is intentionally NOT registered here. It used to be imported
// globally, which pulled the entire (~200KB) chart bundle into the client entry
// loaded on every page. The chart islands now import it from `lib/chart.ts` /
// `lib/chart-zoom.ts`, so it loads lazily only on pages that render a chart.
