# Mocha Game Engine Development Instructions

**CRITICAL: Always follow these instructions first and fallback to additional search and context gathering only when the information here is incomplete or found to be in error.**

Mocha is a Windows-only game engine with Vulkan renderer, C# scripting, and hot-loading HTML/CSS UI system. It uses a complex build process that combines C++ native code (with vcpkg dependencies) and .NET 7.0 C# projects with automatic interop code generation.

## Working Effectively

### Prerequisites (Windows Only)
- **Windows 10/11** - Engine is Windows-only and cannot be built on Linux/macOS
- **Visual Studio 2022** - Required for MSBuild and vcproj support
- **.NET 7.0 SDK** - Required for C# projects (though .NET 8.0 can build .NET 7.0 targets)
- **vcpkg** - Package manager for C++ dependencies
- **Vulkan SDK** - Graphics API requirements
- **Git** with submodules support

### Initial Repository Setup
```bash
# Clone repository with all submodules
git clone --recursive https://github.com/mocha-engine/mocha
cd mocha

# If already cloned, update submodules
git submodule update --init --recursive
```

### Build Process (Windows Only)
**NEVER CANCEL BUILDS**: Builds may take 15-45 minutes. Always set timeout to 60+ minutes.

```batch
# Navigate to Source directory
cd Source

# 1. Install vcpkg dependencies (5-15 minutes)
setup_x64.cmd
# OR for 32-bit: setup_x86.cmd

# 2. Build the entire solution (15-45 minutes total)
# NEVER CANCEL: Complex dependency resolution and interop generation
msbuild Mocha.sln /p:Configuration=Debug /p:Platform=x64
# OR use Visual Studio: Open Source/Mocha.sln and build solution
# First build takes longer due to vcpkg package installation
```

**Build Order Dependencies:**
1. `MochaTool.InteropGen` - Generates C++/C# interop bindings (must build first)
2. `Mocha.Common` - Shared C# library (depends on generated interop code)
3. `Mocha.Engine`, `Mocha.UI`, etc. - Core engine libraries
4. `Mocha.Host` - Native C++ host executable
5. `Mocha` and `MochaDedicatedServer` - Final executables

### Running the Engine
```batch
# After successful build, run the main executable
cd ..\build
Mocha.exe

# Or run with a specific project
Mocha.exe -project ..\Samples\mocha-minimal\project.json
```

### Testing
```batch
# Navigate to Source directory
cd Source

# Run .NET tests (5-10 minutes)
# NEVER CANCEL: Set timeout to 15+ minutes
dotnet test Mocha.Tests/ --configuration Debug

# Run specific test project
dotnet test Mocha.Tests/Mocha.Tests.csproj
```

### Code Formatting and Linting
```batch
# Format .NET code (required before commits)
cd Source
dotnet format

# Check formatting without making changes
dotnet format --verify-no-changes

# The CI pipeline (.github/workflows/format.yml) will fail if code is not formatted
# Always run dotnet format before committing changes
```

**Formatting Notes:**
- Uses tabs for indentation (tab_size = 4) as defined in .editorconfig
- Enforces CRLF line endings on Windows
- Formatting applies only to .NET projects (C++ projects use .clang-format)
- Security vulnerabilities in dependencies (SixLabors.ImageSharp) may cause warnings

## Project Structure

### Key Directories
- **`Source/`** - All source code and project files
  - **`Mocha.sln`** - Main Visual Studio solution
  - **`vcpkg.json`** - C++ dependency manifest
  - **`setup_x64.cmd`** / **`setup_x86.cmd`** - Dependency installation scripts
- **`Content/`** - Core engine content and assets
- **`Samples/`** - Example projects
  - **`mocha-minimal/`** - Basic sample project
- **`build/`** - Build output directory (created during build)

### C# Projects (.NET 7.0)
- **`Mocha.Engine`** - Core game engine library
- **`Mocha.Common`** - Shared utilities and interop bindings
- **`Mocha.UI`** - HTML/CSS UI system
- **`Mocha.Hotload`** - C# hot-loading system
- **`Mocha.Editor`** - ImGui-based editor
- **`MochaTool.AssetCompiler`** - Asset compilation tool
- **`MochaTool.InteropGen`** - C++/C# interop code generator
- **`Mocha.Tests`** - MSTest unit tests

### C++ Projects (Visual Studio)
- **`Mocha.Host`** - Native C++ host library (Vulkan, SDL2, Jolt Physics)
- **`Mocha`** - Main executable wrapper
- **`MochaDedicatedServer`** - Server executable

### Sample Projects

#### mocha-minimal Sample
Located in `Samples/mocha-minimal/`, this is the basic sample project demonstrating engine usage.

