# Unit

```rust
#[test]
fn parses_valid_input() -> Result<(), Box<dyn std::error::Error>> {
    let value = parse("42")?;
    assert_eq!(value, 42);
    Ok(())
}
```

When failure is the behavior, inspect the error. Do not `?` it away.

```bash
cargo test -p <package> parses_valid_input
```

Bad: `assert!(std::fs::read_to_string("README.md")?.contains("parses valid input"));` — docs wording is reviewed, not unit-tested.
