using DebuggingTool.Views;
using System.Resources;

namespace DebuggingTool.Helper
{
    public static class ResourceHelper
    {
        static readonly ResourceManager resourceManager = new ResourceManager("DebuggingTool.Assets.Lang.Resources", typeof(MainWindow).Assembly);
        public static string GetResourceString(string resourceString)
        {
            return resourceManager.GetString(resourceString) ?? string.Empty;
        }

    }
}
