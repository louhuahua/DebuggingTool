namespace Station.Hardware.PLC;

public class Point<T> : PointBase
{
    public T Value { get; set; }
    // 字符串编码设置（仅当T为string时有效）
    public StringEncoding Encoding { get; set; } = StringEncoding.ASCII;
    public override Type DataType => typeof(T);
    public Func<Point<T>, T, Task>? OnValueChanged { get; set; }
    public Func<Point<T>, T, Task>? TriggerAction { get; set; }


    public override void SetValue(object value)
    {
        var type = typeof(T);

        if (type == typeof(bool))
        {
            bool boolValue = Convert.ToBoolean(value);
            TriggerAction?.Invoke(this, (T)(object)boolValue);
            return;
        }

        if (value is T tValue)
        {
            if (TriggerAction != null)
                TriggerAction(this, tValue);
        }
        else
        {
            throw new InvalidCastException(
                $"Cannot cast value of type {value.GetType()} to {typeof(T)}"
            );
        }
    }
}
