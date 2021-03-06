# On-premises library

This library uses Entity Framework to implement the resource access provider and other data provider of the [core library](../Core).
It can be used as the core of one of the following scenarios.

- On-premises server-side web API app.
- Cloud web API services.
- Admin client tools on-premises.
- Windows service for special case.

Following are the commonly used types.

- `AccountDbSetProvider` class: an implementation used for `OnPremisesResourceAccessClient` to power core resources management based on database for server-side.
- `OnPremisesResourceEntityProvider<TEntity>` base class: provide a way for server-side business logic about a specific type of the resource entity.
- `OnPremisesResourceAccessContext` base class: one place for server-side to access all entity providers and core resources.
