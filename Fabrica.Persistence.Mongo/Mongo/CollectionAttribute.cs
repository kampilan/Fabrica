﻿namespace Fabrica.Persistence.Mongo;


[AttributeUsage(AttributeTargets.Class)]
public class CollectionAttribute: Attribute
{

    public CollectionAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }

}