//! Validation rules ported from the value objects in
//! `TTX.Domain.ValueObjects`.
//!
//! The value objects themselves are represented as plain type aliases (see
//! [`crate::primitives`]); these functions carry over their `TryValidate`/`Create`
//! rules and return [`Error::InvalidValueObject`] on failure, mirroring the
//! C# `InvalidValueObjectException`.

use crate::error::{Error, Result};

pub const NAME_MIN_LENGTH: usize = 3;
pub const NAME_MAX_LENGTH: usize = 25;
pub const SLUG_MIN_LENGTH: usize = 3;
pub const SLUG_MAX_LENGTH: usize = 25;
pub const TICKER_MIN_LENGTH: usize = 2;
pub const TICKER_MAX_LENGTH: usize = 15;

fn invalid(kind: &str, reason: &str) -> Error {
    Error::InvalidValueObject(format!("{kind} {reason}"))
}

pub fn validate_name(value: &str) -> Result<()> {
    if value.trim().is_empty() {
        return Err(invalid("Name", "cannot be null or empty."));
    }

    let len = value.chars().count();
    if !(NAME_MIN_LENGTH..=NAME_MAX_LENGTH).contains(&len) {
        return Err(invalid(
            "Name",
            &format!("must be between {NAME_MIN_LENGTH} and {NAME_MAX_LENGTH} characters."),
        ));
    }

    if !value.chars().all(|c| c.is_ascii_alphanumeric() || c == '_') {
        return Err(invalid(
            "Name",
            "can only contain letters, numbers, and underscores.",
        ));
    }

    Ok(())
}

pub fn validate_slug(value: &str) -> Result<()> {
    if value.trim().is_empty() {
        return Err(invalid("Slug", "cannot be null or empty."));
    }

    let len = value.chars().count();
    if !(SLUG_MIN_LENGTH..=SLUG_MAX_LENGTH).contains(&len) {
        return Err(invalid(
            "Slug",
            &format!("must be between {SLUG_MIN_LENGTH} and {SLUG_MAX_LENGTH} characters."),
        ));
    }

    if !value
        .to_lowercase()
        .chars()
        .all(|c| c.is_ascii_alphanumeric() || c == '_')
    {
        return Err(invalid(
            "Slug",
            "can only contain lowercase letters, numbers, and hyphens.",
        ));
    }

    Ok(())
}

pub fn validate_ticker(value: &str) -> Result<()> {
    if value.trim().is_empty() {
        return Err(invalid("Ticker", "cannot be null or empty."));
    }

    let len = value.chars().count();
    if !(TICKER_MIN_LENGTH..=TICKER_MAX_LENGTH).contains(&len) {
        return Err(invalid(
            "Ticker",
            &format!("must be between {TICKER_MIN_LENGTH} and {TICKER_MAX_LENGTH} characters."),
        ));
    }

    if !value
        .to_uppercase()
        .chars()
        .all(|c| c.is_ascii_alphanumeric() || matches!(c, '+' | '$' | ':'))
    {
        return Err(invalid("Ticker", "can only contain uppercase letters."));
    }

    Ok(())
}

pub fn validate_platform_id(value: &str) -> Result<()> {
    if value.trim().is_empty() {
        return Err(invalid("PlatformId", "cannot be null or empty."));
    }

    if !value.chars().all(|c| c.is_ascii_digit()) {
        return Err(invalid("PlatformId", "must be numeric"));
    }

    Ok(())
}

pub fn validate_credits(value: f64) -> Result<()> {
    if value < 0.0 {
        return Err(invalid("Credits", "cannot be negative."));
    }

    Ok(())
}

pub fn validate_quantity(value: i64) -> Result<()> {
    if value < 0 {
        return Err(invalid("Quantity", "cannot be less than zero."));
    }

    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn name_rules() {
        assert!(validate_name("valid_name").is_ok());
        assert!(validate_name("").is_err());
        assert!(validate_name("ab").is_err());
        assert!(validate_name("has space").is_err());
        assert!(validate_name("dash-not-allowed").is_err());
    }

    #[test]
    fn slug_rules() {
        assert!(validate_slug("valid_slug").is_ok());
        assert!(validate_slug("MixedCase").is_ok());
        assert!(validate_slug("no").is_err());
        assert!(validate_slug("has space").is_err());
    }

    #[test]
    fn ticker_rules() {
        assert!(validate_ticker("ABC").is_ok());
        assert!(validate_ticker("abc").is_ok());
        assert!(validate_ticker("A").is_err());
        assert!(validate_ticker("with space").is_err());
    }

    #[test]
    fn platform_id_rules() {
        assert!(validate_platform_id("123456").is_ok());
        assert!(validate_platform_id("").is_err());
        assert!(validate_platform_id("12a").is_err());
    }

    #[test]
    fn credits_and_quantity_rules() {
        assert!(validate_credits(0.0).is_ok());
        assert!(validate_credits(-1.0).is_err());
        assert!(validate_quantity(0).is_ok());
        assert!(validate_quantity(-1).is_err());
    }
}
