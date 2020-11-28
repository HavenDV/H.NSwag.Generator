# [H.NSwag.Generator](https://github.com/HavenDV/H.NSwag.Generator/) 

[![Language](https://img.shields.io/badge/language-C%23-blue.svg?style=flat-square)](https://github.com/HavenDV/H.NSwag.Generator/search?l=C%23&o=desc&s=&type=Code) 
[![License](https://img.shields.io/github/license/HavenDV/H.NSwag.Generator.svg?label=License&maxAge=86400)](LICENSE.md) 
[![Requirements](https://img.shields.io/badge/Requirements-.NET%20Standard%202.0-blue.svg)](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md)
[![Build Status](https://github.com/HavenDV/H.NSwag.Generator/workflows/.NET/badge.svg?branch=master)](https://github.com/HavenDV/H.NSwag.Generator/actions?query=workflow%3A%22.NET%22)

Description

### Nuget

[![NuGet](https://img.shields.io/nuget/dt/H.NSwag.Generator.svg?style=flat-square&label=H.NSwag.Generator)](https://www.nuget.org/packages/H.NSwag.Generator/)

```
Install-Package H.NSwag.Generator
```

### Usage

** Note: NSwagGeneratedPath is the same as specified in the .nswag Output property. **
```xml

  <ItemGroup>
    <PackageReference Include="NSwag.MSBuild" Version="13.9.4">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <PropertyGroup>
    <NSwagConsolePath>$(NSwagExe_Net50)</NSwagConsolePath>
    <NSwagGeneratedPath>%TEMP%\H.NSwag.Generator\Generated.cs</NSwagGeneratedPath>
  </PropertyGroup>

```

### Contacts
* [mail](mailto:havendv@gmail.com)
