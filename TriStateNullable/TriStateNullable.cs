using System.Diagnostics;
using System.Text.Json.Serialization;

namespace TriStateNullable;

/// <summary>
/// Represents a tri-state nullable value:
/// - <see cref="TriStateNullableTag.WithValue"/>: contains a non-null value.
/// - <see cref="TriStateNullableTag.NullSerializable"/>: explicitly serializable null (will be written as null in JSON).
/// - <see cref="TriStateNullableTag.NullNotSerializable"/>: non-serializable null (will be omitted from JSON).
/// Inspired by Rust's <c>Option&lt;T&gt;</c> but with distinct null states for JSON serialization.
/// </summary>
/// <typeparam name="T">The underlying value type.</typeparam>
public struct TriStateNullable<T> : IEquatable<TriStateNullable<T>>
{
    [JsonInclude, JsonPropertyName("Value")]
    private T _value;

    /// <summary>
    /// A tri-state instance representing a serializable null value.
    /// When serialized, this will output <c>null</c>.
    /// </summary>
    public static readonly TriStateNullable<T> NullSerializable = new(TriStateNullableTag.NullSerializable);

    /// <summary>
    /// A tri-state instance representing a non-serializable null value.
    /// When serialized, this will omit the property entirely.
    /// </summary>
    public static readonly TriStateNullable<T> NullNotSerializable = new(TriStateNullableTag.NullNotSerializable);

    /// <summary>
    /// Gets the current tag/state of this instance.
    /// </summary>
    public TriStateNullableTag Tag { get; private set; }

    /// <summary>
    /// Indicates whether the value is present.
    /// </summary>
    public bool IsSome => Tag == TriStateNullableTag.WithValue;

    /// <summary>
    /// Indicates whether the value is absent (either serializable or non-serializable null).
    /// </summary>
    public bool IsNone => !IsSome;

    /// <summary>
    /// Returns this instance if it contains a value; otherwise returns the provided fallback.
    /// </summary>
    public TriStateNullable<T> Or(TriStateNullable<T> fallback) => IsSome ? this : fallback;

    /// <summary>
    /// Returns the contained value if present; otherwise throws <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if there is no value present.</exception>
    public T Unwrap() => IsSome
        ? _value
        : throw new InvalidOperationException("called `Unwrap()` on a `None` value");

    /// <summary>
    /// Returns the contained value if present; otherwise throws <see cref="InvalidOperationException"/> with the given message.
    /// </summary>
    public T Expect(string msg) => IsSome
        ? _value
        : throw new InvalidOperationException(msg);

    /// <summary>
    /// Returns the contained value if present; otherwise returns <paramref name="fallback"/>.
    /// </summary>
    public T UnwrapOr(T fallback) => IsSome ? _value : fallback;

    /// <summary>
    /// Returns the contained value if present; otherwise returns the default value for <typeparamref name="T"/>.
    /// </summary>
    public T UnwrapOrDefault() => IsSome ? _value : default;

    /// <summary>
    /// Returns the contained value if present; otherwise evaluates and returns the result of the provided <paramref name="fallback"/> function.
    /// </summary>
    public T UnwrapOrElse(Func<T> fallback) => IsSome ? _value : fallback();

    /// <summary>
    /// Executes the provided action if a value is present and non-null.
    /// </summary>
    public void IfSome(Action<T> f)
    {
        if (IsSome && _value != null)
            f(_value);
    }

    /// <inheritdoc/>
    public override string ToString() => Tag switch
    {
        TriStateNullableTag.NullNotSerializable => $"NullNotSerializable<{typeof(T)}>",
        TriStateNullableTag.NullSerializable => $"NullSerializable<{typeof(T)}>",
        TriStateNullableTag.WithValue => $"Some({_value})",
        _ => throw new UnreachableException()
    };

    /// <summary>
    /// Maps the contained value to a new value of type <typeparamref name="U"/> using the provided function.
    /// If no value is present, the result will preserve the null state.
    /// </summary>
    public TriStateNullable<U> Map<U>(Func<T, U> f) =>
        IsSome ? new TriStateNullable<U>(f(_value)) : new TriStateNullable<U>(Tag);

    /// <summary>
    /// Applies the provided function that returns another <see cref="TriStateNullable{U}"/> to the contained value, if present.
    /// If no value is present, the result will preserve the null state.
    /// </summary>
    public TriStateNullable<U> AndThen<U>(Func<T, TriStateNullable<U>> f) =>
        IsSome ? f(_value) : new TriStateNullable<U>(Tag);

    private TriStateNullable(TriStateNullableTag tag, T value = default)
    {
        _value = value;
        Tag = tag;
    }

    /// <summary>
    /// Creates a new instance from the specified value.
    /// If the value is null, the instance will be <see cref="NullSerializable"/>.
    /// </summary>
    public TriStateNullable(T? value)
    {
        if (value is null)
        {
            this = NullSerializable;
            return;
        }
        _value = value;
        Tag = TriStateNullableTag.WithValue;
    }

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/> to a <see cref="TriStateNullable{T}"/>.
    /// </summary>
    public static implicit operator TriStateNullable<T>(T? value) => new(value);

    /// <summary>
    /// Equality operator. Two instances are equal if they have the same tag and, if both contain a value, the values are equal.
    /// </summary>
    public static bool operator ==(TriStateNullable<T> x, TriStateNullable<T> y)
    {
        if (x.Tag != y.Tag) return false;
        if (x.IsNone && y.IsNone) return true;
        return x.Unwrap()!.Equals(y.Unwrap());
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(TriStateNullable<T> x, TriStateNullable<T> y) => !(x == y);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is TriStateNullable<T> other && this == other;

    /// <inheritdoc/>
    public override int GetHashCode() => Tag switch
    {
        TriStateNullableTag.WithValue => _value?.GetHashCode() ?? 0,
        _ => Tag.GetHashCode(),
    };

    /// <inheritdoc/>
    public bool Equals(TriStateNullable<T> other) =>
        EqualityComparer<T>.Default.Equals(_value, other._value) && Tag == other.Tag;
}
