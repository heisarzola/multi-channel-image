using System;
using System.Collections.Generic;
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
            private EChannel _previewChannel = EChannel.R;
            private string _targetImagePath;
            private ImageSource _extractedPreview;

            //------------------------------------------------------------------------------------//
            /*--------------------------------- PROPERTIES ---------------------------------------*/
            //------------------------------------------------------------------------------------//
            
            public EChannel PreviewChannel
            {
                get => _previewChannel;
                set => _previewChannel = value;
            }

            public EChannelPickerType PickerType => _pickerType;

            public ImageSource Preview => ImageUtility.Validation.IsValidImage(_targetImagePath) || _pickerType == EChannelPickerType.SetUniformValue ? _extractedPreview : null;

            public string SelectedImagePath => _targetImagePath;

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
                                errors.Add($"No valid image has been picked for channel {_previewChannel.ToString()}.");
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
                _channelToExtract = PreviewChannel;
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
                        _extractedPreview = ImageUtility.ImageGeneration.ExtractChannel(_targetImagePath, _channelToExtract, _previewChannel);
                        break;
                    case EChannelPickerType.SetUniformValue:
                        int pickerValue = UniformValueSlider.Value;
                        switch (_previewChannel)
                        {
                            case EChannel.R:
                                _extractedPreview = ImageUtility.ImageGeneration.GenerateSolidColor(pickerValue, 0, 0, 255);
                                break;
                            case EChannel.G:
                                _extractedPreview = ImageUtility.ImageGeneration.GenerateSolidColor(0, pickerValue, 0, 255);
                                break;
                            case EChannel.B:
                                _extractedPreview = ImageUtility.ImageGeneration.GenerateSolidColor(0, 0, pickerValue, 255);
                                break;
                            case EChannel.A:
                                _extractedPreview = ImageUtility.ImageGeneration.GenerateSolidColor(pickerValue, pickerValue, pickerValue, pickerValue);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

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
                    IsFolderPicker = false
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
        }
    }
}
