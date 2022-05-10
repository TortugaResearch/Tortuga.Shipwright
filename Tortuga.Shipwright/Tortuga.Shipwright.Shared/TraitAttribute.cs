namespace Tortuga.Shipwright;


/// <summary>
/// This attribute indicated that a class is meant to be used as a trait.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class TraitAttribute : Attribute
{

}
