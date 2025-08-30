using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Linq; // Added for FirstOrDefault

namespace RussianHelper
{
    public partial class GlobalPopupWindow : Window
    {
        private readonly RussianLanguageProcessor _processor;
        private readonly TranslationService _translationService;
        private readonly OCRService _ocrService;
        private bool _isProcessing = false;
        private System.Windows.Threading.DispatcherTimer _autoHideTimer;

        public GlobalPopupWindow()
        {
            InitializeComponent();
            
            _processor = new RussianLanguageProcessor();
            _translationService = new TranslationService();
            _ocrService = new OCRService();
            
            // Set up auto-hide timer
            _autoHideTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10) // Auto-hide after 10 seconds
            };
            _autoHideTimer.Tick += AutoHideTimer_Tick;
            
            // Set up mouse events to pause/resume timer
            this.MouseEnter += GlobalPopupWindow_MouseEnter;
            this.MouseLeave += GlobalPopupWindow_MouseLeave;
            
            // Set up window behavior - remove auto-hide on deactivation
            // this.Deactivated += GlobalPopupWindow_Deactivated;
        }

        public async Task ShowTranslationAtPositionAsync(System.Drawing.Point screenPosition, string? recognizedText = null)
        {
            if (_isProcessing) return;
            _isProcessing = true;

            try
            {
                // Position the window near the mouse but ensure it's visible on screen
                PositionWindow(screenPosition);
                
                // Show loading state
                ShowLoadingState();
                this.Show();
                
                // Start auto-hide timer
                _autoHideTimer.Stop();
                _autoHideTimer.Start();

                string russianText = recognizedText;
                
                // If no text provided, use OCR to recognize text from screen area
                if (string.IsNullOrEmpty(russianText))
                {
                    russianText = await _ocrService.RecognizeTextFromScreenAreaAsync(screenPosition);
                }

                // Extract the first Russian word
                var words = russianText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var russianWord = words.FirstOrDefault(word => _processor.IsRussianWord(word));

                if (string.IsNullOrEmpty(russianWord))
                {
                    ShowError("No Russian text found in this area");
                    return;
                }

                // Update popup with recognized word
                PopupWord.Text = russianWord;
                PopupPronunciation.Text = $"[{_processor.GetPronunciation(russianWord)}]";

                // Get translation from DeepSeek API
                var translation = await _translationService.GetTranslationAsync(russianWord);

                // Update popup with translation results
                if (translation.IsLoading)
                {
                    ShowError("Translation is still loading...");
                }
                else if (!string.IsNullOrEmpty(translation.Error))
                {
                    ShowError($"Translation error: {translation.Error}");
                }
                else
                {
                    ShowTranslation(translation);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private void PositionWindow(System.Drawing.Point screenPosition)
        {
            // Get screen dimensions
            var screenBounds = Screen.PrimaryScreen.Bounds;
            
            // Calculate window position (offset from mouse)
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            
            double left = screenPosition.X + 20; // Offset from mouse
            double top = screenPosition.Y - windowHeight - 20; // Above mouse
            
            // Ensure window stays on screen
            if (left + windowWidth > screenBounds.Width)
            {
                left = screenPosition.X - windowWidth - 20; // Move to left of mouse
            }
            
            if (top < 0)
            {
                top = screenPosition.Y + 20; // Move below mouse
            }
            
            if (left < 0) left = 10;
            if (top < 0) top = 10;
            
            this.Left = left;
            this.Top = top;
        }

        private void ShowLoadingState()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            ContentPanel.Visibility = Visibility.Collapsed;
            PopupError.Visibility = Visibility.Collapsed;
            
            PopupTranslation.Text = "Loading...";
            PopupExamples.Text = "Please wait...";
        }

        private void ShowTranslation(TranslationResult translation)
        {
            LoadingPanel.Visibility = Visibility.Collapsed;
            ContentPanel.Visibility = Visibility.Visible;
            PopupError.Visibility = Visibility.Collapsed;
            
            PopupTranslation.Text = translation.Meaning;
            PopupExamples.Text = translation.Examples;
        }

        private void ShowError(string errorMessage)
        {
            LoadingPanel.Visibility = Visibility.Collapsed;
            ContentPanel.Visibility = Visibility.Collapsed;
            PopupError.Visibility = Visibility.Visible;
            
            PopupError.Text = errorMessage;
        }

        private void GlobalPopupWindow_Deactivated(object sender, EventArgs e)
        {
            // Don't auto-hide - let user close manually
            // this.Hide();
        }

        private void AutoHideTimer_Tick(object sender, EventArgs e)
        {
            _autoHideTimer.Stop();
            this.Hide();
        }

        private void GlobalPopupWindow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Pause timer when mouse enters
            _autoHideTimer.Stop();
        }

        private void GlobalPopupWindow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Resume timer when mouse leaves (with shorter duration)
            _autoHideTimer.Interval = TimeSpan.FromSeconds(3);
            _autoHideTimer.Start();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _autoHideTimer.Stop();
            this.Hide();
        }

        protected override void OnClosed(EventArgs e)
        {
            _ocrService?.Dispose();
            base.OnClosed(e);
        }
    }
}
