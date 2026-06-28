//! CreatorApplication persistence. Additional `impl Db` block over the shared pool.

use super::model::{CreatorApplication, CreatorApplicationStatus};
use crate::data::{Db, platform_to_int};
use crate::error::Result;
use crate::primitives::Id;

fn status_to_str(s: CreatorApplicationStatus) -> &'static str {
    match s {
        CreatorApplicationStatus::Pending => "Pending",
        CreatorApplicationStatus::Approved => "Approved",
        CreatorApplicationStatus::Rejected => "Rejected",
    }
}

#[allow(dead_code)]
fn parse_status(s: &str) -> CreatorApplicationStatus {
    match s {
        "Approved" => CreatorApplicationStatus::Approved,
        "Rejected" => CreatorApplicationStatus::Rejected,
        _ => CreatorApplicationStatus::Pending,
    }
}

impl Db {
    pub async fn insert_creator_application(&self, app: &mut CreatorApplication) -> Result<()> {
        let id: i32 = sqlx::query_scalar(
            "INSERT INTO creator_applications \
             (submitter_id, \"Platform\", platform_id, ticker, name, status, created_at, updated_at) \
             VALUES ($1,$2,$3,$4,$5,$6,$7,$8) RETURNING id",
        )
        .bind(app.submitter_id as i32)
        .bind(platform_to_int(app.platform))
        .bind(app.platform_id.to_string())
        .bind(&app.ticker)
        .bind(&app.name)
        .bind(status_to_str(app.status))
        .bind(app.meta.created_at)
        .bind(app.meta.updated_at)
        .fetch_one(&self.pool)
        .await?;
        app.meta.id = id as Id;
        Ok(())
    }
}
