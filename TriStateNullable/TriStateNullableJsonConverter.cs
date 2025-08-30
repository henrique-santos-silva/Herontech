using System.Text.Json;
using System.Text.Json.Serialization;

namespace TriStateNullable;


public class TriStateNullableJsonConverter<T> : JsonConverter<TriStateNullable<T>>
{
    public override TriStateNullable<T> Read(ref Utf8JsonReader r, Type _, JsonSerializerOptions o) =>
        r.TokenType == JsonTokenType.Null
            ? TriStateNullable<T>.NullSerializable
            : new TriStateNullable<T>(JsonSerializer.Deserialize<T>(ref r, o));

    public override void Write(Utf8JsonWriter w, TriStateNullable<T> v, JsonSerializerOptions o)
    {
        switch (v.Tag)
        {
            case TriStateNullableTag.WithValue:
                JsonSerializer.Serialize(w, v.Unwrap(), o);
                break;
            case TriStateNullableTag.NullSerializable:
                w.WriteNullValue();
                break;
            case TriStateNullableTag.NullNotSerializable:
                // intencionalmente não escreve nada (propriedade será omitida se o valor for o default)
                break;
        }
    }
}