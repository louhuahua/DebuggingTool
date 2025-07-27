using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using avalonia_new.Helper;
using System.Windows.Input;

namespace avalonia_new.Controls
{
    public partial class ToastMessage : UserControl
    {
        public static readonly StyledProperty<string> StatusTextProperty =
            AvaloniaProperty.Register<ToastMessage, string>(nameof(StatusText), defaultValue: ResourceHelper.GetResourceString("Error"));

        public static readonly StyledProperty<string> MessageTextProperty =
            AvaloniaProperty.Register<ToastMessage, string>(nameof(MessageText), defaultValue: ResourceHelper.GetResourceString("SomethingWentWrong"));

        public static readonly StyledProperty<ISolidColorBrush> TextColorProperty =
     AvaloniaProperty.Register<ToastMessage, ISolidColorBrush>(nameof(TextColor), defaultValue: new SolidColorBrush(Colors.Black));


        public static readonly StyledProperty<string> FontIconProperty =
            AvaloniaProperty.Register<ToastMessage, string>(nameof(FontIconText), defaultValue: "\uf159");

        public static readonly StyledProperty<ICommand> CloseCommandProperty =
         AvaloniaProperty.Register<ToastMessage, ICommand>(nameof(CloseCommand));

        public ICommand CloseCommand
        {
            get => GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }
        public string StatusText
        {
            get => GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }

        public string MessageText
        {
            get => GetValue(MessageTextProperty);
            set => SetValue(MessageTextProperty, value);
        }

        public ISolidColorBrush TextColor
        {
            get => GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public string FontIconText
        {
            get => GetValue(FontIconProperty);
            set => SetValue(FontIconProperty, value);
        }
        public ToastMessage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
