# Avalonia Boilerplate

Welcome to the Avalonia Boilerplate repository! This project serves as a starter template for building mobile applications using Avalonia. It is designed to help beginners set up a mobile application environment with ease, providing essential features and a clean architecture based on the MVVM pattern with Prism.Avalonia. This boilerplate includes navigation, font icons, SVG support, and a toast notification service.

## Features

- **MVVM Pattern with Prism.Avalonia**: Clean separation of concerns with the MVVM architecture.
- **Navigation**: Easy and intuitive navigation system.
- **Font Icons**: Support for font icons to enhance the UI.
- **SVG Support**: Integration for SVG images.
- **Toast Service**: Built-in service for displaying toast notifications.

## Getting Started

Follow these steps to get your project up and running.

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Visual Studio](https://visualstudio.microsoft.com/vs/) or [Visual Studio Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository:**

    ```bash
    git clone git@github.com:exendahal/avalonia_new.git
    cd avalonia_new
    ```

2. **Restore dependencies:**

    ```bash
    dotnet restore
    ```

3. **Build the project:**

    ```bash
    dotnet build
    ```

4. **Run the application:**

    ```bash
    dotnet run
    ```

## Project Structure

- **/avalonia_new**: Contains the main application code.
  - **/Views**: XAML files for UI components.
  - **/ViewModels**: C# files for view models.
  - **/Models**: Data models.
  - **/Services**: Implementation of services (e.g., toast notifications).
  - **/Resources**: Contains fonts, SVGs, and other resources.

## Using the MVVM Pattern

This boilerplate follows the MVVM pattern using Prism.Avalonia. Here's a brief overview:

### ViewModel

Create your ViewModels in the `/ViewModels` directory. Here's an example of a simple ViewModel:

```csharp
public class MainViewModel : BindableBase
{
    private string _title = "Hello, Avalonia!";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
}
```

### View

Create your Views in the `/Views` directory and bind them to ViewModels. Here's an example of a simple View:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             mc:Ignorable="d">
  <TextBlock Text="{Binding Title}" />
</UserControl>
```

### Navigation

Use Prism's navigation service to handle navigation between views.

### Font Icons and SVG Support

Add your font icons and SVG images to the `/Resources` directory and reference them in your views.

### Toast Service

To use the toast service, call the ShowToast method with a message and toast type. Available toast types are Success, Warning, and Failed:

```csharp
ToastHelper.ShowToast("Welcome", Services.ToastService.ToastType.Success);
```

## Acknowledgments

- [Avalonia](https://avaloniaui.net/)
- [Prism.Avalonia](https://github.com/AvaloniaCommunity/Prism.Avalonia)

---

Thank you for using the Avalonia Mobile Boilerplate! If you have any questions or feedback, feel free to open an issue or submit a pull request. Happy coding!