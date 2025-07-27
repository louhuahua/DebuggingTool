using Avalonia.Preferences;

namespace avalonia_new.Services
{
    public static class PreferenceService
    {
        private static Preferences _preferences = new Preferences();
        public static void SetValue(string key, object value)
        {
           _preferences.Set(key, value);
        }
        public static T? GetValue<T>(string key)
        {
            if (_preferences.ContainsKey(key))
            {
                return _preferences.Get<T>(key, default);
            }
            return default;
        }

    }
}
