using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tesseract;

namespace RussianHelper
{
    public class OCRService : IDisposable
    {
        private TesseractEngine _engine;
        private bool _isInitialized = false;

        public OCRService()
        {
            InitializeTesseract();
        }

        private void InitializeTesseract()
        {
            try
            {
                // Try to find Tesseract data in common locations
                string[] possiblePaths = {
                    @"C:\Program Files\Tesseract-OCR\tessdata",
                    @"C:\tesseract\tessdata",
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "tessdata")
                };

                string tessdataPath = null;
                foreach (var path in possiblePaths)
                {
                    if (Directory.Exists(path))
                    {
                        tessdataPath = path;
                        break;
                    }
                }

                if (tessdataPath == null)
                {
                    // Create tessdata directory and download if needed
                    tessdataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
                    Directory.CreateDirectory(tessdataPath);
                }

                _engine = new TesseractEngine(tessdataPath, "rus+eng", EngineMode.Default);
                _engine.SetVariable("tessedit_char_whitelist", "абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,!?;:()[]{}'\"- ");
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize Tesseract: {ex.Message}");
                _isInitialized = false;
            }
        }

        public async Task<string> RecognizeTextFromScreenAreaAsync(Point mousePosition, int captureRadius = 100)
        {
            if (!_isInitialized)
            {
                return "OCR not initialized";
            }

            try
            {
                // Capture screen area around mouse position
                using (var bitmap = CaptureScreenArea(mousePosition, captureRadius))
                {
                    if (bitmap == null) return "Failed to capture screen area";

                    // Convert Bitmap to Pix for Tesseract
                    using (var memoryStream = new MemoryStream())
                    {
                        bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                        memoryStream.Position = 0;
                        using (var pix = Pix.LoadFromMemory(memoryStream.ToArray()))
                        using (var page = _engine.Process(pix))
                        {
                            var text = page.GetText().Trim();
                            
                            // Extract Russian words from the recognized text
                            var russianWords = ExtractRussianWords(text);
                            
                            return string.Join(" ", russianWords);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OCR Error: {ex.Message}");
                return $"OCR Error: {ex.Message}";
            }
        }

        private Bitmap CaptureScreenArea(Point mousePosition, int radius)
        {
            try
            {
                // Calculate capture rectangle
                var captureRect = new Rectangle(
                    mousePosition.X - radius,
                    mousePosition.Y - radius,
                    radius * 2,
                    radius * 2
                );

                // Ensure rectangle is within screen bounds
                var screenBounds = Screen.PrimaryScreen.Bounds;
                captureRect.Intersect(screenBounds);

                if (captureRect.Width <= 0 || captureRect.Height <= 0)
                {
                    return null;
                }

                // Capture screen area
                using (var screenBitmap = new Bitmap(captureRect.Width, captureRect.Height))
                {
                    using (var graphics = Graphics.FromImage(screenBitmap))
                    {
                        graphics.CopyFromScreen(
                            captureRect.Left,
                            captureRect.Top,
                            0, 0,
                            captureRect.Size
                        );
                    }

                    // Return a copy of the bitmap
                    return new Bitmap(screenBitmap);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Screen capture error: {ex.Message}");
                return null;
            }
        }

        private string[] ExtractRussianWords(string text)
        {
            if (string.IsNullOrEmpty(text)) return new string[0];

            // Regular expression to match Russian words
            var russianPattern = @"[а-яёА-ЯЁ]+";
            var matches = Regex.Matches(text, russianPattern);

            return matches
                .Cast<Match>()
                .Select(m => m.Value.Trim())
                .Where(word => word.Length > 1) // Filter out single characters
                .Distinct()
                .ToArray();
        }

        public bool IsRussianText(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            
            // Check if text contains Cyrillic characters
            return text.Any(c => c >= 0x0400 && c <= 0x04FF);
        }

        public void Dispose()
        {
            _engine?.Dispose();
        }
    }
}
