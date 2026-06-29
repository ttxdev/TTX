use std::fmt;

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct ValidationFailure {
    pub property: String,
    pub code: String,
    pub message: String,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Error {
    InvalidAction(String),
    NotFound(String),
    InvalidValueObject(String),
    InvalidRequest(Vec<ValidationFailure>),
    Database(String),
    External(String),
    Busy(String),
}

impl Error {
    pub fn not_found(entity: &str) -> Self {
        Error::NotFound(entity.to_string())
    }
}

impl fmt::Display for Error {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            Error::InvalidAction(msg) => write!(f, "{msg}"),
            Error::NotFound(name) => write!(f, "{name} not found"),
            Error::InvalidValueObject(msg) => write!(f, "{msg}"),
            Error::InvalidRequest(_) => write!(f, "Invalid Request"),
            Error::Database(msg) => write!(f, "{msg}"),
            Error::External(msg) => write!(f, "{msg}"),
            Error::Busy(msg) => write!(f, "{msg}"),
        }
    }
}

impl std::error::Error for Error {}

impl From<sqlx::Error> for Error {
    fn from(value: sqlx::Error) -> Self {
        Error::Database(value.to_string())
    }
}

pub type Result<T> = std::result::Result<T, Error>;
