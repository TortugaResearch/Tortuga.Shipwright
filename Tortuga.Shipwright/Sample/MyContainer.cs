using Tortuga.Shipwright;

namespace Sample;

[UseTrait(typeof(SimpleTrait), AutoExpose = Expose.MethodsAndProperties)]
public partial class MyAutoContainer : IHasCustomerKey
{
    public int CustomerKey => 15;
}

[UseTrait(typeof(MyTrait))]
public partial class MyContainer : IHasCustomerKey
{
    public int CustomerKey => 5;

    string IHasPets.Pets => "Spot";

    private partial string OnGetName()
    {
        return GetType().Name;
    }

    private partial void SimpleAction()
    {
    }
}

[UseTrait(typeof(MyTrait))]
public partial class SimpleContainer
{
    //Note that it doesn't implement IHasCustomerKey, which the trait is looking for as an optional container type.

    string IHasPets.Pets => "Spot";

    private partial string OnGetName()
    {
        return GetType().Name;
    }

    private partial void SimpleAction()
    {
    }
}

public interface IHasCustomerKey
{
    int CustomerKey { get; }
}
