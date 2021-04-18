using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;
using Multi_Channel_Image_Tool.Interfaces;
using Multi_Channel_Image_Tool.User_Controls;

namespace Multi_Channel_Image_Tool
{
    public partial class MainWindow : Window
    {
        //------------------------------------------------------------------------------------//
        /*----------------------------------- FIELDS -----------------------------------------*/
        //------------------------------------------------------------------------------------//

        private List<Tuple<string, ICanHaveErrors>> _combine_errorDependencies = new List<Tuple<string, ICanHaveErrors>>();

        //------------------------------------------------------------------------------------//
        /*--------------------------------- PROPERTIES ---------------------------------------*/
        //------------------------------------------------------------------------------------//

        private List<string> Combine_Errors
        {
            get
            {
                List<string> errors = new List<string>();

                foreach (var errorDependency in _combine_errorDependencies)
                {
                    errors.Concat(errorDependency.Item2.Errors.ConvertAll(error => $"{errorDependency.Item1}: {error}"));
                }

                return errors;
            }
        }

        //------------------------------------------------------------------------------------//
        /*---------------------------------- METHODS -----------------------------------------*/
        //------------------------------------------------------------------------------------//

        private void Combine_TrackStates()
        {
            // Add Error Dependencies
            _combine_errorDependencies.Add(new Tuple<string, ICanHaveErrors>("Channel R", Combine_ChannelPickerR));
            _combine_errorDependencies.Add(new Tuple<string, ICanHaveErrors>("Channel G", Combine_ChannelPickerG));
            _combine_errorDependencies.Add(new Tuple<string, ICanHaveErrors>("Channel B", Combine_ChannelPickerB));
            _combine_errorDependencies.Add(new Tuple<string, ICanHaveErrors>("Channel A", Combine_ChannelPickerA));

            // Subscribe To Their Changes
            Combine_ChannelPickerR.StateChanged += (sender, args) => { Combine_UpdateVisualElements(); OnMainStateChanged(); };
            Combine_ChannelPickerG.StateChanged += (sender, args) => { Combine_UpdateVisualElements(); OnMainStateChanged(); };
            Combine_ChannelPickerB.StateChanged += (sender, args) => { Combine_UpdateVisualElements(); OnMainStateChanged(); };
            Combine_ChannelPickerA.StateChanged += (sender, args) => { Combine_UpdateVisualElements(); OnMainStateChanged(); };
        }

        private void Combine_UpdateVisualElements()
        {
            // Update Mini Tab Thumbnails
            Combine_TabRPreview.Source = Combine_ChannelPickerR.Preview;
            Combine_TabGPreview.Source = Combine_ChannelPickerG.Preview;
            Combine_TabBPreview.Source = Combine_ChannelPickerB.Preview;
            Combine_TabAPreview.Source = Combine_ChannelPickerA.Preview;

            Combine_TabRPreviewTooltip.Source = Combine_ChannelPickerR.Preview;
            Combine_TabGPreviewTooltip.Source = Combine_ChannelPickerG.Preview;
            Combine_TabBPreviewTooltip.Source = Combine_ChannelPickerB.Preview;
            Combine_TabAPreviewTooltip.Source = Combine_ChannelPickerA.Preview;

            Combine_TabRPreview.Visibility = Combine_ChannelPickerR.Preview == null ? Visibility.Collapsed : Visibility.Visible;
            Combine_TabGPreview.Visibility = Combine_ChannelPickerG.Preview == null ? Visibility.Collapsed : Visibility.Visible;
            Combine_TabBPreview.Visibility = Combine_ChannelPickerB.Preview == null ? Visibility.Collapsed : Visibility.Visible;
            Combine_TabAPreview.Visibility = Combine_ChannelPickerA.Preview == null ? Visibility.Collapsed : Visibility.Visible;

            var errors = Combine_Errors;

            // Allow/Disallow Saving
            Combine_SaveAs.IsEnabled = errors.Count == 0;
        }

        private void Combine_TryToSave(object sender, RoutedEventArgs e) => Combine_TryToSave();

        private void Combine_TryToSave()
        {
            var errors = Combine_Errors;
            if (errors.Count > 0)
            {
                MessageBox.Show("Some referenced images have changed, and are now invalid, please correct these and try again.");
                Combine_UpdateVisualElements();
                OnMainStateChanged();
                return;
            }

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
                    Bitmap combinedImage = ImageUtility.ImageGeneration.CombineChannels(Combine_ChannelPickerR.ChannelImage, Combine_ChannelPickerG.ChannelImage,
                         Combine_ChannelPickerB.ChannelImage, Combine_ChannelPickerA.ChannelImage);
                    combinedImage.Save(dialog.FileName);

                    MessageBox.Show("Image successfully generated and saved.");
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
    }
}