# xUnit unit

```csharp
public sealed class ParseTests
{
    [Fact]
    public void Empty_input_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => Parser.Parse(""));
    }
}
```

```bash
dotnet test <solution> --filter "FullyQualifiedName~ParseTests"
```

Bad: `Assert.Contains("rejects empty input", File.ReadAllText("README.md"));` — docs wording is reviewed, not unit-tested.
