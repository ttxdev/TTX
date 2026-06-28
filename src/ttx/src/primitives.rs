use chrono::{DateTime, Utc};

pub type Credits = f64;
pub type Quantity = u64;
pub type Id = u64;
pub type Timestamp = DateTime<Utc>;

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub struct Meta {
    pub id: Id,
    pub created_at: Timestamp,
    pub updated_at: Timestamp,
}

impl Meta {
    pub fn new(id: Id) -> Self {
        let now = Utc::now();
        Self {
            id,
            created_at: now,
            updated_at: now,
        }
    }

    pub fn bump(&mut self) {
        self.updated_at = Utc::now();
    }
}

impl Default for Meta {
    fn default() -> Self {
        Self::new(0)
    }
}
