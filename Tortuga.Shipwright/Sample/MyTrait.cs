using System.ComponentModel;
using Tortuga.Shipwright;

namespace Sample;

public class MyTrait : IMath
{
    [Expose] public string Name => "Mr. " + OnGetName();

    [Partial]
    public Func<string> OnGetName { get; set; } = null!;


    /// <summary>
    /// Gets or sets the counter.
    /// </summary>
    /// <value>The counter.</value>
    /// <remarks>This should be copied from the trait to the container.</remarks>
    [Expose]
    public int Counter { get; set; }

    int IMath.Add(int a, int b)
    {
        throw new NotImplementedException();
    }

    [Container(RegisterInterface = true)]
    public IHasPets Container { get; set; } = null!;

    int baseValue;
    int IMath.BaseValue
    {
        get => baseValue;
        set
        {
            baseValue = value;
            OnValueChanged();
        }
    }

    [Expose]
    public string AllPets()
    {
        return "Sunshine, Flipper, and " + Container.Pets;
    }

    [Expose]
    public event EventHandler<EventArgs>? ValueChanged;

    public void OnValueChanged() => ValueChanged?.Invoke(this, EventArgs.Empty);


    [Container(IsOptional = true)]
    public IHasCustomerKey? CustomerKeyProvider { get; set; }

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
}


public interface IMath
{
    int Add(int a, int b);


    //Not support yet. See Task-2
    int Counter { get; set; }

    int BaseValue { get; set; }

    event EventHandler<EventArgs>? ValueChanged;

}


public interface IHasPets
{
    public string Pets { get; }

}