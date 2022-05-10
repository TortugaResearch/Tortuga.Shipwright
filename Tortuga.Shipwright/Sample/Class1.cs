//using System;

//namespace Tortuga.Shipwright;

///// <summary>
///// When exposing a method, event, or property, this enumeration indicates what accessibility to use.
///// </summary>
//[Flags]
//public enum Accessibility
//{
//    /// <summary>
//    /// Apply the `public` modifier to the exposed member.
//    /// </summary>
//    Public = 0,

//    /// <summary>
//    /// Apply the `protected` modifier to the exposed member.
//    /// </summary>
//    Protected = 1,

//    /// <summary>
//    /// Apply the `internal` modifier to the exposed member.
//    /// </summary>
//    Internal = 2,

//    /// <summary>
//    /// Apply the `protected internal` modifier to the exposed member.
//    /// </summary>
//    ProtectedOrInternal = 3,

//    /// <summary>
//    /// Apply the `private` modifier to the exposed member.
//    /// </summary>
//    Private = 4,
//}

//[Flags]
//public enum Getter
//{
//    /// <summary>
//    /// No additional modifier is applied to the getter.
//    /// </summary>
//    None = 0,

//    /// <summary>
//    /// Apply the `protected` modifier to the getter.
//    /// </summary>
//    Protected = 1,

//    /// <summary>
//    /// Apply the `internal` modifier to the getter.
//    /// </summary>
//    Internal = 2,

//    /// <summary>
//    /// Apply the `protected internal` modifier to the getter.
//    /// </summary>
//    ProtectedOrInternal = 3,

//    /// <summary>
//    /// Apply the `private` modifier to the getter.
//    /// </summary>
//    Private = 4,
//}

///// <summary>
///// When exposing a property getter, this enumeration indicates what accessibility to use.
///// </summary>
///// <summary>
///// When exposing a method, event, or property, this enumeration indicates which inheritance modifies to use.
///// </summary>
//[Flags]
//public enum Inheritance
//{
//    /// <summary>
//    /// No additional modifier is applied.
//    /// </summary>
//    None = 0,

//    /// <summary>
//    /// Apply the `virtual` modifier to the exposed member.
//    /// </summary>
//    Virtual = 1,

//    /// <summary>
//    /// Apply the `override` modifier to the exposed member.
//    /// </summary>
//    Override = 2,

//    /// <summary>
//    /// Apply the `abstract` modifier to the exposed member.
//    /// </summary>
//    Abstract = 4,

//    /// <summary>
//    /// Apply the `abstract override` modifier to the exposed member.
//    /// </summary>
//    AbstractOverride = Abstract | Override,

//    /// <summary>
//    /// Apply the `sealed` modifier to the exposed member.
//    /// </summary>
//    Sealed = 8,

//    /// <summary>
//    /// Apply the `sealed override` modifier to the exposed member.
//    /// </summary>
//    SealedOverride = Sealed + Override
//}

///// <summary>
///// When exposing a property setter, this enumeration indicates what accessibility to use.
///// </summary>
//[Flags]
//public enum Setter
//{
//    /// <summary>
//    /// No additional modifier is applied to the setter.
//    /// </summary>
//    None = 0,

//    /// <summary>
//    /// Apply the `protected` modifier to the setter.
//    /// </summary>
//    Protected = 1,

//    /// <summary>
//    /// Apply the `internal` modifier to the setter.
//    /// </summary>
//    Internal = 2,

//    /// <summary>
//    /// Apply the `protected internal` modifier to the setter.
//    /// </summary>
//    ProtectedOrInternal = 3,

//    /// <summary>
//    /// Apply the `private` modifier to the setter.
//    /// </summary>
//    Private = 4,

//    /// <summary>
//    /// The setter is exposed as `init`. This may be combined another accessibility modifier.
//    /// </summary>
//    Init = 8
//}

///// <summary>
///// When placed on a property, that property is automatically set to the container of the trait object.
///// </summary>
//[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
//public class ContainerAttribute : Attribute
//{
//    /// <summary>
//    /// If set to true, the container isn't required to implement the indicated interface. When that happens, a null be assigned to the property.
//    /// </summary>
//    public bool IsOptional { get; set; }

//    /// <summary>
//    /// If set to true and the property's type is an interface, then that interface will be added to the owning class.
//    /// </summary>
//    public bool RegisterInterface { get; set; }
//}

///// <summary>
///// Expose the method, event, or property on the class using this trait.
///// </summary>
//[AttributeUsage(AttributeTargets.Method | AttributeTargets.Event | AttributeTargets.Property, AllowMultiple = false)]
//public class ExposeAttribute : Attribute
//{
//    /// <summary>
//    /// Sets the accessibility for a given method, event, or property when exposed as a trait.
//    /// </summary>
//    /// <remarks>This defaults to Public</remarks>
//    public Accessibility Accessibility { get; set; } = Accessibility.Public;

//    /// <summary>
//    /// Sets the accessibility the property's getter.
//    /// </summary>
//    /// <remarks>This is ignored if the property is write-only or used on a non-property member.</remarks>
//    public Getter Getter { get; set; } = Getter.None;

//    /// <summary>
//    /// Sets the inheritance rule for a given method, event, or property when exposed as a trait.
//    /// </summary>
//    public Inheritance Inheritance { get; set; } = Inheritance.None;

//    /// <summary>
//    /// Sets the accessibility the property's setter. May also be used to indicate a property is `init`.
//    /// </summary>
//    /// <remarks>This is ignored if the property is read-only or used on a non-property member.</remarks>
//    public Setter Setter { get; set; } = Setter.None;
//}

///// <summary>
///// Generate a partial method on the class using this trait and attach it to this delegate property.
///// </summary>
///// <remarks>This may only be used on properties of type Action or Func</remarks>
//[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
//public class PartialAttribute : Attribute
//{
//    /// <summary>
//    /// Initializes a new instance of the <see cref="PartialAttribute"/> class.
//    /// </summary>
//    /// <param name="parameterNames">A comma separated list of parameter names to use in the generated partial method.</param>
//    public PartialAttribute(string parameterNames = "")

//    {
//        ParameterNames = parameterNames;
//    }

//    /// <summary>
//    /// Gets the comma separated list of parameter names to use in the generated partial method.
//    /// </summary>
//    public string ParameterNames { get; }
//}

///// <summary>
///// Include the indicated trait in the class this attribute is used on.
///// </summary>
//[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
//public class UseTraitAttribute : Attribute
//{
//    /// <summary>
//    /// Initializes a new instance of the <see cref="UseTraitAttribute"/> class.
//    /// </summary>
//    /// <param name="traitType">Type of the trait.</param>
//    public UseTraitAttribute(Type traitType) { TraitType = traitType; }

//    /// <summary>
//    /// Gets the type of the trait.
//    /// </summary>
//    /// <value>The type of the trait.</value>
//    public Type TraitType { get; }
//}
