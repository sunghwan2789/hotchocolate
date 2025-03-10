namespace Server;

[ObjectType<Foo>]
public static partial class FooNode
{
    public static Foo GetFoo() => new();
}
