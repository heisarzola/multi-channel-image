using System.Windows;

namespace Multi_Channel_Image_Tool
{
    public partial class MainWindow : Window
    {
        //------------------------------------------------------------------------------------//
        /*---------------------------------- METHODS -----------------------------------------*/
        //------------------------------------------------------------------------------------//

        public MainWindow()
        {
            InitializeComponent();

            Combine_UpdateVisualElements();
            Combine_TrackStates();
        }
    }
}