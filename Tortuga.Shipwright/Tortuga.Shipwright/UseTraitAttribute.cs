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
    /// Gets the type of the trait.
    /// </summary>
    /// <value>The type of the trait.</value>
    public Type TraitType { get; }
}
