namespace LogistiCore.Domain.Rules;

public static class ParcelStatus
{
    public const string Registered = "Registered";
    public const string AtWarehouse = "AtWarehouse";
    public const string Sorted = "Sorted";
    public const string Assigned = "Assigned";
    public const string OutForDelivery = "OutForDelivery";
    public const string Delivered = "Delivered";
    public const string FailedAttempt = "FailedAttempt";
    public const string Returned = "Returned";
    public const string Lost = "Lost";
    public const string Damaged = "Damaged";
}

public static class ServiceLevels
{
    public const string Standard = "Standard";
    public const string Express = "Express";
    public const string Overnight = "Overnight";

    public static readonly string[] All = { Standard, Express, Overnight };
}

public static class HandlingTags
{
    public static readonly string[] All = { "Fragile", "KeepUpright", "Perishable", "SignatureRequired" };
}

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
