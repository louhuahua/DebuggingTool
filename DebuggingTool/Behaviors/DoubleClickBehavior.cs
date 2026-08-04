using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Windows.Input;
using RxVoid = ReactiveUI.Primitives.RxVoid;

namespace DebuggingTool.Behaviors;

public static class DoubleClickBehavior
{
    public static readonly AttachedProperty<ICommand> CommandProperty =
        AvaloniaProperty.RegisterAttached<Control, ICommand>(
            "Command", typeof(DoubleClickBehavior));

    static DoubleClickBehavior()
    {
        CommandProperty.Changed.Subscribe(OnCommandChanged);
    }

    private static void OnCommandChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is Control control)
        {
            control.DoubleTapped -= OnDoubleTapped;

            if (e.NewValue is ICommand command)
            {
                control.DoubleTapped += OnDoubleTapped;
            }
        }
    }

    private static void OnDoubleTapped(object sender, TappedEventArgs e)
    {
        var control = (Control)sender;
        var command = GetCommand(control);

        var parameter = RxVoid.Default;

        if (command?.CanExecute(parameter) == true)
        {
            command.Execute(parameter);
        }
    }

    public static ICommand GetCommand(Control element) =>
        element.GetValue(CommandProperty);

    public static void SetCommand(Control element, ICommand value) =>
        element.SetValue(CommandProperty, value);
}
