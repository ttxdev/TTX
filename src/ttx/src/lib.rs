// Shared infrastructure.
pub mod cache;
pub mod data;
pub mod di;
pub mod dto;
pub mod error;
pub mod events;
pub mod factories;
pub mod jobs;
pub mod options;
pub mod platforms;
pub mod primitives;
pub mod validators;

// Leaf domain modules (no service / repo of their own).
pub mod shares;
pub mod users;

// Feature modules: each `<name>.rs` carries the service (when one exists) and
// declares its `model` / `data` / `dto` / `events` submodules. Creator
// applications and opt-outs live as nested submodules under `creators`.
pub mod creators;
pub mod lootboxes;
pub mod players;
pub mod transactions;
