public class TypedInstance<T>
{
    private readonly T _value;
    protected TypedInstance(T value) => _value = value;
    public static implicit operator T(TypedInstance<T> typedInstance) => typedInstance._value;
}