using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using Multi_Channel_Image_Tool.Interfaces;

namespace Multi_Channel_Image_Tool.User_Controls
{
    public partial class ImagePicker : UserControl, ICanHaveErrors, IHaveStates
    {
        //------------------------------------------------------------------------------------//
        /*----------------------------------- FIELDS -----------------------------------------*/
        //------------------------------------------------------------------------------------//

        private string _selectedImagePath;

        public event EventHandler StateChanged;

        //------------------------------------------------------------------------------------//
        /*--------------------------------- PROPERTIES ---------------------------------------*/
        //------------------------------------------------------------------------------------//

        public string SelectedImagePath => _selectedImagePath;

        public List<string> Errors
        {
            get
            {
                List<string> errors = new List<string>();
                
                if (!ImageUtility.Validation.IsValidImage(_selectedImagePath))
                {
                    errors.Add($"No valid image has been picked.");
                }

                return errors;
            }
        }

        //------------------------------------------------------------------------------------//
        /*---------------------------------- METHODS -----------------------------------------*/
        //------------------------------------------------------------------------------------//

        public ImagePicker()
        {
            InitializeComponent();
        }

        private void OnStateChanged()
        {
            if (StateChanged != null)
            {
                StateChanged(this, EventArgs.Empty);
            }
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
                    _selectedImagePath = dialog.FileName;
                    TargetImagePath.Text = _selectedImagePath;
                }
                else
                {
                    MessageBox.Show("The selected file is not a valid image.");
                    _selectedImagePath = string.Empty;
                }
                OnStateChanged();
            }
        }

        private void TryRefreshImage(object sender, RoutedEventArgs e)
        {
            if (ImageUtility.Validation.IsValidImage(TargetImagePath.Text))
            {
                _selectedImagePath = TargetImagePath.Text;
            }
            else
            {
                MessageBox.Show("The provided file path does not point to a valid image.");
            }
            OnStateChanged();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _selectedImagePath = TargetImagePath.Text;
            OnStateChanged();
        }
    }
}
