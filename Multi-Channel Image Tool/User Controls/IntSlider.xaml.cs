using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Multi_Channel_Image_Tool.User_Controls
{
    public class ValueChangedEventArgs<T> : EventArgs
    {
        private readonly T _value;
        public T Value => _value;
        public ValueChangedEventArgs(T value) => _value = value;
    }

    public partial class IntSlider : UserControl
    {
        //------------------------------------------------------------------------------------//
        /*----------------------------------- FIELDS -----------------------------------------*/
        //------------------------------------------------------------------------------------//

        public event EventHandler<ValueChangedEventArgs<int>> SliderValueChanged;

        //------------------------------------------------------------------------------------//
        /*--------------------------------- PROPERTIES ---------------------------------------*/
        //------------------------------------------------------------------------------------//

        public int Minimum
        {
            get => (int)ValueSlider.Minimum;
            set => ValueSlider.Minimum = value;
        }

        public int Maximum
        {
            get => (int)ValueSlider.Maximum;
            set => ValueSlider.Maximum = value;
        }

        public int Value
        {
            get => (int)ValueSlider.Value;
            set => ValueSlider.Value = value;
        }

        public string LabelText
        {
            set => SliderLabel.Text = value;
            get => SliderLabel.Text;
        }

        //------------------------------------------------------------------------------------//
        /*---------------------------------- METHODS -----------------------------------------*/
        //------------------------------------------------------------------------------------//

        public IntSlider()
        {
            InitializeComponent();
            this.DataContext = this;
            UpdateValueLabel();
        }

        private void OnSliderValueChanged()
        {
            if (SliderValueChanged != null)
            {
                SliderValueChanged(this, new ValueChangedEventArgs<int>(Value));
            }
        }

        private void UpdateValueLabel() => ValueField.Text = ValueSlider.Value.ToString();

        private bool ValidateValues()
        {
            Regex regex = new Regex("[^0-9]+");
            bool isValid = regex.IsMatch(ValueField.Text);
            if (isValid)
            {
                int value = (int)float.Parse(ValueField.Text);
                if (value > Maximum) { value = Maximum; }
                if (value < Minimum) { value = Minimum; }

                ValueSlider.Value = value;
                UpdateValueLabel();
            }
            return isValid;
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            bool isValid = ValidateValues();
            e.Handled = isValid;
            OnSliderValueChanged();
        }

        private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateValueLabel();
            OnSliderValueChanged();
        }

        private void OnTextBoxValueChanged(object sender, TextChangedEventArgs e)
        {
            ValidateValues();
            OnSliderValueChanged();
        }
    }
}
