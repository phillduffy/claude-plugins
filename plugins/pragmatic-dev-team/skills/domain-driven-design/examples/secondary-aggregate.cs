// Secondary Aggregate Pattern - Licence Entity
// From: OfficeAddins/src/Core/Core.Domain/Licensing/Licence.cs
// Demonstrates: Different aggregate, static factories, value object children

using CSharpFunctionalExtensions;

namespace Core.Domain.Licensing;

/// <summary>
/// Licence aggregate - demonstrates pattern works across bounded contexts.
/// Private constructor + static factories control object creation.
/// </summary>
public class Licence : AggregateRoot<Guid>
{
    public LicenceStatus Status { get; private set; }
    public string ClientName { get; private set; }
    public IReadOnlyCollection<Entitlement> Entitlements { get; private set; }
    private readonly LicenceMetadata? _metadata;

    // Derived properties
    public string StatusMessage => $"Licence Status: {Status}";
    public string Metadata => _metadata?.JsonString ?? "No Metadata";
    public bool IsValid => Status == LicenceStatus.Active;

    // PRIVATE CONSTRUCTOR - forces use of static factories
    private Licence(
        Guid licenceId,
        string clientName,
        IEnumerable<Entitlement> entitlements,
        LicenceMetadata? metadata,
        LicenceStatus status) : base(licenceId)
    {
        ClientName = clientName;
        Entitlements = entitlements.ToList().AsReadOnly();
        _metadata = metadata;
        Status = status;
    }

    // DOMAIN LOGIC - encapsulates business rules
    public bool HasEntitlement(Entitlement entitlement)
        => IsValid && Entitlements.Contains(entitlement);

    // STATIC FACTORY - valid licence
    public static Licence Create(
        Guid licenceId,
        string client,
        IEnumerable<Entitlement> entitlements,
        LicenceMetadata? metadata = null)
    {
        var licence = new Licence(licenceId, client, entitlements, metadata, LicenceStatus.Active);
        licence.AddDomainEvent(new LicenseUpdatedEvent(licence));
        return licence;
    }

    // STATIC FACTORY - invalid licence (different creation path)
    public static Licence CreateInvalid(LicenceStatus status)
    {
        var licence = new Licence(Guid.Empty, "Unlicensed", [], null, status);
        licence.AddDomainEvent(new LicenseUpdatedEvent(licence));
        return licence;
    }

    // PRESENTATION METHOD - aggregate knows how to present itself
    public string AsMarkdown()
    {
        var sb = new StringBuilder();
        sb.Append("- **Status**: ").Append(Status).AppendLine();
        sb.Append("- **Client Name**: ").AppendLine(ClientName);
        sb.AppendLine("- **Entitlements**:");

        foreach (var entitlement in Entitlements)
            sb.Append("  - ").Append(entitlement).AppendLine();

        return sb.ToString();
    }
}

// Supporting types
public enum LicenceStatus { Active, Expired, Invalid, Revoked }

// Entitlement as enum (could be value object for more complex cases)
public enum Entitlement { BasicFeatures, AdvancedFeatures, PremiumSupport }

// Nested value object for complex data
public record LicenceMetadata(string JsonString);

// KEY PATTERNS:
// 1. Private constructor prevents invalid object creation
// 2. Static factories for different creation scenarios (Create vs CreateInvalid)
// 3. Value object children (Entitlements collection)
// 4. Domain logic encapsulated (HasEntitlement checks validity + membership)
// 5. Events raised in factories
// 6. AsReadOnly() for collection immutability
// 7. Presentation methods can live on aggregate (AsMarkdown)
