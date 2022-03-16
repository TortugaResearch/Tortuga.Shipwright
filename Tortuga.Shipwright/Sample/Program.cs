// See https://aka.ms/new-console-template for more information

using Sample;

Console.WriteLine("Hello, World!");

var container = new MyContainer();


Console.WriteLine("My name is " + container.Name);

Console.WriteLine("My pet is named " + container.AllPets());

container.Counter += 5;
Console.WriteLine(container.Counter);
