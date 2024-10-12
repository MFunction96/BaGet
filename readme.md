# BaGet

> Forked from [loic-sharma/BaGet](https://github.com/loic-sharma/BaGet/)

[![Build Status](https://dev.azure.com/XanaCN/Lyoko/_apis/build/status/BaGet?branchName=main)](https://dev.azure.com/XanaCN/Lyoko/_build/latest?definitionId=16&branchName=main) [![Release Status](https://vsrm.dev.azure.com/XanaCN/_apis/public/Release/badge/f06af8ee-5084-455c-ac24-8fc4f735382c/4/6)](https://dev.azure.com/XanaCN/Lyoko/_release?view=all&path=%BaGet&_a=releases) [![Code Coverage](https://img.shields.io/azure-devops/coverage/XanaCN/Lyoko/16/main)]()

[![dotnet](https://img.shields.io/badge/.NET-%3E%3D8.0.4-blue.svg?style=flat-square&logo=.NET)](https://dotnet.microsoft.com/)
[![GitHub License](https://img.shields.io/github/license/MFunction96/BaGet)](https://github.com/MFunction96/BaGet/blob/main/LICENSE)
![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/MFunction96/BaGet/total)

There has been no update onto the [original BaGet repository](https://github.com/loic-sharma/BaGet/) for a long time. This fork aims to maintain the core function of NuGet Server and keep BaGet alive. So this fork will 
 keep the minimal common components, then lightweight some features, especially custom cloud support, multiple database and storage support.

**ATTENTION!!! This fork has switched to `AGPL-3.0` rather than `MIT`.**

**Do SUPPORT**

- Core NuGet Server function, including Upload, Store, Restore package.
- *(In future)* User control/API Token and Package Management
- Based on .NET 8 and above LTS
- MariaDB and SQLite
- FileSystem storage
- Docker
- *(In future)* `deb` for Linux, `msi`(or `exe`) for Windows easy-installation

> Please set `Mariadb` on `Database:Type` at `appsettings.json` to use MariaDB Database. 

**DO NOT SUPPORT**

- Custom Cloud components, such as AWS, Azure, Aliyun, etc.
- AzureTable, MySQL, SQL Server, PostgreSql, etc.
- Non-FileSystem storage component, such as Azure Blob, etc.

---

# Original README

![Build status] [![Discord][Discord image]][Discord link] [![Twitter][Twitter image]][Twitter link]

A lightweight [NuGet] and [symbol] server.

<p align="center">
  <img width="100%" src="https://user-images.githubusercontent.com/737941/50140219-d8409700-0258-11e9-94c9-dad24d2b48bb.png">
</p>

## Getting Started

1. Install the [.NET SDK]
2. Download and extract [BaGet's latest release]
3. Start the service with `dotnet BaGet.dll`
4. Browse `http://localhost:5000/` in your browser

For more information, please refer to the [documentation].

## Features

* **Cross-platform**: runs on Windows, macOS, and Linux!
* **Cloud native**: supports [Docker], [Azure], [AWS], [Google Cloud], [Alibaba Cloud]
* **Offline support**: [mirror a NuGet server] to speed up builds and enable offline downloads

Stay tuned, more features are planned!

[Build status]: https://img.shields.io/github/actions/workflow/status/loic-sharma/BaGet/.github/workflows/main.yml
[Discord image]: https://img.shields.io/discord/889377258068930591
[Discord link]: https://discord.gg/MWbhpf66mk
[Twitter image]: https://img.shields.io/twitter/follow/bagetapp?label=Follow
[Twitter link]: https://twitter.com/bagetapp

[NuGet]: https://learn.microsoft.com/nuget/what-is-nuget
[symbol]: https://docs.microsoft.com/en-us/windows/desktop/debug/symbol-servers-and-symbol-stores
[.NET SDK]: https://www.microsoft.com/net/download
[Node.js]: https://nodejs.org/

[BaGet's latest release]: https://github.com/loic-sharma/BaGet/releases

[Documentation]: https://loic-sharma.github.io/BaGet/
[Docker]: https://loic-sharma.github.io/BaGet/installation/docker/
[Azure]: https://loic-sharma.github.io/BaGet/installation/azure/
[AWS]: https://loic-sharma.github.io/BaGet/installation/aws/
[Google Cloud]: https://loic-sharma.github.io/BaGet/installation/gcp/
[Alibaba Cloud]: https://loic-sharma.github.io/BaGet/installation/aliyun/

[Mirror a NuGet server]: https://loic-sharma.github.io/BaGet/configuration/#enable-read-through-caching