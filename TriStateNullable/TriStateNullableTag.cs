namespace TriStateNullable;

public enum TriStateNullableTag : byte
{
    NullNotSerializable = 0, // default
    NullSerializable,
    WithValue
}