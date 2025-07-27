using avalonia_new.Views;
using System.Resources;

namespace avalonia_new.Helper
{
    public static class ResourceHelper
    {
        static readonly ResourceManager resourceManager = new ResourceManager("avalonia_new.Assets.Lang.Resources", typeof(MainWindow).Assembly);
        public static string GetResourceString(string resourceString)
        {
            return resourceManager.GetString(resourceString) ?? string.Empty;
        }

    }
}
