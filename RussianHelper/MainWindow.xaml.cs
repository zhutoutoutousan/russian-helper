using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

namespace RussianHelper
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _hoverTimer;
        private readonly RussianLanguageProcessor _processor;
        private readonly TranslationService _translationService;
        private readonly GlobalHookService _globalHook;
        private readonly GlobalPopupWindow _globalPopup;
        private readonly System.Windows.Threading.DispatcherTimer _globalHoverTimer;
        private System.Drawing.Point _lastMousePosition;
        private string _currentWord = "";
        private bool _isFlashcardMode = false;
        private int _score = 0;
        private int _currentCardIndex = 0;
        private List<string> _flashcardWords = new();
        private bool _answerShown = false;
        private bool _globalHookEnabled = false;
        
        private static readonly string LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "russian_helper.log");

        public MainWindow()
        {
            try
            {
                LogMessage("MainWindow constructor starting");
                
                InitializeComponent();
                LogMessage("InitializeComponent completed");
                
                _hoverTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(500)
                };
                _hoverTimer.Tick += HoverTimer_Tick;
                LogMessage("Hover timer initialized");
                
                _processor = new RussianLanguageProcessor();
                LogMessage("RussianLanguageProcessor initialized");
                
                _translationService = new TranslationService();
                LogMessage("TranslationService initialized");
                
                _globalPopup = new GlobalPopupWindow();
                LogMessage("GlobalPopupWindow initialized");
                
                _globalHoverTimer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(1000) // 1 second hover threshold
                };
                _globalHoverTimer.Tick += GlobalHoverTimer_Tick;
                
                _globalHook = new GlobalHookService(
                    (point) => OnGlobalHover(point), 
                    (point) => OnGlobalMouseMove(point));
                LogMessage("GlobalHookService initialized");
                
                InitializeFlashcards();
                LogMessage("Flashcards initialized");
                
                UpdateWordCount();
                LogMessage("Word count updated");
                
                LogMessage("MainWindow constructor completed successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in MainWindow constructor: {ex}");
                MessageBox.Show($"MainWindow initialization error: {ex.Message}\n\nCheck the log file for details.", 
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void InitializeFlashcards()
        {
            try
            {
                LogMessage("Initializing flashcards");
                _flashcardWords = new List<string>
                {
                    "привет", "здравствуйте", "спасибо", "пожалуйста", "извините",
                    "да", "нет", "хорошо", "плохо", "большой", "маленький",
                    "красивый", "умный", "добрый", "новый", "старый", "молодой",
                    "холодный", "горячий", "быстрый", "медленный", "дорогой",
                    "дешевый", "легкий", "тяжелый", "счастливый", "грустный",
                    "веселый", "серьезный", "важный", "интересный", "сложный",
                    "простой", "правильный", "неправильный", "возможный",
                    "невозможный", "свободный", "занятый", "готовый", "готов"
                };
                LogMessage($"Flashcards initialized with {_flashcardWords.Count} words");
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR initializing flashcards: {ex}");
                throw;
            }
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                UpdateWordCount();
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in InputTextBox_TextChanged: {ex}");
            }
        }

        private void UpdateWordCount()
        {
            try
            {
                var words = InputTextBox.Text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                WordCountText.Text = $"Words: {words.Length}";
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in UpdateWordCount: {ex}");
            }
        }

        private void InputTextBox_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var position = e.GetPosition(InputTextBox);
                var word = GetWordAtPosition(position);
                
                if (!string.IsNullOrEmpty(word) && _processor.IsRussianWord(word))
                {
                    if (word != _currentWord)
                    {
                        _currentWord = word;
                        _hoverTimer.Stop();
                        _hoverTimer.Start();
                    }
                }
                else
                {
                    _hoverTimer.Stop();
                    HoverPopup.IsOpen = false;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in InputTextBox_MouseMove: {ex}");
            }
        }

        private void InputTextBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var position = e.GetPosition(InputTextBox);
                var word = GetWordAtPosition(position);
                
                if (!string.IsNullOrEmpty(word) && _processor.IsRussianWord(word))
                {
                    _currentWord = word;
                    _ = ShowTranslationPopup();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in InputTextBox_MouseRightButtonDown: {ex}");
            }
        }

        private void OnGlobalMouseMove(System.Drawing.Point screenPosition)
        {
            try
            {
                // Check if mouse has moved significantly
                var distance = Math.Sqrt(Math.Pow(screenPosition.X - _lastMousePosition.X, 2) + 
                                       Math.Pow(screenPosition.Y - _lastMousePosition.Y, 2));
                
                if (distance > 5) // Mouse moved more than 5 pixels
                {
                    _globalHoverTimer.Stop();
                    _lastMousePosition = screenPosition;
                    _globalHoverTimer.Start();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in OnGlobalMouseMove: {ex}");
            }
        }

        private void OnGlobalHover(System.Drawing.Point screenPosition)
        {
            try
            {
                LogMessage($"Global hover detected at: {screenPosition.X}, {screenPosition.Y}");
                _ = _globalPopup.ShowTranslationAtPositionAsync(screenPosition);
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in OnGlobalHover: {ex}");
            }
        }

        private void GlobalHoverTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                _globalHoverTimer.Stop();
                OnGlobalHover(_lastMousePosition);
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in GlobalHoverTimer_Tick: {ex}");
            }
        }

        private void InputTextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                _hoverTimer.Stop();
                HoverPopup.IsOpen = false;
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in InputTextBox_MouseLeave: {ex}");
            }
        }

        private void HoverTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                _hoverTimer.Stop();
                _ = ShowTranslationPopup();
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in HoverTimer_Tick: {ex}");
            }
        }

        private string GetWordAtPosition(System.Windows.Point position)
        {
            try
            {
                var textBox = InputTextBox;
                var index = textBox.GetCharacterIndexFromPoint(position, true);
                
                if (index == -1) return "";
                
                var text = textBox.Text;
                if (index >= text.Length) return "";
                
                // Find word boundaries
                var start = index;
                var end = index;
                
                while (start > 0 && char.IsLetterOrDigit(text[start - 1]))
                    start--;
                
                while (end < text.Length && char.IsLetterOrDigit(text[end]))
                    end++;
                
                return text.Substring(start, end - start);
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in GetWordAtPosition: {ex}");
                return "";
            }
        }

        private async Task ShowTranslationPopup()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentWord)) return;
                
                // Show popup immediately with loading state
                PopupWord.Text = _currentWord;
                PopupPronunciation.Text = $"[{_processor.GetPronunciation(_currentWord)}]";
                PopupTranslation.Text = "Loading...";
                PopupExamples.Text = "Please wait...";
                LoadingPanel.Visibility = Visibility.Visible;
                PopupError.Visibility = Visibility.Collapsed;
                
                HoverPopup.IsOpen = true;
                StatusText.Text = $"Loading translation for: {_currentWord}";
                
                // Get translation from service (which will call DeepSeek API)
                var translation = await _translationService.GetTranslationAsync(_currentWord);
                
                // Update popup with results
                LoadingPanel.Visibility = Visibility.Collapsed;
                
                if (translation.IsLoading)
                {
                    PopupTranslation.Text = "Still loading...";
                    PopupExamples.Text = "Please wait a moment...";
                }
                else if (!string.IsNullOrEmpty(translation.Error))
                {
                    PopupTranslation.Text = "Translation Error";
                    PopupExamples.Text = "Unable to get translation";
                    PopupError.Text = translation.Error;
                    PopupError.Visibility = Visibility.Visible;
                    StatusText.Text = $"Error getting translation for: {_currentWord}";
                }
                else
                {
                    PopupTranslation.Text = translation.Meaning;
                    PopupExamples.Text = translation.Examples;
                    StatusText.Text = $"Showing translation for: {_currentWord}";
                }
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in ShowTranslationPopup: {ex}");
                LoadingPanel.Visibility = Visibility.Collapsed;
                PopupTranslation.Text = "Translation Error";
                PopupExamples.Text = "Unable to get translation";
                PopupError.Text = ex.Message;
                PopupError.Visibility = Visibility.Visible;
                StatusText.Text = $"Error getting translation: {ex.Message}";
            }
        }

        private async void PronunciationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(InputTextBox.Text))
                {
                    MessageBox.Show("Please enter some Russian text first.", "No Text", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                StatusText.Text = "Generating pronunciation...";
                var pronunciation = await _processor.GenerateFullPronunciationAsync(InputTextBox.Text);
                
                OutputTextBlock.Text = $"🔊 Pronunciation Guide:\n\n{pronunciation}";
                StatusText.Text = "Pronunciation generated successfully!";
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in PronunciationButton_Click: {ex}");
                StatusText.Text = $"Error generating pronunciation: {ex.Message}";
                MessageBox.Show($"Error generating pronunciation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SwitchModeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _isFlashcardMode = !_isFlashcardMode;
                
                if (_isFlashcardMode)
                {
                    SwitchToFlashcardMode();
                }
                else
                {
                    SwitchToTranslationMode();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in SwitchModeButton_Click: {ex}");
            }
        }

        private void SwitchToFlashcardMode()
        {
            try
            {
                TranslationPanel.Visibility = Visibility.Collapsed;
                FlashcardPanel.Visibility = Visibility.Visible;
                FlashcardAnswer.Visibility = Visibility.Collapsed;
                SwitchModeButton.Content = "🔄 Switch to Translation";
                
                _currentCardIndex = 0;
                _score = 0;
                _answerShown = false;
                ScoreText.Text = "0";
                
                ShowNextCard();
                StatusText.Text = "Flashcard mode activated!";
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in SwitchToFlashcardMode: {ex}");
            }
        }

        private void SwitchToTranslationMode()
        {
            try
            {
                TranslationPanel.Visibility = Visibility.Visible;
                FlashcardPanel.Visibility = Visibility.Collapsed;
                FlashcardAnswer.Visibility = Visibility.Collapsed;
                SwitchModeButton.Content = "🔄 Switch to Flashcards";
                
                StatusText.Text = "Translation mode activated!";
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in SwitchToTranslationMode: {ex}");
            }
        }

        private void ShowNextCard()
        {
            try
            {
                if (_currentCardIndex >= _flashcardWords.Count)
                {
                    _currentCardIndex = 0; // Restart from beginning
                }
                
                FlashcardWord.Text = _flashcardWords[_currentCardIndex];
                FlashcardAnswer.Visibility = Visibility.Collapsed;
                _answerShown = false;
                
                StatusText.Text = $"Card {_currentCardIndex + 1} of {_flashcardWords.Count}";
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in ShowNextCard: {ex}");
            }
        }

        private async void ShowAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_answerShown) return;
                
                var word = FlashcardWord.Text;
                var translation = await _translationService.GetTranslationAsync(word);
                var pronunciation = _processor.GetPronunciation(word);
                
                FlashcardTranslation.Text = translation.Meaning;
                FlashcardPronunciation.Text = $"[{pronunciation}]";
                FlashcardExamples.Text = translation.Examples;
                
                FlashcardAnswer.Visibility = Visibility.Visible;
                _answerShown = true;
                
                StatusText.Text = $"Answer shown for: {word}";
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in ShowAnswerButton_Click: {ex}");
                StatusText.Text = $"Error showing answer: {ex.Message}";
            }
        }

        private void NextCardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentCardIndex++;
                ShowNextCard();
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in NextCardButton_Click: {ex}");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InputTextBox.Text = "";
                OutputTextBlock.Text = "Enter Russian text and hover over words to see translations and pronunciation guides...";
                StatusText.Text = "Ready - Hover over Russian words to see translations";
                UpdateWordCount();
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in ClearButton_Click: {ex}");
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(OutputTextBlock.Text);
                StatusText.Text = "Text copied to clipboard!";
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in CopyButton_Click: {ex}");
                StatusText.Text = $"Error copying to clipboard: {ex.Message}";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = $"russian_pronunciation_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                };
                
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, OutputTextBlock.Text);
                    StatusText.Text = $"File saved: {saveFileDialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in SaveButton_Click: {ex}");
                StatusText.Text = $"Error saving file: {ex.Message}";
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GlobalHookButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _globalHookEnabled = !_globalHookEnabled;
                
                if (_globalHookEnabled)
                {
                    _globalHook.StartHook();
                    GlobalHookButton.Content = "🌐 Disable Global Hook";
                    GlobalHookButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                    StatusText.Text = "Global hook enabled! Hover over Russian text for 1 second to see translations.";
                    LogMessage("Global hook enabled");
                }
                else
                {
                    _globalHook.StopHook();
                    _globalHoverTimer.Stop();
                    GlobalHookButton.Content = "🌐 Enable Global Hook";
                    GlobalHookButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
                    StatusText.Text = "Global hook disabled.";
                    LogMessage("Global hook disabled");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR in GlobalHookButton_Click: {ex}");
                StatusText.Text = $"Error toggling global hook: {ex.Message}";
                MessageBox.Show($"Error toggling global hook: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private static void LogMessage(string message)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] [MainWindow] {message}";
                
                File.AppendAllText(LogFile, logEntry + Environment.NewLine);
                Console.WriteLine(logEntry);
            }
            catch
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [MainWindow] {message}");
            }
        }
    }
}
