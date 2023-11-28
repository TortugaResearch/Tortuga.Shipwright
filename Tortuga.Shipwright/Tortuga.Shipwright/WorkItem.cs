using Microsoft.CodeAnalysis;

namespace Tortuga.Shipwright;

class AnnotatedTraitClass
{
    public AnnotatedTraitClass(INamedTypeSymbol traitClass, Expose autoExpose)
    {
        TraitClass = traitClass;
        AutoExpose = autoExpose;
    }

    public Expose AutoExpose { get; }
    public INamedTypeSymbol TraitClass { get; }
}

class WorkItem
{
    public WorkItem(INamedTypeSymbol hostingClass)
    {
        ContainerClass = hostingClass ?? throw new ArgumentNullException(nameof(hostingClass));
    }

    public INamedTypeSymbol ContainerClass { get; }
    public HashSet<AnnotatedTraitClass> TraitClasses { get; } = new(AnnotatedTraitClassComparer.Default);
}

class AnnotatedTraitClassComparer : IEqualityComparer<AnnotatedTraitClass>
{
    public static AnnotatedTraitClassComparer Default = new AnnotatedTraitClassComparer();

    public bool Equals(AnnotatedTraitClass x, AnnotatedTraitClass y)
    {
        return SymbolEqualityComparer.Default.Equals(x.TraitClass, y.TraitClass);
    }

    public int GetHashCode(AnnotatedTraitClass obj)
    {
        return SymbolEqualityComparer.Default.GetHashCode(obj.TraitClass);
    }
}
