using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using NAudio.Wave;
using System.ComponentModel;
/*using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Svg.Skia;*/
using Path = System.IO.Path;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Win32;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;


namespace SpeechToTextApp
{
    public partial class MainWindow : Window
    {
        private readonly string pythonScriptPath = "Speech-to-Text.py";
        private readonly string audioDevicesFilePath = "audio_devices.txt";
        private string filePath;
        private FileSystemWatcher watcher;
        private Process pythonProcess;

        public MainWindow()
        {
            InitializeComponent();
            this.MinWidth = 1000;
            this.MinHeight = 700;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            PopulateAudioDevicesComboBox();

            Closing += MainWindow_Closing;

            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "recognized_text.txt");

            watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName(filePath);
            watcher.Filter = Path.GetFileName(filePath);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += OnFileChanged;
            watcher.EnableRaisingEvents = true;

            var timer = new DispatcherTimer();
            timer.Tick += UpdateUI;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                pythonProcess.Kill(); // Terminate the Python process if running
            }
        }


        private void PopulateAudioDevicesComboBox()
        {
            var audioDevices = ReadAudioDevicesFromFile();
            audioDeviceComboBox.ItemsSource = audioDevices;
            audioDeviceComboBox.DisplayMemberPath = "Name";
            audioDeviceComboBox.SelectedIndex = 0;
        }

        private string GetSelectedLanguageCode()
        {
            string selectedLanguage = (languageComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            switch (selectedLanguage)
            {
                case "English (en-US)":
                    return "en-US";
                case "Russian (ru-RU)":
                    return "ru-RU";
                case "Ukrainian (uk-UA)":
                    return "uk-UA";
                case "French (fr-FR)":
                    return "fr-FR";
                case "German (de-DE)":
                    return "de-DE";
                case "Spanish (es-ES)":
                    return "es-ES";
                case "Polish (pl-PL)":
                    return "pl-PL";
                case "Italian (it-IT)":
                    return "it-IT";
                case "Portuguese (pt-PT)":
                    return "pt-PT";
                case "Japanese (ja-JP)":
                    return "ja-JP";
                default:
                    return "en-US";
            }
        }

        private ObservableCollection<AudioDevice> ReadAudioDevicesFromFile()
        {
            var audioDevices = new ObservableCollection<AudioDevice>();

            string[] lines = File.ReadAllLines(audioDevicesFilePath);
            foreach (var line in lines)
            {
                string[] parts = line.Split(':');
                if (parts.Length == 2)
                {
                    string id = parts[0].Trim();
                    string name = parts[1].Trim();
                    audioDevices.Add(new AudioDevice { Id = id, Name = name });
                }
            }

            return audioDevices;
        }
    }
}