mod creators;
mod players;
mod sessions;
mod transactions;
mod ws;

use axum::Router;
use axum::http::{HeaderValue, Method, header};
use axum::routing::get;
use chrono::Duration;
use tower_http::cors::{AllowOrigin, CorsLayer};
use tower_http::trace::TraceLayer;
use ttx::dto::portfolio::{HistoryParams, TimeStep};
use utoipa::openapi::RefOr;
use utoipa::openapi::schema::{AdditionalProperties, Schema};
use utoipa::openapi::security::{HttpAuthScheme, HttpBuilder, SecurityScheme};
use utoipa::{Modify, OpenApi};
use utoipa_swagger_ui::SwaggerUi;

use crate::state::AppState;

/// OpenAPI document. Replaces the C# Swashbuckle setup in `AddTtxApi`.
#[derive(OpenApi)]
#[openapi(
    info(
        title = "TTX API",
        version = "0.1.0",
        description = "Twitch stock-exchange game API (ported from TTX.Api).",
    ),
    paths(
        creators::index,
        creators::show,
        creators::create,
        creators::opt_out,
        players::index,
        players::show,
        players::get_me,
        players::gamba,
        transactions::create,
        sessions::login,
        sessions::twitch_callback,
    ),
    // Enums referenced only by `IntoParams` query parameters aren't picked up by
    // utoipa's automatic path-schema collection, so register them explicitly —
    // otherwise their `$ref`s dangle and strict consumers (NSwag) fail to resolve.
    components(schemas(
        ttx::creators::CreatorOrderBy,
        ttx::players::PlayerOrderBy,
        ttx::dto::pagination::OrderDirection,
    )),
    modifiers(&SecurityAddon, &CloseSchemas),
    tags(
        (name = "creators", description = "Creator listings, onboarding, and opt-outs"),
        (name = "players", description = "Player listings, profile, and lootboxes"),
        (name = "transactions", description = "Placing buy/sell orders"),
        (name = "sessions", description = "Login and OAuth callback"),
    ),
)]
struct ApiDoc;

/// Registers the `bearer_auth` (JWT) security scheme used by `[Authorize]` routes.
struct SecurityAddon;

impl Modify for SecurityAddon {
    fn modify(&self, openapi: &mut utoipa::openapi::OpenApi) {
        if let Some(components) = openapi.components.as_mut() {
            components.add_security_scheme(
                "bearer_auth",
                SecurityScheme::Http(
                    HttpBuilder::new()
                        .scheme(HttpAuthScheme::Bearer)
                        .bearer_format("JWT")
                        .build(),
                ),
            );
        }
    }
}

/// Marks every object schema as closed (`additionalProperties: false`).
///
/// Without this, strict clients (NSwag) treat the objects as open and emit a
/// generic property-copy loop in each generated class. In a *derived* class
/// that loop runs after `super.init()` and re-copies inherited fields as their
/// raw JSON — clobbering parsed `Date`s back into strings, which then blows up
/// in `toJSON()` (`created_at.toISOString is not a function`).
struct CloseSchemas;

impl Modify for CloseSchemas {
    fn modify(&self, openapi: &mut utoipa::openapi::OpenApi) {
        if let Some(components) = openapi.components.as_mut() {
            for schema in components.schemas.values_mut() {
                close_schema(schema);
            }
        }
    }
}

fn close_schema(schema: &mut RefOr<Schema>) {
    if let RefOr::T(schema) = schema {
        match schema {
            Schema::Object(object) => {
                object.additional_properties =
                    Some(Box::new(AdditionalProperties::FreeForm(false)));
            }
            // `#[serde(flatten)]` DTOs become an allOf of a $ref + an inline
            // object; close the inline parts too.
            Schema::AllOf(all_of) => all_of.items.iter_mut().for_each(close_schema),
            Schema::AnyOf(any_of) => any_of.items.iter_mut().for_each(close_schema),
            Schema::OneOf(one_of) => one_of.items.iter_mut().for_each(close_schema),
            _ => {}
        }
    }
}

pub(crate) fn history_params(before: Duration) -> HistoryParams {
    let step = if before.num_days() > 30 {
        TimeStep::Month
    } else if before.num_days() > 7 {
        TimeStep::Week
    } else if before.num_days() > 1 {
        TimeStep::Day
    } else if before.num_hours() > 1 {
        TimeStep::Hour
    } else {
        TimeStep::Minute
    };
    HistoryParams { step, before }
}

