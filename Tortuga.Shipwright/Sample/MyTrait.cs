using Tortuga.Shipwright;

namespace Sample;

public class MyTrait : IMath
{
    [Expose] public string Name => "Mr. " + OnGetName();

    [Partial]
    public Func<string> OnGetName { get; set; } = null!;

    [Expose]
    public int Counter { get; set; }

    int IMath.Add(int a, int b)
    {
        throw new NotImplementedException();
    }

    [Container(RegisterInterface = true)]
    public IHasPets Container { get; set; } = null!;

    [Expose]
    public string AllPets()
    {
        return "Sunshine, Flipper, and " + Container.Pets;
    }
}


public interface IMath
{
    int Add(int a, int b);


    //Not support yet. See Task-2
    //int Counter { get; set; }
}


public interface IHasPets
{
    public string Pets { get; }
}