using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Multi_Channel_Image_Tool.Interfaces;
using Multi_Channel_Image_Tool.User_Controls;

namespace Multi_Channel_Image_Tool
{
    public partial class MainWindow : Window
    {
        //------------------------------------------------------------------------------------//
        /*----------------------------------- FIELDS -----------------------------------------*/
        //------------------------------------------------------------------------------------//

        private ImageSource _combine_TexturePreview;
        private List<Tuple<string, ICanHaveErrors>> _combine_errorDependencies = new List<Tuple<string, ICanHaveErrors>>();

        //------------------------------------------------------------------------------------//
        /*--------------------------------- PROPERTIES ---------------------------------------*/
        //------------------------------------------------------------------------------------//

        private List<string> Combine_Errors
        {
            get
            {
                List<string> errors = new List<string>();

                if (Combine_ImageDimensionsDontMatch)
                {
                    errors.Add("Some of the selected textures have different dimensions.");
                }

                foreach (var errorDependency in _combine_errorDependencies)
                {
                    errors.Concat(errorDependency.Item2.Errors.ConvertAll(error => $"{errorDependency.Item1}: {error}"));
                }

                return errors;
            }
        }

        private bool Combine_ImageDimensionsDontMatch
        {
            get
            {
                HashSet<ImageChannelPicker> toCheck = new HashSet<ImageChannelPicker>() { Combine_ChannelPickerR, Combine_ChannelPickerG, Combine_ChannelPickerB, Combine_ChannelPickerA };

                if (Combine_ChannelPickerR.PickerType == EChannelPickerType.SetUniformValue) { toCheck.Remove(Combine_ChannelPickerR); }
                if (Combine_ChannelPickerG.PickerType == EChannelPickerType.SetUniformValue) { toCheck.Remove(Combine_ChannelPickerG); }
                if (Combine_ChannelPickerB.PickerType == EChannelPickerType.SetUniformValue) { toCheck.Remove(Combine_ChannelPickerB); }
                if (Combine_ChannelPickerA.PickerType == EChannelPickerType.SetUniformValue) { toCheck.Remove(Combine_ChannelPickerA); }

                if (toCheck.Count == 0) { return false; }

                var firstItem = toCheck.Pop().Preview;
                int width = (int)firstItem.Width;
                int height = (int)firstItem.Height;

                foreach (var channelPicker in toCheck)
                {
                    if (width != (int)channelPicker.Preview.Width) { return true; }
                    if (height != (int)channelPicker.Preview.Height) { return true; }
                }

                return false;
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
            Combine_ChannelPickerR.StateChanged += (sender, args) => Combine_UpdateVisualElements();
            Combine_ChannelPickerG.StateChanged += (sender, args) => Combine_UpdateVisualElements();
            Combine_ChannelPickerB.StateChanged += (sender, args) => Combine_UpdateVisualElements();
            Combine_ChannelPickerA.StateChanged += (sender, args) => Combine_UpdateVisualElements();
        }

        private void Combine_UpdateVisualElements()
        {
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
        }
    }
}