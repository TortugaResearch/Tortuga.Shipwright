namespace Tortuga.Shipwright
{
    /// <summary>
    /// Indicates that this property should contribute to equality checks.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class EqualityKeyAttribute : Attribute
    {

    }
}