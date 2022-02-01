using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using Multi_Channel_Image_Tool.Interfaces;
using Multi_Channel_Image_Tool.User_Controls;
using Image = System.Windows.Controls.Image;

namespace Multi_Channel_Image_Tool
{
    public class SplitChannelSettings
    {
        private Bitmap _imageCache;
        private string _lastSettingsHash = string.Empty;

        public readonly EChannel targetChannel;
        public readonly System.Windows.Controls.Image previewControl;
        public readonly System.Windows.Controls.Image tooltipControl;
        public readonly CheckBox invertCheckbox;
        public readonly Button saveButton;
        public readonly ImagePicker imagePicker;

        public Bitmap Cache => _imageCache;
        public string SettingsHash => $"{imagePicker.SelectedImagePath}|{invertCheckbox.IsChecked ?? false}";

        public SplitChannelSettings(Image previewControl, Image tooltipControl, CheckBox invertCheckbox, Button saveButton, ImagePicker imagePicker, EChannel targetChannel)
        {
            this.previewControl = previewControl;
            this.tooltipControl = tooltipControl;
            this.invertCheckbox = invertCheckbox;
            this.saveButton = saveButton;
            this.targetChannel = targetChannel;
            this.imagePicker = imagePicker;
        }

        public void UpdateVisualElements(List<string> errors)
        {
            saveButton.IsEnabled = invertCheckbox.IsEnabled = errors.Count == 0;

            if (errors.Count > 0)
            {
                var icon = _imageCache != null ? ImageUtility.EditorImages.Error : ImageUtility.EditorImages.Icon;
                tooltipControl.Source = previewControl.Source = icon;
                return;
            }
            string settingsHash = SettingsHash;
            if (!_lastSettingsHash.Equals(settingsHash))
            {
                _lastSettingsHash = settingsHash;
                _imageCache = ImageUtility.ImageGeneration.ExtractChannel(imagePicker.SelectedImagePath, targetChannel,
                    targetChannel, invertCheckbox.IsChecked ?? false, $"Extracting Channel {targetChannel.ToString().ToUpperInvariant()}");
            }

            tooltipControl.Source = previewControl.Source = ImageUtility.ImageGeneration.BitmapToImageSource(_imageCache);
        }
    }

    public partial class MainWindow : Window
    {
        //------------------------------------------------------------------------------------//
        /*----------------------------------- FIELDS -----------------------------------------*/
        //------------------------------------------------------------------------------------//

        private SplitChannelSettings _split_r, _split_g, _split_b, _split_a;
        private List<Tuple<string, ICanHaveErrors>> _split_errorDependencies = new List<Tuple<string, ICanHaveErrors>>();

        //------------------------------------------------------------------------------------//
        /*--------------------------------- PROPERTIES ---------------------------------------*/
        //------------------------------------------------------------------------------------//

        private List<string> Split_Errors
        {
            get
            {
                List<string> errors = new List<string>();

                foreach (var errorDependency in _split_errorDependencies)
                {
                    errors.Concat(errorDependency.Item2.Errors.ConvertAll(error => $"{errorDependency.Item1}: {error}"));
                }

                return errors;
            }
        }

        private bool Split_AreThereErrors
        {
            get
            {
                var errors = Combine_Errors;
                if (errors.Count > 0)
                {
                    MessageBox.Show("Some referenced images have changed, and are now invalid, please correct these and try again.");
                    Combine_UpdateVisualElements();
                    OnMainStateChanged();
                    return true;
                }
                return false;
            }
        }

        //------------------------------------------------------------------------------------//
        /*---------------------------------- METHODS -----------------------------------------*/
        //------------------------------------------------------------------------------------//

        private void Split_Initialize()
        {
            // Add Error Dependencies
            _split_errorDependencies.Add(new Tuple<string, ICanHaveErrors>("Image Picker", Split_ImagePicker));

            // Subscribe To Their Changes
            Split_ImagePicker.StateChanged += (sender, args) => { Split_UpdateVisualElements(); OnMainStateChanged(); };

            _split_r = new SplitChannelSettings(Split_RPreview, Split_RPreviewTooltip, Split_InvertCheckboxR, Split_SaveR, Split_ImagePicker, EChannel.R);
            _split_g = new SplitChannelSettings(Split_GPreview, Split_GPreviewTooltip, Split_InvertCheckboxG, Split_SaveG, Split_ImagePicker, EChannel.G);
            _split_b = new SplitChannelSettings(Split_BPreview, Split_BPreviewTooltip, Split_InvertCheckboxB, Split_SaveB, Split_ImagePicker, EChannel.B);
            _split_a = new SplitChannelSettings(Split_APreview, Split_APreviewTooltip, Split_InvertCheckboxA, Split_SaveA, Split_ImagePicker, EChannel.A);

            Split_InvertCheckboxR.Checked += (s, a) => { _split_r.UpdateVisualElements(Split_Errors); OnMainStateChanged(); };
            Split_InvertCheckboxR.Unchecked += (s, a) => { _split_r.UpdateVisualElements(Split_Errors); OnMainStateChanged(); };

            Split_InvertCheckboxG.Checked += (s, a) => { _split_g.UpdateVisualElements(Split_Errors); OnMainStateChanged(); };
            Split_InvertCheckboxG.Unchecked += (s, a) => { _split_g.UpdateVisualElements(Split_Errors); OnMainStateChanged(); };

            Split_InvertCheckboxB.Checked += (s, a) => { _split_b.UpdateVisualElements(Split_Errors); OnMainStateChanged(); };
            Split_InvertCheckboxB.Unchecked += (s, a) => { _split_b.UpdateVisualElements(Split_Errors); OnMainStateChanged(); };

            Split_InvertCheckboxA.Checked += (s, a) => { _split_a.UpdateVisualElements(Split_Errors); OnMainStateChanged(); };
            Split_InvertCheckboxA.Unchecked += (s, a) => { _split_a.UpdateVisualElements(Split_Errors); OnMainStateChanged(); };
        }

        private void Split_UpdateVisualElements()
        {
            var errors = Split_Errors;

            _split_r.UpdateVisualElements(errors);
            _split_g.UpdateVisualElements(errors);
            _split_b.UpdateVisualElements(errors);
            _split_a.UpdateVisualElements(errors);
        }

        private void StartSaveOperation(SplitChannelSettings target)
        {
            if (Split_AreThereErrors) { return; }

            CommonSaveFileDialog dialog = new CommonSaveFileDialog
            {
                DefaultFileName = "Output",
                DefaultExtension = "png",
                Filters = { new CommonFileDialogFilter("PNG File", "png") }
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                try
                {
                    target.Cache.Save(dialog.FileName);
                    MessageBox.Show("Image successfully saved.");
                }
                catch
                {
                    MessageBox.Show("An error has occurred generating and saving the final image, no image was saved. Please check this application has the required permissions to read/write the images.");
                }
            }
            else
            {
                MessageBox.Show("Operation cancelled, no image was saved.");
            }
        }

        private void Split_SaveRButton(object sender, RoutedEventArgs e) => StartSaveOperation(_split_r);
        private void Split_SaveGButton(object sender, RoutedEventArgs e) => StartSaveOperation(_split_g);
        private void Split_SaveBButton(object sender, RoutedEventArgs e) => StartSaveOperation(_split_b);
        private void Split_SaveAButton(object sender, RoutedEventArgs e) => StartSaveOperation(_split_a);
    }
}