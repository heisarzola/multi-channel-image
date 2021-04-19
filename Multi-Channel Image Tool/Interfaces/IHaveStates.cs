using System;

namespace Multi_Channel_Image_Tool.Interfaces
{
    public interface IHaveStates
    {
        event EventHandler StateChanged;
    }
}