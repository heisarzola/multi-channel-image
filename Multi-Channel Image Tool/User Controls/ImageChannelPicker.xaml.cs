using System;
using System.Windows;
using System.Windows.Controls;

namespace Multi_Channel_Image_Tool
{
    public enum EChannel
    {
        R = 0,
        G = 1,
        B = 2,
        A = 3
    }

    public enum EChannelPickerType
    {
        PickTexture = 0,
        SetUniformValue = 1
    }

    namespace User_Controls
    {
        public partial class ImageChannelPicker : UserControl
        {
            //------------------------------------------------------------------------------------//
            /*----------------------------------- FIELDS -----------------------------------------*/
            //------------------------------------------------------------------------------------//

            private EChannelPickerType _pickerType = EChannelPickerType.PickTexture;
            private EChannel _channelToExtract = EChannel.R;
            private EChannel _previewChannel = EChannel.R;
            private string _targetImagePath;

            //------------------------------------------------------------------------------------//
            /*--------------------------------- PROPERTIES ---------------------------------------*/
            //------------------------------------------------------------------------------------//

            private int UniformValueDefault
            {
                get
                {
                    switch (_previewChannel)
                    {
                        case EChannel.R:
                        case EChannel.G:
                        case EChannel.B:
                            return 0;
                        case EChannel.A:
                            return 255;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            public EChannel PreviewChannel
            {
                get => _previewChannel;
                set => _previewChannel = value;
            }

            //------------------------------------------------------------------------------------//
            /*---------------------------------- METHODS -----------------------------------------*/
            //------------------------------------------------------------------------------------//

            public ImageChannelPicker()
            {
                InitializeComponent();
                UpdateVisualElements();
                InitializeElements();
            }

            private void InitializeElements()
            {
                UniformValueSlider.Value = UniformValueDefault;
                PickerType.SelectedIndex = 0;
                ChannelToExtract.SelectedIndex = 0;
            }

            private void UpdateVisualElements()
            {
                SetElementsVisibility();
                UpdatePreviews();
            }

            private void SetElementsVisibility()
            {
                SetUniformArea.Visibility = _pickerType == EChannelPickerType.SetUniformValue ? Visibility.Visible : Visibility.Hidden;
                PickTextureArea.Visibility = _pickerType == EChannelPickerType.PickTexture ? Visibility.Visible : Visibility.Hidden;
            }

            private void UpdatePreviews()
            {

            }

            private void OnSelectedPickerTypeChanged(object sender, SelectionChangedEventArgs e)
            {
                _pickerType = (EChannelPickerType) PickerType.SelectedIndex;
                UpdateVisualElements();
            }
        }
    }
}
