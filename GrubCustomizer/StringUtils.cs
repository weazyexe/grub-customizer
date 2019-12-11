namespace GrubCustomizer
{
    public static class StringUtils
    {
        public static bool IsResolution(string resolution, char splitSymbol)
        {
            if (resolution == null) return false;
            if (!resolution.Contains(splitSymbol)) return false;

            foreach (var symbol in resolution)
                if (!char.IsDigit(symbol) && symbol != splitSymbol) return false;

            var splitted = resolution.Split(splitSymbol);
            
            if (splitted.Length != 2) return false;
            if (splitted[0] == "" || splitted[1] == "") return false;

            return true;
        }

        public static bool IsImage(string image)
        {
            if (image == null) return false;
            if (!image.Contains(".jpg") && !image.Contains(".png") && !image.Contains(".jpeg") &&
                !image.Contains(".bmp"))
                return false;
            
            return true;
        }

        public static string GetBootMenuEntry(string rawString)
        {
            var splitted = rawString.Split('\'');
            return splitted[1];
        }

        public static string GetValueFrom(string str)
        {
            var splitted = str.Split('\"');
            return splitted[1];
        }
    }
}