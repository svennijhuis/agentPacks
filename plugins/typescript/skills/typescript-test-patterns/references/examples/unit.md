# Unit

Match the repo runner. A `"test"` script wins.

```ts
it("rejects empty input", () => {
  expect(() => parse("")).toThrow(/empty/);
});
```

```bash
<pm> run test
<pm> exec vitest run
<pm> exec jest
node --test
```

Bad: `expect(fs.readFileSync("README.md", "utf8")).toContain("rejects empty input")` — docs wording is reviewed, not unit-tested.
