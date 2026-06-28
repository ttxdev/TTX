//! Creator applications feature module.
//!
//! No service exists for applications in the App layer (the C# original only
//! defined the model, DTO, and events), so this root just aggregates the
//! submodules.

pub mod data;
pub mod dto;
pub mod events;
pub mod model;

pub use model::{CreatorApplication, CreatorApplicationStatus};
