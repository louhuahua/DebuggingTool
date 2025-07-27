using Avalonia.Media.Immutable;
using Avalonia.Media;

namespace avalonia_new.Services
{
    public static  class ResourceService
    {
        public static IImmutableSolidColorBrush GetSolidColorFromKey(string key)
        {
            object? value;
            var found = App.Current.TryGetResource(key, App.Current.ActualThemeVariant, out value);
            if (found)
            {
                return (ImmutableSolidColorBrush)value;
            }
            return new ImmutableSolidColorBrush(Colors.Black);
        }

    }
}
