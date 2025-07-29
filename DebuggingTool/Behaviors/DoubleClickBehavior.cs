using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Reactive;
using System.Windows.Input;

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

        if (command?.CanExecute(control.DataContext) == true)
        {
            //command.Execute(control.DataContext);
            command.Execute(Unit.Default);
        }
    }

    public static ICommand GetCommand(Control element) =>
        element.GetValue(CommandProperty);

    public static void SetCommand(Control element, ICommand value) =>
        element.SetValue(CommandProperty, value);
}
