using MCModInstaller.Forms;
using MCModInstaller.Utilities;

namespace MCModInstaller;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Set up global exception handlers
        Application.ThreadException += Application_ThreadException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        // Configure application
        ApplicationConfiguration.Initialize();

        // Run the main form
        Application.Run(new MainForm());
    }

    private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
    {
        ShowErrorMessage(e.Exception);
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            ShowErrorMessage(ex);
        }
    }

    private static void ShowErrorMessage(Exception ex)
    {
        var message = $"{Constants.ErrorUnknown}\n\nDétails: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
        MessageBox.Show(
            message,
            Constants.ErrorTitle,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}