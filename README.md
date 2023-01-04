
<p align="center">
    <h3 align="center">
        Mocha
    </h3>
    <p align="center">
        A work-in-progress game engine
    </p>
</p>

![Mocha Screenshot](https://user-images.githubusercontent.com/12881812/210655312-1e0d25a1-e4bf-49d9-943b-f88f932f7e08.png)

## Features

- Bespoke modern Vulkan 1.3 renderer, with support for VK_KHR_ray query (ray tracing)
- ImGui-based editor
- Hot-loading HTML / CSS user interface solution
- Jolt physics implementation
- C# scripting with automatic C++ <-> C# interop bindings generator
- Work-in-progress physically-based shading & material implementation (currently without IBL)

## Technical Information

I'm doing a monthly series of write-ups about Mocha on my blog. These should give some good insights on how Mocha works and how I plan on expanding it in the future.

- [Mocha - December 2022](https://blog.gu3.me/mocha/)

## Building

### Prerequisites

#### Requirements

- .NET 7.0
- vcpkg
- Visual Studio 2022
- Windows

#### Recommendations

- Windows 10 / 11
- A decent graphics card & processor

### Instructions

1. First, clone the repository recursively with `git clone --recursive https://github.com/xezno/mocha`
2. Run `vcpkg install`.
3. Once complete, open `Source/Mocha.sln` and build the solution.

## Contributing

Contributions to this project are greatly appreciated; please follow these steps in order to submit your contribution to the project:

1. Fork the project
2. Create a branch under the name `YourName/FeatureName`
3. Once you've made all the changes you need to make, go ahead and submit a Pull Request.

## Code Conventions

Throughout this project, the standard [C# coding conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions) should be observed and used within any pull requests / any other direct contributions.

## Support

Feel free to [open an issue](https://github.com/xezno/mocha/issues/new) if you encounter any bugs or problems, have any feature requests, or have any questions.

## License

```
Mocha Game Engine
Copyright (C) 2023  Alex Guthrie

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
```

A copy of this license is available at [Docs/LICENSE.md](https://github.com/xezno/mocha/blob/main/Docs/LICENSE.md).

## Acknowledgements
* [Badges](https://shields.io)
* [Icons](https://www.flaticon.com/)
