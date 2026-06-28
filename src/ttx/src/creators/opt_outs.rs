//! Creator opt-outs feature module.
//!
//! No standalone service exists (opt-outs are created via `CreatorService`),
//! so this root aggregates the submodules.

pub mod data;
pub mod dto;
pub mod model;

pub use model::CreatorOptOut;
