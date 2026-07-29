using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DebuggingTool.PLC;

public class Point<T> : PointBase
{
    public T Value { get; set; } = default!;

    // 字符串编码设置（仅当T为string时有效）
    public StringEncoding Encoding { get; set; } = StringEncoding.ASCII;

    public override Type DataType => typeof(T);

    public T OldValue { get; set; } = default!;

    public Func<Point<T>, T, Task>? OnValueChanged { get; set; }

    public Func<Point<T>, T, Task>? UpdateAction { get; set; }

    public Func<Point<T>, T, Task>? ChangedAction { get; set; }

    public override void SetValue(object value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        if (!TryConvertValue(value, out T convertedValue))
        {
            throw new InvalidCastException(
                $"Cannot cast value of type {value.GetType()} to {typeof(T)}"
            );
        }

        var previousValue = OldValue;
        Value = convertedValue;
        _ = InvokeCallbacksAsync(convertedValue, previousValue);
        OldValue = convertedValue;
    }

    private static bool TryConvertValue(object value, out T convertedValue)
    {
        if (value is T typedValue)
        {
            convertedValue = typedValue;
            return true;
        }

        if (typeof(T) == typeof(bool))
        {
            convertedValue = (T)(object)Convert.ToBoolean(value);
            return true;
        }

        convertedValue = default!;
        return false;
    }

    private async Task InvokeCallbacksAsync(T value, T previousValue)
    {
        if (OnValueChanged != null)
        {
            await OnValueChanged(this, value).ConfigureAwait(false);
        }

        if (!OnlySubscribeMode && HandleUpdate && UpdateAction != null)
        {
            await UpdateAction(this, value).ConfigureAwait(false);
        }

        if (
            HandleChange
            && ChangedAction != null
            && !EqualityComparer<T>.Default.Equals(value, previousValue)
        )
        {
            await ChangedAction(this, value).ConfigureAwait(false);
        }
    }
}
