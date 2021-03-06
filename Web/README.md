# Web API

This ASP.NET Core library includes a controller to core resources and a base controller for customized resource provider.
It also contains an authentication handler and some and toolkits.

Following are the commonly used types.

- `ResourceAccessController` class: an MVC web API controller for core resources.
- `ResourceEntityController<TProvider, TEntity>` base class: an MVC web API controller to route the network transfer to `OnPremisesResourceEntityProvider<TEntity>` implementation.
