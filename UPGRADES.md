# DotNetSamples — Containerization & Upgrade Notes

This document records (1) how the samples were containerized and (2) concrete upgrade
opportunities found while building them. All findings below were observed by actually
building/running the projects in containers, not inferred from code alone.

## 1. Containerization

Added a multi-stage `Dockerfile` per Linux-runnable ASP.NET Core project and a root
`docker-compose.yml`. Each app self-seeds a SQLite Northwind database on first start
from `App_Data/EqDemoData.zip`, so **no external database is required**.

```bash
docker compose up -d --build                          # 8 working services
docker compose --profile needs-upgrade up -d --build  # also try legacy SPA samples
```

| Port | Service | TFM | Runtime image | Status |
|------|---------|-----|---------------|--------|
| 8001 | razor-advancedsearch | net8.0 | aspnet:8.0 | ✅ HTTP 200 |
| 8002 | razor-adhocreporting | net8.0 | aspnet:8.0 | ✅ 200 (login at `/Identity/Account/Login`) |
| 8003 | razor-datafiltering | net8.0 | aspnet:8.0 | ✅ HTTP 200 |
| 8004 | mvc-datafiltering | net8.0 | aspnet:8.0 | ✅ HTTP 200 |
| 8005 | blazor-server | net6.0 | aspnet:6.0 | ✅ HTTP 200 |
| 8006 | blazor-wasm | net6.0 | aspnet:6.0 | ✅ HTTP 200 |
| 8007 | angular-advancedsearch | net8.0 + Angular 12 | aspnet:8.0 | ✅ HTTP 200 (client built on Node 16) |
| 8013 | razor-ts-adhocreporting | net8.0 + Rollup 4 | aspnet:8.0 | ✅ HTTP 200 (client built on Node 20) |
| 8009 | vue2-advancedsearch | net6.0 + Vue CLI 3 | — | ❌ build fails (see §3) |
| 8011 | react-advancedsearch | net6.0 + react-scripts 1.x | — | ❌ build fails (see §3) |
| 8012 | stencil-advancedsearch | net6.0 + Stencil 2 | — | ❌ build fails (see §3) |

**Not containerized (Windows-only — cannot run in Linux containers):**
- `AspNet4/` (ASP.NET MVC 5 + WebForms, .NET Framework 4.x)
- `WinForms/`, `Wpf/` (.NET Framework 4.x and `net6.0-windows`)
- `AspNetCore/Vue3` and `AspNetCore/Angular/AdHocReporting` use a legacy Visual Studio
  JavaScript project (`.esproj` + `Microsoft.AspNetCore.SpaProxy`) whose build is tied to
  the VS JS SDK / dev-server proxy; they need a Node-build refactor before they containerize
  cleanly (see §3).

## 2. .NET / NuGet upgrades

### 2.1 Target framework
- **`net6.0` reached end-of-life on 2024-11-12.** These projects should move to **`net8.0`
  (LTS, supported to 2026-11)** or `net9.0`:
  `Blazor/AdvancedSearch.BlazorServer`, `Blazor/AdHocReporting.BlazorWasm/*`,
  `Vue2`, `Vue3`, `React`, `Stencil`.
  Moving them to `net8.0` also lets their containers use the supported `aspnet:8.0` base
  image instead of the EOL `aspnet:6.0`.
- `WinForms/EqDemoWinFormsNet6` and `Wpf/EqDemoWpfNet6` target `net6.0-windows` → bump to
  `net8.0-windows`.
- The `net8.0` projects (all `Razor-Mvc/*`, both Angular) are current.

### 2.2 NuGet version drift (inconsistent across the repo)
Microsoft framework packages are pinned at a mix of `6.0.1`, `8.0.2`, `8.0.4`, `8.0.5`.
Align every project on the **latest 8.0.x patch**, e.g.:
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.Identity.UI`
- `Microsoft.EntityFrameworkCore.Sqlite` / `.SqlServer` / `.Tools`
- `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore`

### 2.3 Old packages with known-CVE history (prioritize)
- `Microsoft.Data.SqlClient` **2.1.7** (React sample) → align to **5.2.x** (used elsewhere).
- `System.Data.SqlClient` **4.8.6** → legacy; replace with `Microsoft.Data.SqlClient`.
- `System.Net.Http` **4.3.4** → remove (provided by the framework) or update.
- `System.Text.RegularExpressions` **4.3.1** → remove/update (ReDoS-era package).
- `System.Drawing.Common` **4.7.2 / 4.7.3** → update to **8.0.x**; note it is unsupported on
  non-Windows since .NET 6 (PdfSharp/ClosedXML exporters rely on it).

### 2.4 EasyQuery / Korzh packages (latest stable on NuGet)
- `Korzh.EasyQuery.*` **7.4.0** → **7.4.2**
- `EasyData.Exporters.*` **1.5.8 / 1.5.9** → **1.5.12**
- `Korzh.DbUtils.*` mixed **1.4.1 / 1.4.3** → align on **1.4.3**
- `AspNet4` projects still reference `7.4.0-rc04` prerelease → move to stable `7.4.0`/`7.4.2`.

## 3. Front-end toolchain upgrades (block SPA containerization)

| Sample | Tool (current) | Failure when building | Recommended upgrade |
|--------|----------------|-----------------------|---------------------|
| React | `react-scripts ^1.1.5` (2018) | `Failed to minify` on `@easydata/core` ESM | `react-scripts` 5.x, or migrate to **Vite** |
| Vue2 | `@vue/cli-service ^3`, Vue 2 (EOL), TS 3.4 | TS `strictPropertyInitialization` errors (`Property 'context' has no initializer`) | Migrate to **Vue 3 + Vite**, TS 5 |
| Stencil | `@stencil/core ^2` | prerender hydration crash during `stencil build --prerender` | **Stencil 4** |
| Angular (adv. search) | Angular 12 | builds only on **Node 16** (needs `--openssl-legacy-provider` on newer Node) | **Angular 17/18** to build on Node 20+ |
| Vue3 / Angular-AdHoc | `.esproj` + `Microsoft.AspNetCore.SpaProxy` | VS JavaScript-project build, dev-server proxy | replace with a plain Node build step invoked from the `.csproj` (like the other SPA samples) so it builds headless in CI/containers |

## 4. Other observations
- No `global.json` — builds float to whatever SDK is installed. Consider pinning the SDK.
- Containers log `Failed to determine the https port for redirect` because only HTTP is
  exposed. Harmless for the demo; for production, terminate TLS at a reverse proxy.
- `App_Data/` content (sample DB seed zip, saved-query JSON) is **not** published by default;
  the Dockerfiles copy it explicitly into the image.
