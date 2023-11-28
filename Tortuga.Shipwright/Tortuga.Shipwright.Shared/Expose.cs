namespace Tortuga.Shipwright;

/// <summary>
/// Flags for exposing members.
/// </summary>
[Flags]
public enum Expose
{
    /// <summary>
    /// Do not auto-expose members.
    /// </summary>
    None = 0,

    /// <summary>
    /// Expose properties.
    /// </summary>
    Properties = 1,

    /// <summary>
    /// Expose methods.
    /// </summary>
    Methods = 2,

    /// <summary>
    /// Expose methods and properties.
    /// </summary>
    MethodsAndProperties = Properties | Methods,

    /// <summary>
    /// Expose events.
    /// </summary>
    Events = 4,

    /// <summary>
    /// Expose all supported types.
    /// </summary>
    All = Properties | Methods | Events,
}
