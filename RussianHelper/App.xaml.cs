using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace RussianHelper
{
    public partial class App : Application
    {
        private static readonly string LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "russian_helper.log");
        
        public App()
        {
            try
            {
                LogMessage("=== Russian Helper Application Starting ===");
                LogMessage($"Current Directory: {AppDomain.CurrentDomain.BaseDirectory}");
                LogMessage($"OS Version: {Environment.OSVersion}");
                LogMessage($".NET Version: {Environment.Version}");
                LogMessage($"Is 64-bit: {Environment.Is64BitProcess}");
                
                // Set up global exception handlers
                this.DispatcherUnhandledException += App_DispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                
                LogMessage("Application constructor completed successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"CRITICAL ERROR in App constructor: {ex}");
                MessageBox.Show($"Critical startup error: {ex.Message}\n\nCheck the log file for details.", 
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                LogMessage("OnStartup called");
                LogMessage($"Startup arguments: {string.Join(" ", e.Args)}");
                
                base.OnStartup(e);
                
                LogMessage("OnStartup completed successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in OnStartup: {ex}");
                MessageBox.Show($"Startup error: {ex.Message}\n\nCheck the log file for details.", 
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
        
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogMessage($"DISPATCHER UNHANDLED EXCEPTION: {e.Exception}");
            LogMessage($"Exception Source: {e.Exception.Source}");
            LogMessage($"Exception Stack Trace: {e.Exception.StackTrace}");
            
            MessageBox.Show($"An unexpected error occurred:\n\n{e.Exception.Message}\n\nCheck the log file for details.", 
                "Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            e.Handled = true;
        }
        
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            LogMessage($"DOMAIN UNHANDLED EXCEPTION: {exception}");
            if (exception != null)
            {
                LogMessage($"Exception Source: {exception.Source}");
                LogMessage($"Exception Stack Trace: {exception.StackTrace}");
            }
            
            MessageBox.Show($"A critical error occurred:\n\n{exception?.Message ?? "Unknown error"}\n\nCheck the log file for details.", 
                "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        private static void LogMessage(string message)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] {message}";
                
                File.AppendAllText(LogFile, logEntry + Environment.NewLine);
                Console.WriteLine(logEntry); // Also write to console for immediate feedback
            }
            catch
            {
                // If logging fails, at least try to write to console
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}");
            }
        }
    }
}
