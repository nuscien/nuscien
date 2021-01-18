> Still developing...

# NuScien 5

[![MIT Licensed](./docs/assets/badge_lisence_MIT.svg)](https://github.com/nuscien/nuscien/blob/master/LICENSE)

A framework with ACL, CMS and message management.

See [sample and tutorials](https://github.com/nuscien/sample) to get start. Following are the introduction for each project.

## Core

[![NuGet package](https://img.shields.io/nuget/dt/NuScien?label=nuget+downloads)](https://www.nuget.org/packages/NuScien)
![.NET 5.0](./docs/assets/badge_NET_5_0.svg)
![.NET Core 3.1](./docs/assets/badge_NET_Core_3_1.svg)

> `NuScien.dll`

The common library.

- `BaseResourceEntity` base class and its sub-classes: the shared model for DAL, BLL, Web API and GUI databinding.
- `OnPremisesResourceAccessClient` class: used to authorize (OAuth 2.0) and to access core resources for server-side.
- `HttpResourceAccessClient` class: used to authorize and access core resources for client-side.
- `HttpResourceEntityProvider<TEntity>` base class: provide a way for client-side to access the entity or its collection in business.
- `HttpResourceAccessContext` base class: one place for client-side to access all entity providers and core resources.

P.S.:
Core resources mentioned above include passport, groups, messages, settings, etc.
Authorizationn is based on OAuth 2.0 powered by [Trivial](https://github.com/nuscien/trivial) library.

## OnPremises

[![NuGet package](https://img.shields.io/nuget/dt/NuScien.OnPremises?label=nuget+downloads)](https://www.nuget.org/packages/NuScien.OnPremises)
![.NET 5.0](./docs/assets/badge_NET_5_0.svg)
![.NET Core 3.1](./docs/assets/badge_NET_Core_3_1.svg)

> `NuScien.OnPremises.dll`

On-premises library with EF Core 5.0 supports.

- `AccountDbSetProvider` class: an implementation used for `OnPremisesResourceAccessClient` to power core resources management based on database for server-side.
- `OnPremisesResourceEntityProvider<TEntity>` base class: provide a way for server-side business logic about a specific type of the resource entity.
- `OnPremisesResourceAccessContext` base class: one place for server-side to access all entity providers and core resources.

## Web

[![NuGet package](https://img.shields.io/nuget/dt/NuScien.Web?label=nuget+downloads)](https://www.nuget.org/packages/NuScien.Web)
![ASP.NET Core 5.0](./docs/assets/badge_ASPNET_5_0.svg)

> `NuScien.Web.dll`

Web API components and toolkits.

- `ResourceAccessController` class: an MVC web API controller for core resources.
- `ResourceEntityController<TProvider, TEntity>` base class: an MVC web API controller to route the network transfer to `OnPremisesResourceEntityProvider<TEntity>` implementation.

## Unit tests and others

| Project | Description |
| ------------ | ----------------------- |
| `UnitTest` | The unit tests. |
| `SampleSite` | A simple Web API. |

The directory `Sql` is used to store SSMS project and TSQL files to create initialized tables and to do other useful things for database.
