﻿
using Tortuga.Shipwright;

namespace Sample;


[UseTrait(typeof(MyTrait))]
public partial class MyContainer : IHasCustomerKey
{
    private partial string OnGetName() { return GetType().Name; }


    string IHasPets.Pets => "Spot";

    public int CustomerKey => 5;

    private partial void SimpleAction() { }


}


[UseTrait(typeof(MyTrait))]
public partial class SimpleContainer
{
    //Note that it doesn't implement IHasCustomerKey, which the trait is looking for as an optional container type.

    private partial string OnGetName() { return GetType().Name; }


    string IHasPets.Pets => "Spot";

    private partial void SimpleAction() { }

}

public interface IHasCustomerKey
{
    int CustomerKey { get; }
}