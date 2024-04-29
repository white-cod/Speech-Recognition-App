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
    public partial class MainWindow
    {
        // Button click event handlers
        private void OnStartButtonClick(object sender, RoutedEventArgs e)
        {
            string selectedDeviceId = (audioDeviceComboBox.SelectedItem as AudioDevice)?.Id;
            string selectedLanguageCode = GetSelectedLanguageCode();

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"{pythonScriptPath} {selectedDeviceId} {selectedLanguageCode}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            pythonProcess = new Process { StartInfo = startInfo };
            pythonProcess.Start();

            stopButton.IsEnabled = true;
            startButton.IsEnabled = false;
        }

        private void OnStopButtonClick(object sender, RoutedEventArgs e)
        {
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                pythonProcess.Kill();
                stopButton.IsEnabled = false;
                startButton.IsEnabled = true;
            }
        }

        private void OnClearButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                File.WriteAllText(filePath, string.Empty);
                Dispatcher.Invoke(() => { textBox.Text = string.Empty; });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => { textBox.Text = "Error clearing file: " + ex.Message; });
            }
        }

        private void OnDecreaseFontSizeClick(object sender, RoutedEventArgs e)
        {
            textBox.FontSize -= 2;
            UpdateFontSizeComboBox();
        }

        private void OnIncreaseFontSizeClick(object sender, RoutedEventArgs e)
        {
            textBox.FontSize += 2;
            UpdateFontSizeComboBox();
        }

        private void OnExportButtonClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                FileName = "Document"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(saveFileDialog.FileName, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new Body());

                    string text = textBox.Text;
                    Body body = mainPart.Document.Body;
                    Paragraph paragraph = new Paragraph();
                    Run run = new Run(new Text(text));
                    paragraph.Append(run);
                    body.Append(paragraph);
                }
            }
        }

        // Other event handlers
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void OnFontFamilySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fontFamilyComboBox.SelectedItem != null)
            {
                ComboBoxItem selectedFontFamily = (ComboBoxItem)fontFamilyComboBox.SelectedItem;
                textBox.FontFamily = new System.Windows.Media.FontFamily(selectedFontFamily.Content.ToString());
            }
        }

        private void OnFontSizeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fontSizeComboBox.SelectedItem != null)
            {
                ComboBoxItem selectedFontSizeItem = (ComboBoxItem)fontSizeComboBox.SelectedItem;
                double selectedFontSize;
                if (double.TryParse(selectedFontSizeItem.Content.ToString(), out selectedFontSize))
                {
                    textBox.FontSize = selectedFontSize;
                }
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            UpdateUI(null, null);
        }

        private void UpdateUI(object sender, EventArgs e)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    string text = reader.ReadToEnd();
                    Dispatcher.Invoke(() =>
                    {
                        textBox.Text = text;
                    });
                }

                if (pythonProcess != null && pythonProcess.HasExited)
                {
                    Dispatcher.Invoke(() =>
                    {
                        stopButton.IsEnabled = false;
                        startButton.IsEnabled = true;
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => { textBox.Text = "Error reading file: " + ex.Message; });
            }
        }

        private void UpdateFontSizeComboBox()
        {
            fontSizeComboBox.SelectedItem = fontSizeComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == textBox.FontSize.ToString());
        }
    }
}
