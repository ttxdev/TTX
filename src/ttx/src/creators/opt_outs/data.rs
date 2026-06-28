//! CreatorOptOut persistence. Additional `impl Db` block over the shared pool.

use super::model::CreatorOptOut;
use crate::data::{Db, platform_to_int};
use crate::error::Result;
use crate::platforms::Platform;
use crate::primitives::Id;

impl Db {
    pub async fn creator_opt_out_exists(
        &self,
        platform: Platform,
        platform_id: &str,
    ) -> Result<bool> {
        let exists: bool = sqlx::query_scalar(
            "SELECT EXISTS(SELECT 1 FROM creator_opt_outs WHERE \"Platform\" = $1 AND platform_id = $2)",
        )
        .bind(platform_to_int(platform))
        .bind(platform_id)
        .fetch_one(&self.pool)
        .await?;
        Ok(exists)
    }

    pub async fn insert_creator_opt_out(&self, opt: &mut CreatorOptOut) -> Result<()> {
        let id: i32 = sqlx::query_scalar(
            "INSERT INTO creator_opt_outs (platform_id, \"Platform\", \"Reason\", created_at, updated_at) \
             VALUES ($1,$2,$3,$4,$5) RETURNING id",
        )
        .bind(opt.platform_id.to_string())
        .bind(platform_to_int(opt.platform))
        .bind(&opt.reason)
        .bind(opt.meta.created_at)
        .bind(opt.meta.updated_at)
        .fetch_one(&self.pool)
        .await?;
        opt.meta.id = id as Id;
        Ok(())
    }
}
