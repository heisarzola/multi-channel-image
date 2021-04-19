using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Multi_Channel_Image_Tool.Interfaces;

namespace Multi_Channel_Image_Tool
{
    public partial class MainWindow : Window
    {
        //------------------------------------------------------------------------------------//
        /*----------------------------------- FIELDS -----------------------------------------*/
        //------------------------------------------------------------------------------------//

        private Bitmap _split_r, _split_g, _split_b, _split_a;
        private List<Tuple<string, ICanHaveErrors>> _split_errorDependencies = new List<Tuple<string, ICanHaveErrors>>();
        private string _lastValidSelection;

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

        private void Split_TrackStates()
        {
            // Add Error Dependencies
            _split_errorDependencies.Add(new Tuple<string, ICanHaveErrors>("Image Picker", Split_ImagePicker));

            // Subscribe To Their Changes
            Split_ImagePicker.StateChanged += (sender, args) => { Split_UpdateVisualElements(); OnMainStateChanged(); };
        }

        private void Split_UpdateVisualElements()
        {
            var errors = Split_Errors;
            if (errors.Count > 0)
            {
                var icon = _split_a != null ? ImageUtility.EditorImages.Error : ImageUtility.EditorImages.Icon;

                Split_APreviewTooltip.Source = Split_APreview.Source = icon;
                Split_RPreviewTooltip.Source = Split_RPreview.Source = icon;
                Split_GPreviewTooltip.Source = Split_GPreview.Source = icon;
                Split_BPreviewTooltip.Source = Split_BPreview.Source = icon;
            }
            else
            {
                if (!Split_ImagePicker.SelectedImagePath.Equals(_lastValidSelection))
                {
                    _lastValidSelection = Split_ImagePicker.SelectedImagePath;

                    _split_r = ImageUtility.ImageGeneration.ExtractChannel(Split_ImagePicker.SelectedImagePath, EChannel.R, EChannel.R);
                    _split_g = ImageUtility.ImageGeneration.ExtractChannel(Split_ImagePicker.SelectedImagePath, EChannel.G, EChannel.G);
                    _split_b = ImageUtility.ImageGeneration.ExtractChannel(Split_ImagePicker.SelectedImagePath, EChannel.B, EChannel.B);
                    _split_a = ImageUtility.ImageGeneration.ExtractChannel(Split_ImagePicker.SelectedImagePath, EChannel.A, EChannel.A);
                }

                Split_APreviewTooltip.Source = Split_APreview.Source = ImageUtility.ImageGeneration.BitmapToImageSource(_split_a);
                Split_RPreviewTooltip.Source = Split_RPreview.Source = ImageUtility.ImageGeneration.BitmapToImageSource(_split_r);
                Split_GPreviewTooltip.Source = Split_GPreview.Source = ImageUtility.ImageGeneration.BitmapToImageSource(_split_g);
                Split_BPreviewTooltip.Source = Split_BPreview.Source = ImageUtility.ImageGeneration.BitmapToImageSource(_split_b);
            }

            Split_SaveR.IsEnabled = Split_SaveG.IsEnabled = Split_SaveB.IsEnabled = Split_SaveA.IsEnabled = errors.Count == 0;
        }

        private void StartSaveOperation(Bitmap target)
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
                    target.Save(dialog.FileName);
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