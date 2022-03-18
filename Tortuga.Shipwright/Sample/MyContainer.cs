
using Tortuga.Shipwright;

namespace Sample;


[UseTrait(typeof(MyTrait))]
public partial class MyContainer
{
    private partial string OnGetName() { return GetType().Name; }


    string IHasPets.Pets => "Spot";

}
