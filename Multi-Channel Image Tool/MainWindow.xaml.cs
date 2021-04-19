using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Multi_Channel_Image_Tool
{
    public partial class MainWindow : Window
    {
        //------------------------------------------------------------------------------------//
        /*--------------------------------- PROPERTIES ---------------------------------------*/
        //------------------------------------------------------------------------------------//

        private List<string> TabErrors
        {
            get
            {
                switch (ModulePicker.SelectedIndex)
                {
                    case 0:
                        return Combine_Errors;
                    case 1:
                        return Split_Errors;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        //------------------------------------------------------------------------------------//
        /*---------------------------------- METHODS -----------------------------------------*/
        //------------------------------------------------------------------------------------//

        public MainWindow()
        {
            InitializeComponent();

            Combine_TrackStates();
            Split_TrackStates();

            Combine_UpdateVisualElements();
            Split_UpdateVisualElements();
        }

        private void OnMainStateChanged()
        {
            UpdateErrorLabel();
        }

        private void UpdateErrorLabel()
        {
            var errors = TabErrors;
            ErrorLabel.Text = errors.Count == 0 ? string.Empty : errors.First();
        }

        private void OnSelectedModuleChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateErrorLabel();
        }
    }
}