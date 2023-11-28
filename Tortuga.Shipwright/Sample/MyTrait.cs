using System.ComponentModel;
using Tortuga.Shipwright;

namespace Sample;

public interface IMath
{
    event EventHandler<EventArgs>? ValueChanged;

    int BaseValue { get; set; }

    //Not support yet. See Task-2
    int Counter { get; set; }

    [Obsolete]
    public string IAmAlsoBad { get; }

    int Add(int a, int b);
}

public class MyTrait : IMath
{
    int baseValue;

    [Expose]
    public event EventHandler<EventArgs>? ValueChanged;

    int IMath.BaseValue
    {
        get => baseValue;
        set
        {
            baseValue = value;
            OnValueChanged();
        }
    }

    [Container(RegisterInterface = true)]
    public IHasPets Container { get; set; } = null!;

    /// <summary>
    /// Gets or sets the counter.
    /// </summary>
    /// <value>The counter.</value>
    /// <remarks>This should be copied from the trait to the container.</remarks>
    [Expose]
    public int Counter { get; set; }

    [Container(IsOptional = true)]
    public IHasCustomerKey? CustomerKeyProvider { get; set; }

    string IMath.IAmAlsoBad => throw new NotImplementedException();
    [Expose] public string Name => "Mr. " + OnGetName();

    [Obsolete()]
    [Expose]
    public bool OldMethodA { get; set; }

    [Obsolete("This is a message")]
    [Expose]
    [EditorBrowsable]
    public bool OldMethodB { get; set; }

    [Obsolete("This is a\r\n message with \"quotes\"", true)]
    [Expose]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public bool OldMethodC { get; set; }

    [Partial]
    public Func<string> OnGetName { get; set; } = null!;

    [Partial]
    public Action SimpleAction { get; set; } = null!;

    int IMath.Add(int a, int b)
    {
        throw new NotImplementedException();
    }

    [Expose]
    public string AllPets()
    {
        return "Sunshine, Flipper, and " + Container.Pets;
    }

    public void OnValueChanged() => ValueChanged?.Invoke(this, EventArgs.Empty);
}

public class SimpleTrait
{
    //Note that this doesn't use ExposeAttribute, so you need to use auto-expose instead.

    public string? Name { get; set; }

    public int GetNameLength() => Name?.Length ?? 0;
}

public interface IHasPets
{
    public string Pets { get; }
}
