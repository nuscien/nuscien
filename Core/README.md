# Core library

This provides a solution to build community and enterprise projects based on resource entity and accessories with ACL and CMS built-in.

## Resource Entity

The resource entity is the model for DAL (data access layer), BLL (business logic layer) and UX layer (including GUI view model and web API model).

The abstract class `BaseResourceEntity` is the base class for inheriting.
It supports property changing notifier so it also can be used as view model of UWP and WPF directly with databinding.
Its sub-class `SiteOwnedResourceEntity`, `SpecificOwnerResourceEntity` and `OwnerResourceEntity<TOwner, TTarget>` are also used to inherit with advanced usages.

It need be defined like following to map the entity to the table design in database and to JSON schema of the web API.

- Set an attribute `[Table]` on the class to map the table name in database.
- Set an attribute `[Column]` on the property to map the column name in the table of the database; or attribute `[NotMapped]` for the one without mapping.
- Set an attribute `[JsonPropertyName]` on the property to map the JSON property name for network transfer; or attribute `[JsonIgnore]` for the one that does not need serialize and deserialzie.
- The getter and setter of the property should be implemented by `GetCurrentProperty<T>` and `SetCurrentProperty` method to supports databinding feature except the ones you don't care about it or they are just route and convert to/from other properties.

## Authentication & Permissions

The static class `ResourceAccessClients` is used to manage the core resource access client about following features.

- User authentication and management.
- Client authentication and management.
- Site and sub-site management.
- Permission management.
- User group management.
- Authorization tokens and sessions.
- CMS (content management system). 

You need set up the connection information to the data source before usages by calling its `Setup` static method.
Then you can call it `CreateAsync` static method to get an instance to use.

## Auto-DAL

DAL is implemented by the framework by default so we can skip it unless we have a special reason to override it.
We just need implement BLL based on that.

Base class `OnPremisesResourceEntityProvider<T>` is used to implement the provider of the specific entity as its business logic layer.
It contains some methods for basic accessing capabilities.
You can override it to add ACL.
You can also extend it to add further accessing way.
Please make sure it contains the constructors with parameter types and orders as same as the one of base class.
And, of course, you can add further constructors as you want.

Then inherit `OnPremisesResourceAccessContext` class to organize all the entity providers in your business logic.
The way is very simple: define them as properties with public getter and setter.
They will be filled automatically when the instance is initialized.

## SNS

It also includes a built-in contact, blog and internal mail system.
The static class `SocialNetworkResources` is the entry point to access them.