**Structure:**
- `project.json` - Project configuration (name, version, resources, tick rate)
- `code/` - C# game code (Game.cs, Player.cs, Controllers)
- `content/` - Game assets
  - `materials/pbr/` - PBR material definitions and textures (.mmat, .mtex files)
  - `models/` - 3D models (FBX files and .mmdl material mappings)
  - `ui/` - SCSS stylesheets for HTML/CSS UI

**Running the Sample:**
```batch
cd build
Mocha.exe -project ..\Samples\mocha-minimal\project.json
```

## Common Development Tasks

### Building Individual Components
```batch
# Build only .NET projects (faster iteration)
cd Source
dotnet build --configuration Debug

# Build specific project
dotnet build Mocha.Engine/Mocha.Engine.csproj

# Build native components only (requires Visual Studio)
msbuild Mocha.Host/Mocha.Host.vcxproj /p:Configuration=Debug /p:Platform=x64
```

### Asset Compilation
```batch
# Compile assets (after successful build)
# This compiles content from both sample project and core engine content
build\MochaTool.AssetCompiler.exe --mountpoints "samples/mocha-minimal/content/" "content/core/" -f

# Or use the provided batch file
BuildContent.bat
```

### Debugging
```batch
# The engine supports both C++ and C# debugging
# Use Visual Studio debugger with "Mocha" profile
# Launch settings are auto-generated in each project's Properties/launchSettings.json
```

## Validation Requirements

### Manual Validation Scenarios
After making changes, always test these core scenarios:

1. **Engine Launch Test**
   ```batch
   cd build
   Mocha.exe
   # Verify: Engine window opens without crashes
   ```

2. **Sample Project Test**
   ```batch
   cd build
   Mocha.exe -project ..\Samples\mocha-minimal\project.json
   # Verify: Sample loads and renders correctly
   ```

3. **Hot-loading Test**
   ```batch
   # With engine running, modify C# code in sample project
   # Verify: Changes reload automatically without restart
   ```

### Pre-commit Validation
```batch
# ALWAYS run before committing:
cd Source

# 1. Format code
dotnet format

# 2. Build solution (NEVER CANCEL - 15-45 minutes)
msbuild Mocha.sln /p:Configuration=Debug /p:Platform=x64

# 3. Run tests (NEVER CANCEL - 5-10 minutes)
dotnet test

# 4. Test basic functionality
cd ..\build
Mocha.exe -project ..\Samples\mocha-minimal\project.json
```

## Troubleshooting

### Build Issues
- **"Missing Microsoft.Cpp.Default.props"** - Install Visual Studio 2022 with C++ workload
- **"vcpkg packages not found"** - Run `setup_x64.cmd` first
- **"Vulkan not found"** - Install Vulkan SDK and ensure graphics drivers are updated
- **"InteropGen.exe not found"** - Build order issue; clean and rebuild entire solution
- **".NET 7.0 not found"** - Install .NET 7.0 SDK (or newer with 7.0 targeting pack)
- **"Package vulnerabilities"** - Known issues with SixLabors.ImageSharp 3.1.5; warnings can be ignored for development
- **"Cannot open .vcxproj"** - Linux limitation; C++ projects require Windows and Visual Studio

### Runtime Issues
- **"Vulkan initialization failed"** - Update graphics drivers or install Vulkan runtime
- **"Assembly not found errors"** - Ensure all projects built successfully
- **"Hot-loading not working"** - Check that C# projects are in Debug configuration

### Performance Issues
- **Slow builds** - Normal behavior; builds can take 15-45 minutes
- **Slow vcpkg install** - Normal behavior; first-time setup can take 15+ minutes

## Important Notes

- **Windows Only**: Cannot build or run on Linux/macOS
- **Long Build Times**: NEVER CANCEL builds - they can take 45+ minutes
- **Complex Dependencies**: Build failures often cascade; clean and rebuild entire solution
- **Format Requirements**: Code must be formatted with `dotnet format` or CI will fail
- **Interop Complexity**: Changes to C++ headers may require regenerating interop bindings
- **Visual Studio Dependency**: While some .NET parts work with dotnet CLI, full builds require Visual Studio

## CI/CD Pipeline

### GitHub Actions
- **`.github/workflows/format.yml`** - Validates .NET code formatting
- **`.github/workflows/commit-log.yml`** - Discord notifications

The format workflow will fail if code is not properly formatted. Always run `dotnet format` before committing.

## Getting Help

- **Discord**: [https://discord.gg/SDP4R6Wsza](https://discord.gg/SDP4R6Wsza)
- **Wiki**: [https://wiki.getmocha.org/](https://wiki.getmocha.org/)
- **Issues**: [https://github.com/mocha-engine/mocha/issues](https://github.com/mocha-engine/mocha/issues)
- **Style Guide**: [Mocha Style Guidelines](https://wiki.getmocha.org/index.php/Style_Guidelines)