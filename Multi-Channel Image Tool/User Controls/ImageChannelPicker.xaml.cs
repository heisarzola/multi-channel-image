using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

            private string _lastSettingsHash;
            private Bitmap _cachedResult;

            //------------------------------------------------------------------------------------//
            /*--------------------------------- PROPERTIES ---------------------------------------*/
            //------------------------------------------------------------------------------------//

            public EChannel TargetChannel
            {
                get => _targetChannel;
                set => _targetChannel = value;
            }

            public EChannelPickerType PickerType => _pickerType;

            public string SettingsHash
            {
                get
                {
                    switch (_pickerType)
                    {
                        case EChannelPickerType.PickTexture:
                            return $"{TargetImagePath}|{_channelToExtract.ToString()}|{_targetChannel.ToString()}";
                        case EChannelPickerType.SetUniformValue:
                            return UniformValueSlider.Value.ToString();
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            public ImageSource Preview => ImageUtility.Validation.IsValidImage(TargetImagePath) || _pickerType == EChannelPickerType.SetUniformValue ? ImageUtility.ImageGeneration.BitmapToImageSource(ChannelImage) : null;

            public Bitmap ChannelImage
            {
                get
                {
                    UpdateCachedResult();
                    return _cachedResult;
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
                            errors.Concat(TargetImagePicker.Errors.ConvertAll(error => $"Image Picker: {errors}"));
                            break;
                        case EChannelPickerType.SetUniformValue:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    return errors;
                }
            }

            private string TargetImagePath => TargetImagePicker.SelectedImagePath;

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
                        TargetImagePicker.StateChanged += (s, a) =>
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
            private void UpdateCachedResult()
            {
                // Only redo combining the channels if settings changed.
                if (!SettingsHash.Equals(_lastSettingsHash))
                {
                    _lastSettingsHash = SettingsHash;

                    switch (_pickerType)
                    {
                        case EChannelPickerType.PickTexture:
                            _cachedResult= ImageUtility.ImageGeneration.ExtractChannel(TargetImagePath, _channelToExtract, _targetChannel);
                            break;
                        case EChannelPickerType.SetUniformValue:
                            int pickerValue = UniformValueSlider.Value;
                            switch (_targetChannel)
                            {
                                case EChannel.R:
                                    _cachedResult = ImageUtility.ImageGeneration.GenerateSolidColor(pickerValue, 0, 0, 255);
                                    break;
                                case EChannel.G:
                                    _cachedResult = ImageUtility.ImageGeneration.GenerateSolidColor(0, pickerValue, 0, 255);
                                    break;
                                case EChannel.B:
                                    _cachedResult = ImageUtility.ImageGeneration.GenerateSolidColor(0, 0, pickerValue, 255);
                                    break;
                                case EChannel.A:
                                    _cachedResult = ImageUtility.ImageGeneration.GenerateSolidColor(pickerValue, pickerValue, pickerValue, pickerValue);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
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
                        if (string.IsNullOrEmpty(TargetImagePath)) { return; }
                        break;
                    case EChannelPickerType.SetUniformValue:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                UpdateCachedResult();

                var extractedPreview = ImageUtility.ImageGeneration.BitmapToImageSource(ChannelImage);

                PreviewImage.Source = extractedPreview;
                PreviewImageTooltip.Source = extractedPreview;
            }

            private void OnSelectedPickerTypeChanged(object sender, SelectionChangedEventArgs e)
            {
                _pickerType = (EChannelPickerType)PickerTypeDropdown.SelectedIndex;
                UpdateVisualElements();
                OnStateChanged();
            }

            private void OnTargetChannelChanged(object sender, SelectionChangedEventArgs e)
            {
                _channelToExtract = (EChannel)ChannelToExtract.SelectedIndex;
                UpdatePreviews();
                OnStateChanged();
            }
        }
    }
}
