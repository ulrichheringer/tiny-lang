# tiny-lang

An interactive interpreter for a small language with variables, arithmetic expressions, and print statements.

## Run

Requires the .NET 10 SDK. Browser builds also need the WebAssembly workload: `dotnet workload install wasm-tools`.

### Desktop

~~~sh
dotnet run --project TinyLang.Desktop/TinyLang.Desktop.csproj
~~~

### Browser

~~~sh
dotnet run --project TinyLang.Browser/TinyLang.Browser.csproj
~~~

## Example

~~~text
let value = 6;
print value * 7;
~~~
