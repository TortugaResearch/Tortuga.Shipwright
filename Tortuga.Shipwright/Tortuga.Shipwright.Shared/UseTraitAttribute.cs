namespace Tortuga.Shipwright;

/// <summary>
/// Include the indicated trait in the class this attribute is used on.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class UseTraitAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UseTraitAttribute"/> class.
    /// </summary>
    /// <param name="traitType">Type of the trait.</param>
    public UseTraitAttribute(Type traitType) { TraitType = traitType; }

    /// <summary>
    /// If enabled, automatically expose members even if they don't have the ExposeAttribute.
    /// </summary>
    /// <remarks>This is used when the class being exposed is 3rd party. All members will be exposed as public.</remarks>
    public Expose AutoExpose { get; set; } = Expose.None;

    /// <summary>
    /// Gets the type of the trait.
    /// </summary>
    /// <value>The type of the trait.</value>
    public Type TraitType { get; }
}
