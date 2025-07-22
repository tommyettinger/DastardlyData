### Steps To Use This Template

- replace the license file with your desired license ✅
- remove excess categories in .gitignore ✅
- remove excess categories in .gitconfig ✅
- adjust xml in main .csproj ✅
- rename test, benchmarks, and main .csproj ✅
- rename solution and reconnect projects to solution ✅
- replace this readme

### Unity & Godot Game Engine Language Support

| Engine Version        | .NET Version         | C# Version | Backend        | Build System   | Compiler     | Nuget |
| --------------------- | -------------------- | ---------- | -------------- | -------------- | ------------ | ----- |
| Godot 3.x             | .NET framework 4.7.2 | C# 7.3     | Mono           | MSBuild        | Roslyn       | 🟨   |
| Godot 4.x Default     | .NET 6               | C# 10      | CoreCLR        | MSBuild        | Roslyn       | ✅   |
| Godot 4.x Web         | Unsupported ❌       | ❌         | ❌            | ❌             | ❌          | ❌   |
| Godot 4.x Android Min | .NET 7               | C# 11      | CoreCLR        | MSBuild        | Roslyn       | ✅   |
| Godot 4.x IOS Min     | .NET 8               | C# 12      | CoreCLR        | MSBuild        | Roslyn       | ✅   |
| Godot 4.x Max         | .NET 8               | C# 12      | CoreCLR        | MSBuild        | Roslyn       | ✅   |

- 🟨 nuget package support in Godot 3.x was a little rough, expect to need to clear your build cache and restart the MSBuild server occaisionally
