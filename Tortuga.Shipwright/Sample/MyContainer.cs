
using Tortuga.Shipwright;

namespace Sample;


[UseTrait(typeof(MyTrait))]
public partial class MyContainer
{
    private partial string OnGetName() { return GetType().Name; }


    string IHasPets.Pets => "Spot";


    public event EventHandler<EventArgs> ValueChanged
    {
        add { __Trait0.ValueChanged += value; }
        remove { __Trait0.ValueChanged -= value; }
    }
}
