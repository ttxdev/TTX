use chrono::Duration;
use serde::{Deserialize, Serialize};

#[derive(Debug, PartialEq, Eq, Serialize, Deserialize, utoipa::ToSchema)]
pub enum TimeStep {
    Minute,
    FiveMinute,
    FifteenMinute,
    ThirtyMinute,
    Hour,
    Day,
    Week,
    Month,
}

impl TimeStep {
    pub fn to_timescale_string<'a>(&self) -> &'a str {
        match self {
            TimeStep::Minute => "1 minute",
            TimeStep::FiveMinute => "5 minute",
            TimeStep::FifteenMinute => "15 minute",
            TimeStep::ThirtyMinute => "30 minute",
            TimeStep::Hour => "1 hour",
            TimeStep::Day => "1 day",
            TimeStep::Week => "1 week",
            TimeStep::Month => "1 month",
        }
    }
}

#[derive(Debug, PartialEq)]
pub struct HistoryParams {
    pub step: TimeStep,
    pub before: Duration,
}
