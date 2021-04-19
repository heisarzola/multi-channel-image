using System;

namespace Multi_Channel_Image_Tool
{
    public class ValueChangedEventArgs<T> : EventArgs
    {
        private readonly T _value;
        public T Value => _value;
        public ValueChangedEventArgs(T value) => _value = value;
    }
}