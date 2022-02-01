using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace Multi_Channel_Image_Tool.Additional_Windows
{
    /// <summary>
    /// Interaction logic for ProgressBar.xaml
    /// </summary>
    public partial class PopupTextWindow : Window
    {
        public static void OpenWindowAndExecute(string message, Action toExecute)
        {
            PopupTextWindow window = new PopupTextWindow() { PopupText = { Text = message }, Owner = App.Current.MainWindow };
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (s, a) =>
            {
                toExecute();
                (s as BackgroundWorker)?.ReportProgress(100);
            };
            worker.ProgressChanged += (s, a) => { window.Close(); };
            worker.RunWorkerAsync();
            window.ShowDialog();
        }

        public PopupTextWindow()
        {
            InitializeComponent();
        }
    }
}
