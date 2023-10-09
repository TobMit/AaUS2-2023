// See https://aka.ms/new-console-template for more information

using AaUS2.BinaryVyhladavaciStrom;

Console.WriteLine("Hello, World!");

BtsTree<int> strom = new BtsTree<int>();
strom.Insert(2);
strom.Insert(3);
strom.Insert(1);
strom.Insert(6);
strom.toString();