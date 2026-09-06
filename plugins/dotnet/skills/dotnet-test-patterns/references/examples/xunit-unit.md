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
