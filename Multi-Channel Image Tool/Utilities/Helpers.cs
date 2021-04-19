namespace Multi_Channel_Image_Tool
{
    public static class Helpers
    {
        public static int Max(params int[] values)
        {
            int max = -1;

            foreach (var entry in values)
            {
                if (entry > max) { max = entry; }
            }

            return max;
        }
    }
}