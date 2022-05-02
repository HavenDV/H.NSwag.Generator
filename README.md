# [H.NSwag.Generator](https://github.com/HavenDV/H.NSwag.Generator/) 

### [![NuGet](https://img.shields.io/nuget/dt/H.NSwag.Generator.svg?style=flat-square&label=H.NSwag.Generator)](https://www.nuget.org/packages/H.NSwag.Generator/)
```
Install-Package H.NSwag.Generator
```

### Usage
The generator generates code based on any .nswag file in the AdditionalFiles ItemGroup.
```xml
<ItemGroup>
  <AdditionalFiles Include="openapi.nswag" />
</ItemGroup>
```

### Global options
Enable caching - suitable for cases where your openapi specification rarely changes or you get it via url.
```xml
<PropertyGroup>
  <NSwagGenerator_UseCache>true</NSwagGenerator_UseCache>
</PropertyGroup>
```

### Contacts
* [mail](mailto:havendv@gmail.com)
