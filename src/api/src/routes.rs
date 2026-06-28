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
        .route("/hubs/events", get(ws::events))
        .route("/hubs/votes", get(ws::votes))
        .route("/hubs/portfolios", get(ws::portfolios))
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

        assert!(json["components"]["securitySchemes"]["bearer_auth"].is_object());
    }

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

    #[test]
    fn object_schemas_are_closed() {
        let json = serde_json::to_value(ApiDoc::openapi()).expect("doc serializes");
        let schemas = json["components"]["schemas"]
            .as_object()
            .expect("schemas object");

        assert_eq!(
            schemas["CreatorPartialDto"]["additionalProperties"],
            serde_json::json!(false),
            "CreatorPartialDto must be closed"
        );

        let creator_all_of = schemas["CreatorDto"]["allOf"].as_array().expect("allOf");
        assert!(
            creator_all_of
                .iter()
                .any(|part| part["additionalProperties"] == serde_json::json!(false)),
            "CreatorDto's inline allOf object must be closed"
        );
    }

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
