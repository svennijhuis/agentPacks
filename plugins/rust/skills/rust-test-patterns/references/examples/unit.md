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
