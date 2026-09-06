# Integration

Lives in `<crate>/tests/*.rs`. Public API only.

```rust
#[test]
fn rejects_empty_body() {
    let err = client::post("").expect_err("empty body");
    assert!(err.to_string().contains("empty"));
}
```

```bash
cargo test -p <package> --test <integration-target>
cargo test --workspace --no-fail-fast
```

When nextest is configured: `cargo nextest run --workspace` plus `cargo test --workspace --doc`.

Local crate. Deployed URL smoke: [deployed-smoke](deployed-smoke.md).
