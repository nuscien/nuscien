# NuScien 6

[![MIT Licensed](./docs/assets/badge_lisence_MIT.svg)](https://github.com/nuscien/nuscien/blob/master/LICENSE)

A framework with ACL and CMS built-in.
It provide a way to build the project by let you only need focus on business logic itself.
It contains both for client-side SDK and server-side service.

See [sample and tutorials](https://github.com/nuscien/sample) to get start. Following are the introduction for each project.

## Core

[![NuGet package](https://img.shields.io/nuget/dt/NuScien?label=nuget+downloads)](https://www.nuget.org/packages/NuScien)

![.NET 6](./docs/assets/badge_NET_6.svg)
![.NET 5](./docs/assets/badge_NET_5.svg)
![.NET Core 3.1](./docs/assets/badge_NET_Core_3_1.svg)
![.NET Standard 2.0](./docs/assets/badge_NET_Standard_2_0.svg)
![.NET Framework 4.8](./docs/assets/badge_NET_Fx_4_8.svg)
![.NET Framework 4.6.1](./docs/assets/badge_NET_Fx_4_6_1.svg)

`NuScien.dll`

The core library of NuScien project.
It contains the foundation for ACL and business resource entity.

Following are the commonly used types.

- `BaseResourceEntity` base class and its sub-classes: the shared model for DAL, BLL, Web API and GUI databinding.
- `OnPremisesResourceAccessClient` class: used to authorize (OAuth 2.0) and to access core resources for server-side.
- `HttpResourceAccessClient` class: used to authorize and access core resources for client-side.
- `HttpResourceEntityProvider<TEntity>` base class: provide a way for client-side to access the entity or its collection in business.
- `HttpResourceAccessContext` base class: one place for client-side to access all entity providers and core resources.

P.S.:
Core resources mentioned above includes passport, groups, settings, CMS, etc.
Authorization is based on OAuth 2.0 and powered by [Trivial](https://github.com/nuscien/trivial) library.

## OnPremises

[![NuGet package](https://img.shields.io/nuget/dt/NuScien.OnPremises?label=nuget+downloads)](https://www.nuget.org/packages/NuScien.OnPremises)

![.NET 6](./docs/assets/badge_NET_6.svg)
![.NET 5](./docs/assets/badge_NET_5.svg)
![.NET Core 3.1](./docs/assets/badge_NET_Core_3_1.svg)

`NuScien.OnPremises.dll`

On-premises library uses Entity Framework to implement the resource access provider and other data provider of the core library.
It can be used as the core of one of the following scenarios.

- On-premises server-side web API app.
- Cloud web API services.
- Admin client tools on-premises.
- Windows service for special case.

Following are the commonly used types.

- `AccountDbSetProvider` class: an implementation used for `OnPremisesResourceAccessClient` to power core resources management based on database for server-side.
- `OnPremisesResourceEntityProvider<TEntity>` base class: provide a way for server-side business logic about a specific type of the resource entity.
- `OnPremisesResourceAccessContext` base class: one place for server-side to access all entity providers and core resources.

## Web

[![NuGet package](https://img.shields.io/nuget/dt/NuScien.Web?label=nuget+downloads)](https://www.nuget.org/packages/NuScien.Web)

![ASP.NET Core 6.0](./docs/assets/badge_ASPNET_6_0.svg)
![ASP.NET Core 5.0](./docs/assets/badge_ASPNET_5_0.svg)

`NuScien.Web.dll`

Web library includes a controller to core resources and a base controller for customized resource provider.
It also contains an authentication handler and some and toolkits.

Following are the commonly used types.

- `ResourceAccessController` class: an MVC web API controller for core resources.
- `ResourceEntityController<TProvider, TEntity>` base class: an MVC web API controller to route the network transfer to `OnPremisesResourceEntityProvider<TEntity>` implementation.

## Others

| Project | Description |
| ------------ | ----------------------- |
| `UnitTest` | The unit tests. |
| `SampleSite` | A simple Web API. |

And the directory `Sql` is used to store SSMS project and TSQL files to create initialized tables and to do other useful things for database.
