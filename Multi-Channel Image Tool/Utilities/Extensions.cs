namespace Multi_Channel_Image_Tool
{
    public static class Extensions
    {
        public static bool Contains<T>(this T[] array, T toFind)
        {
            foreach (T item in array)
            {
                if (item.Equals(toFind)) { return true; }
            }
            return false;
        }
    }
}