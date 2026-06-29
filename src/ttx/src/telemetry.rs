//! Shared OpenTelemetry + `tracing` initialisation for the TTX binaries.
//!
//! Every binary calls [`init`] once at startup with its service name. The
//! console `fmt` layer (driven by `RUST_LOG`/`EnvFilter`) is always installed,
//! so logging behaves exactly as before. When `OTEL_EXPORTER_OTLP_ENDPOINT` is
//! set, spans are additionally exported over OTLP/HTTP to that collector;
//! when it is unset the process runs with console logging only, so local
//! development needs no collector running.
//!
//! The returned [`OtelGuard`] must be held for the whole process lifetime: on
//! drop it flushes and shuts down the tracer provider so buffered spans are not
//! lost on exit.

use opentelemetry::trace::TracerProvider as _;
use opentelemetry_sdk::Resource;
use opentelemetry_sdk::propagation::TraceContextPropagator;
use opentelemetry_sdk::trace::SdkTracerProvider;
use tracing_subscriber::EnvFilter;
use tracing_subscriber::layer::SubscriberExt;
use tracing_subscriber::util::SubscriberInitExt;

/// Flush-on-drop guard for the OpenTelemetry pipeline. Keep it alive for as
/// long as the process should emit telemetry; dropping it shuts export down.
#[must_use = "dropping the guard immediately tears down trace export"]
pub struct OtelGuard {
    provider: Option<SdkTracerProvider>,
}

impl Drop for OtelGuard {
    fn drop(&mut self) {
        if let Some(provider) = self.provider.take()
            && let Err(err) = provider.shutdown()
        {
            eprintln!("opentelemetry: error shutting down tracer provider: {err}");
        }
    }
}

/// Initialise logging and tracing for a binary. `service_name` identifies this
/// process in the telemetry backend (e.g. `"ttx-api"`, `"ttx-jobs"`).
///
/// Installs the global `tracing` subscriber, so it must be called exactly once.
pub fn init(service_name: &str) -> OtelGuard {
    let filter = EnvFilter::try_from_default_env().unwrap_or_else(|_| EnvFilter::new("info"));
    let fmt_layer = tracing_subscriber::fmt::layer();

    let provider = build_provider(service_name);
    // `Option<Layer>` is itself a `Layer`, so this is a no-op when export is off.
    let otel_layer = provider
        .as_ref()
        .map(|provider| tracing_opentelemetry::layer().with_tracer(provider.tracer("ttx")));

    tracing_subscriber::registry()
        .with(filter)
        .with(fmt_layer)
        .with(otel_layer)
        .init();

    if provider.is_some() {
        opentelemetry::global::set_text_map_propagator(TraceContextPropagator::new());
        tracing::info!(service_name, "OpenTelemetry OTLP export enabled");
    } else {
        tracing::info!("OpenTelemetry export disabled: set OTEL_EXPORTER_OTLP_ENDPOINT to enable");
    }

    OtelGuard { provider }
}

/// Build the OTLP tracer provider and register it globally, or return `None`
/// when no endpoint is configured (local dev) or the exporter fails to build.
///
/// The HTTP exporter reads its endpoint and headers from the standard
/// `OTEL_EXPORTER_OTLP_*` environment variables.
fn build_provider(service_name: &str) -> Option<SdkTracerProvider> {
    // No endpoint configured (e.g. local dev, or an empty value injected by a
    // deployment template) -> run with console logging only.
    let endpoint = std::env::var("OTEL_EXPORTER_OTLP_ENDPOINT").unwrap_or_default();
    if endpoint.trim().is_empty() {
        return None;
    }

    let exporter = opentelemetry_otlp::SpanExporter::builder()
        .with_http()
        .build()
        .inspect_err(|err| {
            eprintln!("opentelemetry: failed to build OTLP exporter, export disabled: {err}");
        })
        .ok()?;

    let resource = Resource::builder()
        .with_service_name(service_name.to_owned())
        .build();

    let provider = SdkTracerProvider::builder()
        .with_batch_exporter(exporter)
        .with_resource(resource)
        .build();

    opentelemetry::global::set_tracer_provider(provider.clone());
    Some(provider)
}
