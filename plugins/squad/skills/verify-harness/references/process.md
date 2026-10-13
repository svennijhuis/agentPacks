# Process rules

- Start the real app process with `exec` or `setsid` and record that PID (not a wrapper like `npm`), so Cleanup stops the server itself.
- Cleanup kills that process group (`kill -- -<pgid>`), never by name.
- After Cleanup, check every Launch port is free (`curl` fails or no listener). A port still open is a Cleanup failure in the report.
- Each run writes evidence to a new `<run-id>` folder (timestamp), so earlier runs stay.
