using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Demo5;

public static class SnackbarServiceExtensions
{
    public static void ShowPrimary(this ISnackbarService snackbarService, string title, string message, int seconds = 2, IconElement? icon = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            snackbarService.Show(title, message, ControlAppearance.Primary, null, TimeSpan.FromSeconds(seconds));
        });
    }

    public static void ShowSecondary(this ISnackbarService snackbarService, string title, string message, int seconds = 2, IconElement? icon = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            snackbarService.Show(title, message, ControlAppearance.Secondary, null, TimeSpan.FromSeconds(seconds));
        });
    }

    public static void ShowInfo(this ISnackbarService snackbarService, string title, string message, int seconds = 2, IconElement? icon = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            snackbarService.Show(title, message, ControlAppearance.Info, null, TimeSpan.FromSeconds(seconds));
        });
    }

    public static void ShowDark(this ISnackbarService snackbarService, string title, string message, int seconds = 2, IconElement? icon = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            snackbarService.Show(title, message, ControlAppearance.Dark, null, TimeSpan.FromSeconds(seconds));

        });
    }

    public static void ShowLight(this ISnackbarService snackbarService, string title, string message, int seconds = 2, IconElement? icon = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            snackbarService.Show(title, message, ControlAppearance.Light, null, TimeSpan.FromSeconds(seconds));
        });
    }

    public static void ShowDanger(this ISnackbarService snackbarService, string title, string message, int seconds = 2, IconElement? icon = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            snackbarService.Show(title, message, ControlAppearance.Danger, null, TimeSpan.FromSeconds(seconds));
        });
    }

    public static void ShowSuccess(this ISnackbarService snackbarService, string title, string message, int seconds = 2, IconElement? icon = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            snackbarService.Show(title, message, ControlAppearance.Success, null, TimeSpan.FromSeconds(seconds));
        });
    }

    public static void ShowCaution(this ISnackbarService snackbarService, string title, string message, int seconds = 2, IconElement? icon = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            snackbarService.Show(title, message, ControlAppearance.Caution, null, TimeSpan.FromSeconds(seconds));
        });
    }

    public static void ShowTransparent(this ISnackbarService snackbarService, string title, string message, int seconds = 2, IconElement? icon = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            snackbarService.Show(title, message, ControlAppearance.Transparent, null, TimeSpan.FromSeconds(seconds));
        });
    }
}