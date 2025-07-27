using System.Globalization;

namespace avalonia_new.Services
{
    public static class LanguageManager
    {
        public static void ChangeLanguage(string languageCode)
        {
            if (!string.IsNullOrWhiteSpace(languageCode))
            {
                Assets.Lang.Resources.Culture = new CultureInfo(languageCode);
            }
        }
    }
}
