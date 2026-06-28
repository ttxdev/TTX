use serde::{Deserialize, Serialize};

#[derive(Debug, Clone, Copy, PartialEq, Eq, Default, Serialize, Deserialize, utoipa::ToSchema)]
pub enum OrderDirection {
    #[default]
    Ascending,
    Descending,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize, utoipa::ToSchema)]
pub struct Order<T> {
    pub by: T,
    pub dir: OrderDirection,
}

#[derive(Debug, PartialEq, Eq, Deserialize)]
pub struct PaginatedRequest<TOrder> {
    #[serde(default = "default_page")]
    pub page: i64,
    #[serde(default = "default_limit")]
    pub limit: i64,
    #[serde(default)]
    pub search: Option<String>,
    #[serde(default = "none::<TOrder>")]
    pub order: Option<Order<TOrder>>,
}

fn default_page() -> i64 {
    1
}

fn default_limit() -> i64 {
    10
}

fn none<T>() -> Option<Order<T>> {
    None
}

impl<TOrder> Default for PaginatedRequest<TOrder> {
    fn default() -> Self {
        Self {
            page: 1,
            limit: 10,
            search: None,
            order: None,
        }
    }
}

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct PaginationDto<T> {
    pub data: Vec<T>,
    pub total: i64,
}
