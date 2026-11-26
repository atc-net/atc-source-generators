namespace Atc.SourceGenerators.Generators.Internal;

/// <summary>
/// An immutable array with value equality, for incremental generator models.
/// <see cref="ImmutableArray{T}"/> compares by reference, so a model holding one never compares equal
/// across generator runs and every step that produces it re-runs.
/// </summary>
/// <typeparam name="T">The element type; it must have value equality itself.</typeparam>
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Factory for an empty value")]
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    private readonly T[]? items;

    public EquatableArray(IEnumerable<T> items)
        => this.items = items.ToArray();

    public static EquatableArray<T> Empty => default;

    public int Count => items?.Length ?? 0;

    public T this[int index] => items![index];

    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right)
        => left.Equals(right);

    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right)
        => !left.Equals(right);

    public bool Equals(EquatableArray<T> other)
    {
        var left = items ?? [];
        var right = other.items ?? [];
        if (left.Length != right.Length)
        {
            return false;
        }

        for (var i = 0; i < left.Length; i++)
        {
            if (!left[i].Equals(right[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
        => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        // HashCode.Combine is not available on netstandard2.0
        unchecked
        {
            var hash = 17;
            foreach (var item in items ?? [])
            {
                hash = (hash * 31) + item.GetHashCode();
            }

            return hash;
        }
    }

    public IEnumerator<T> GetEnumerator()
        => ((IEnumerable<T>)(items ?? [])).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();
}