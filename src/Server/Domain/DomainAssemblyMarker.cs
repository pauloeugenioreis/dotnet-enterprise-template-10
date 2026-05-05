namespace ProjectTemplate.Domain;

/// <summary>
/// Marker class used as an anchor to reference the Domain assembly.
/// Used with assembly scanning tools such as FluentValidation, MediatR, Scrutor, etc.
/// </summary>
/// <example>
/// FluentValidation — registers ALL validators in the Domain assembly:
/// <code>
/// services.AddValidatorsFromAssemblyContaining&lt;DomainAssemblyMarker&gt;();
/// </code>
/// MediatR — registers all handlers in the Domain assembly:
/// <code>
/// services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DomainAssemblyMarker).Assembly));
/// </code>
/// </example>
public sealed class DomainAssemblyMarker;
