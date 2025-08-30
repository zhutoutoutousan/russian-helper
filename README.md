# 🇷🇺 Russian Helper

A comprehensive C# WPF application for Russian language learning with **omnipresent right-click translation** functionality, pronunciation guides, and flashcard games.

## ✨ Features

### 🌐 **Omnipresent Right-Click Translation** (NEW!)
- **Global Hook**: Right-click anywhere on your screen to translate Russian text
- **OCR Integration**: Automatically recognizes Russian text from any application or webpage
- **DeepSeek API**: Real-time translations powered by advanced AI
- **Smart Positioning**: Translation popup appears intelligently positioned near your mouse
- **Loading States**: Visual feedback during API calls

### 📝 **Translation & Pronunciation**
- Hover over Russian words in the app to see instant translations
- Generate pronunciation guides for entire sentences
- Copy and save pronunciation results
- Word count tracking

### 🎯 **Flashcard Game**
- Interactive flashcard mode with Russian words
- Show/hide answers with translations and pronunciation
- Navigate through cards
- Score tracking

## 🚀 **How to Use the Omnipresent Translation**

1. **Launch the Application**: Run `RussianHelper.exe`
2. **Enable Global Hook**: Click the "🌐 Enable Global Hook" button
3. **Right-Click Anywhere**: Right-click on any Russian text on your screen
4. **Get Translation**: A popup will appear with:
   - Recognized Russian word
   - Pronunciation guide
   - English translation
   - Example sentences
   - Grammar information
   - CEFR level

### **Supported Sources**
- Web browsers (Chrome, Firefox, Edge)
- PDF documents
- Word documents
- Any application displaying Russian text
- Images with Russian text (via OCR)

## 🛠️ **Technical Requirements**

- **Windows 10/11** (64-bit)
- **.NET 8.0 Runtime**
- **Tesseract OCR** (automatically handled)
- **Internet connection** (for DeepSeek API)

## 📦 **Installation**

1. Download the latest release
2. Extract to a folder
3. Run `RussianHelper.exe`
4. Grant permissions when prompted for global hook access

## 🔧 **Configuration**

### DeepSeek API Key
The application includes a pre-configured API key. For production use, consider:
- Using your own DeepSeek API key
- Setting up rate limiting
- Implementing API key rotation

### OCR Settings
- Default capture radius: 100 pixels around mouse position
- Supports Russian and English text recognition
- Automatic language detection

## 🎮 **Usage Examples**

### **Omnipresent Translation**
```
1. Open any webpage with Russian text
2. Enable global hook in Russian Helper
3. Right-click on "привет" → See "Hello" translation
4. Right-click on "спасибо" → See "Thank you" translation
```

### **In-App Translation**
```
1. Type Russian text in the input box
2. Hover over words to see translations
3. Right-click for instant translation
4. Generate full sentence pronunciation
```

### **Flashcard Mode**
```
1. Switch to flashcard mode
2. See Russian word: "привет"
3. Click "Show Answer" → See translation and pronunciation
4. Click "Next Card" to continue
```

## 🔒 **Privacy & Security**

- **Local Processing**: OCR and text recognition happen locally
- **API Calls**: Only Russian words are sent to DeepSeek API
- **No Data Storage**: Translations are cached temporarily in memory
- **Global Hook**: Only captures right-click events, no continuous monitoring

## 🐛 **Troubleshooting**

### **Global Hook Not Working**
- Ensure the application has administrator privileges
- Check Windows security settings
- Restart the application

### **OCR Not Recognizing Text**
- Ensure text is clearly visible
- Try adjusting screen resolution
- Check if Tesseract data is properly installed

### **API Translation Errors**
- Check internet connection
- Verify DeepSeek API key is valid
- Check API rate limits

## 📝 **Log Files**

The application creates detailed logs in:
- `russian_helper.log` - Application logs
- Console output - Real-time debugging

## 🤝 **Contributing**

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📄 **License**

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 **Acknowledgments**

- **DeepSeek** for providing the translation API
- **Tesseract** for OCR capabilities
- **Microsoft** for WPF framework
- **Newtonsoft.Json** for JSON handling

---

**Happy Russian Learning! 🇷🇺📚**