pub fn router(state: AppState) -> Router {
    let cors = CorsLayer::new()
        .allow_origin(AllowOrigin::list([
            HeaderValue::from_static("https://ttx.gg"),
            HeaderValue::from_static("http://localhost:5173"),
            HeaderValue::from_static("http://127.0.0.1:5173"),
        ]))
        .allow_methods([Method::GET, Method::POST, Method::PUT, Method::DELETE])
        .allow_headers([header::AUTHORIZATION, header::CONTENT_TYPE])
        .allow_credentials(true);

    Router::new()
        .route("/health", get(|| async { "ok" }))
        .nest("/creators", creators::router())
        .nest("/players", players::router())
        .nest("/transactions", transactions::router())
        .nest("/sessions", sessions::router())
        // Websocket hubs (replacing SignalR's events/votes/portfolios hubs).
        .route("/hubs/events", get(ws::events))
        .route("/hubs/votes", get(ws::votes))
        .route("/hubs/portfolios", get(ws::portfolios))
        // Swagger UI at /swagger-ui, raw spec at /api-docs/openapi.json.
        .merge(SwaggerUi::new("/swagger-ui").url("/api-docs/openapi.json", ApiDoc::openapi()))
        .layer(cors)
        .layer(TraceLayer::new_for_http())
        .with_state(state)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn openapi_spec_is_complete() {
        let doc = ApiDoc::openapi();
        let json = serde_json::to_value(&doc).expect("doc serializes");

        let paths = json["paths"].as_object().expect("paths object");
        for path in [
            "/creators",
            "/creators/{slug}",
            "/players",
            "/players/me",
            "/players/me/lootboxes/{lootbox_id}/open",
            "/transactions",
            "/sessions/login",
            "/sessions/twitch/callback",
        ] {
            assert!(paths.contains_key(path), "missing path {path}");
        }

        // DTO body schemas (auto-collected) and the query-param enums (registered
        // explicitly) must all be present.
        let schemas = json["components"]["schemas"]
            .as_object()
            .expect("schemas object");
        for schema in [
            "CreatorDto",
            "PlayerDto",
            "CreatorOptOutDto",
            "LootBoxResultDto",
            "TokenDto",
            "CreatorOrderBy",
            "PlayerOrderBy",
            "OrderDirection",
        ] {
            assert!(schemas.contains_key(schema), "missing schema {schema}");
        }

        // The JWT security scheme is registered by the modifier.
        assert!(json["components"]["securitySchemes"]["bearer_auth"].is_object());
    }

    /// Every `#/components/schemas/X` reference must resolve, or strict clients
    /// (e.g. NSwag) fail to generate. Walks the whole document.
    #[test]
    fn openapi_has_no_dangling_schema_refs() {
        let json = serde_json::to_value(ApiDoc::openapi()).expect("doc serializes");
        let schemas = json["components"]["schemas"]
            .as_object()
            .expect("schemas object");

        let mut refs = Vec::new();
        collect_refs(&json, &mut refs);

        let prefix = "#/components/schemas/";
        for r in refs {
            if let Some(name) = r.strip_prefix(prefix) {
                assert!(schemas.contains_key(name), "dangling schema reference: {r}");
            }
        }
    }

    /// The bucket size must be fine enough that short windows return a series,
    /// not a single point (the 1h window must use minute buckets).
    #[test]
    fn history_params_bucket_sizes() {
        use chrono::Duration;
        use ttx::dto::portfolio::TimeStep;

        assert_eq!(history_params(Duration::hours(1)).step, TimeStep::Minute);
        assert_eq!(history_params(Duration::hours(6)).step, TimeStep::Hour);
        assert_eq!(history_params(Duration::hours(24)).step, TimeStep::Hour);
        assert_eq!(history_params(Duration::days(3)).step, TimeStep::Day);
        assert_eq!(history_params(Duration::days(10)).step, TimeStep::Week);
        assert_eq!(history_params(Duration::days(40)).step, TimeStep::Month);
    }

    /// Object schemas must be closed so NSwag doesn't emit the generic
    /// property-copy loop that clobbers parsed dates in derived DTOs.
    #[test]
    fn object_schemas_are_closed() {
        let json = serde_json::to_value(ApiDoc::openapi()).expect("doc serializes");
        let schemas = json["components"]["schemas"]
            .as_object()
            .expect("schemas object");

        // A plain object DTO.
        assert_eq!(
            schemas["CreatorPartialDto"]["additionalProperties"],
            serde_json::json!(false),
            "CreatorPartialDto must be closed"
        );

        // A flattened DTO: allOf of [$ref, inline object]; the inline object closed.
        let creator_all_of = schemas["CreatorDto"]["allOf"].as_array().expect("allOf");
        assert!(
            creator_all_of
                .iter()
                .any(|part| part["additionalProperties"] == serde_json::json!(false)),
            "CreatorDto's inline allOf object must be closed"
        );
    }

    /// Client codegen (NSwag) derives method names from `operationId`s, which
    /// must be unique across the whole document and match the frontend's calls.
    #[test]
    fn operation_ids_are_unique_and_stable() {
        let json = serde_json::to_value(ApiDoc::openapi()).expect("doc serializes");
        let paths = json["paths"].as_object().expect("paths object");

        let mut ids = Vec::new();
        for methods in paths.values() {
            for op in methods.as_object().into_iter().flatten().map(|(_, op)| op) {
                if let Some(id) = op.get("operationId").and_then(|v| v.as_str()) {
                    ids.push(id.to_string());
                }
            }
        }

        let mut unique = ids.clone();
        unique.sort();
        unique.dedup();
        assert_eq!(ids.len(), unique.len(), "duplicate operationId(s): {ids:?}");

        for expected in [
            "GetCreators",
            "GetCreator",
            "CreateCreator",
            "OptOutCreator",
            "GetPlayers",
            "GetPlayer",
            "GetSelf",
            "Gamba",
            "PlaceOrder",
            "GetLoginUrl",
            "TwitchCallback",
        ] {
            assert!(
                ids.iter().any(|id| id == expected),
                "missing operationId {expected}"
            );
        }
    }

    fn collect_refs(value: &serde_json::Value, out: &mut Vec<String>) {
        match value {
            serde_json::Value::Object(map) => {
                for (key, val) in map {
                    if key == "$ref" {
                        if let Some(s) = val.as_str() {
                            out.push(s.to_string());
                        }
                    } else {
                        collect_refs(val, out);
                    }
                }
            }
            serde_json::Value::Array(items) => {
                for item in items {
                    collect_refs(item, out);
                }
            }
            _ => {}
        }
    }
}
