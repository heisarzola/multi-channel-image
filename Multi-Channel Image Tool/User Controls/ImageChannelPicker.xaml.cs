using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;
using Multi_Channel_Image_Tool.Interfaces;

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
        public partial class ImageChannelPicker : UserControl, ICanHaveErrors, IHaveStates
        {
            //------------------------------------------------------------------------------------//
            /*----------------------------------- FIELDS -----------------------------------------*/
            //------------------------------------------------------------------------------------//
            
            public event EventHandler StateChanged;

            private EChannelPickerType _pickerType = EChannelPickerType.SetUniformValue;
            private EChannel _channelToExtract = EChannel.R;
            private EChannel _targetChannel = EChannel.R;
            private string _targetImagePath;
            private ImageSource _extractedPreview;

            //------------------------------------------------------------------------------------//
            /*--------------------------------- PROPERTIES ---------------------------------------*/
            //------------------------------------------------------------------------------------//
            
            public EChannel TargetChannel
            {
                get => _targetChannel;
                set => _targetChannel = value;
            }

            public EChannelPickerType PickerType => _pickerType;

            public ImageSource Preview => ImageUtility.Validation.IsValidImage(_targetImagePath) || _pickerType == EChannelPickerType.SetUniformValue ? _extractedPreview : null;

            public Bitmap ChannelImage
            {
                get
                {
                    switch (_pickerType)
                    {
                        case EChannelPickerType.PickTexture:
                            return ImageUtility.ImageGeneration.ExtractChannel(_targetImagePath, _channelToExtract, _targetChannel);
                        case EChannelPickerType.SetUniformValue:
                            int pickerValue = UniformValueSlider.Value;
                            switch (_targetChannel)
                            {
                                case EChannel.R:
                                    return ImageUtility.ImageGeneration.GenerateSolidColor(pickerValue, 0, 0, 255);
                                case EChannel.G:
                                    return ImageUtility.ImageGeneration.GenerateSolidColor(0, pickerValue, 0, 255);
                                case EChannel.B:
                                    return ImageUtility.ImageGeneration.GenerateSolidColor(0, 0, pickerValue, 255);
                                case EChannel.A:
                                    return ImageUtility.ImageGeneration.GenerateSolidColor(pickerValue, pickerValue, pickerValue, pickerValue);
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            public List<string> Errors
            {
                get
                {
                    List<string> errors = new List<string>();

                    switch (_pickerType)
                    {
                        case EChannelPickerType.PickTexture:
                            if (!ImageUtility.Validation.IsValidImage(_targetImagePath))
                            {
                                errors.Add($"No valid image has been picked for channel {_targetChannel.ToString()}.");
                            }
                            break;
                        case EChannelPickerType.SetUniformValue:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    return errors;
                }
            }

            //------------------------------------------------------------------------------------//
            /*---------------------------------- METHODS -----------------------------------------*/
            //------------------------------------------------------------------------------------//

            public ImageChannelPicker()
            {
                InitializeComponent();
                bool firstLoad = false;
                Loaded += (sender, args) =>
                {
                    if (!firstLoad)
                    {
                        firstLoad = true;
                        InitializeElements();
                        UpdateVisualElements();
                        UniformValueSlider.SliderValueChanged += (s, a) =>
                        {
                            UpdatePreviews();
                            OnStateChanged();
                        };
                    }
                };
            }

            private void OnStateChanged()
            {
                if (StateChanged != null)
                {
                    StateChanged(this, EventArgs.Empty);
                }
            }

            private void InitializeElements()
            {
                UniformValueSlider.Value = 255;
                PickerTypeDropdown.SelectedIndex = 1;
                _channelToExtract = TargetChannel;
                ChannelToExtract.SelectedIndex = (int)_channelToExtract;
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
                PreviewImage.Source = null;
                PreviewImageTooltip.Source = null;

                switch (_pickerType)
                {
                    case EChannelPickerType.PickTexture:
                        if (string.IsNullOrEmpty(_targetImagePath)) { return; }
                        break;
                    case EChannelPickerType.SetUniformValue:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _extractedPreview = ImageUtility.ImageGeneration.BitmapToImageSource(ChannelImage);

                PreviewImage.Source = _extractedPreview;
                PreviewImageTooltip.Source = _extractedPreview;
            }

            private void OnSelectedPickerTypeChanged(object sender, SelectionChangedEventArgs e)
            {
                _pickerType = (EChannelPickerType)PickerTypeDropdown.SelectedIndex;
                UpdateVisualElements();
                OnStateChanged();
            }

            private void PickImage(object sender, RoutedEventArgs e)
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = false,
                    Filters = { new CommonFileDialogFilter("Image", ImageUtility.Validation._VALID_EXTENSIONS_AS_STRING_LIST) }
                };
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    if (ImageUtility.Validation.IsValidImage(dialog.FileName))
                    {
                        _targetImagePath = dialog.FileName;
                        TargetImagePath.Text = _targetImagePath;
                    }
                    else
                    {
                        MessageBox.Show("The selected file is not a valid image.");
                        _targetImagePath = string.Empty;
                    }
                    UpdatePreviews();
                    OnStateChanged();
                }
            }

            private void OnTargetChannelChanged(object sender, SelectionChangedEventArgs e)
            {
                _channelToExtract = (EChannel)ChannelToExtract.SelectedIndex;
                UpdatePreviews();
                OnStateChanged();
            }

            private void TryRefreshImage(object sender, RoutedEventArgs e)
            {
                if (ImageUtility.Validation.IsValidImage(TargetImagePath.Text))
                {
                    _targetImagePath = TargetImagePath.Text;
                }
                else
                {
                    MessageBox.Show("The provided file path does not point to a valid image.");
                }
                UpdatePreviews();
                OnStateChanged();
            }

            private void OnTextChanged(object sender, TextChangedEventArgs e)
            {
                _targetImagePath = TargetImagePath.Text;
                OnStateChanged();
            }
        }
    }
}
