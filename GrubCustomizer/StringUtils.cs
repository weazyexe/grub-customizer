namespace GrubCustomizer
{
    public static class StringUtils
    {
        /// <summary>
        /// Проверка, является ли входная строка вида NxM
        /// </summary>
        /// <param name="resolution">Входная строка</param>
        /// <param name="splitSymbol">Символ разделения (x по дефолту)</param>
        /// <returns></returns>
        public static bool IsResolution(string resolution, char splitSymbol)
        {
            if (resolution == null) return false;
            
            // Если не содержит splitSymbol
            if (!resolution.Contains(splitSymbol)) return false;

            // Если строка содержит не цифру (кроме splitSymbol!)
            foreach (var symbol in resolution)
                if (!char.IsDigit(symbol) && symbol != splitSymbol) return false;

            // Разделяем строку по splitSymbol
            // Пример Split при splitSymbol = ' '
            // "Олег гей" => "Олег", "гей"
            var splitted = resolution.Split(splitSymbol);
            
            // Если длина разделенного массива не равняется 2
            if (splitted.Length != 2) return false;
            
            // Если значения некорректны
            if (splitted[0] == "" || splitted[1] == "") return false;

            
            // Если все условия проверки пройдены, возвращаем true
            return true;
        }

        /// <summary>
        /// Проверка расширения файла - является ли файл изображением
        /// </summary>
        /// <param name="image">Путь до файла</param>
        /// <returns></returns>
        public static bool IsImage(string image)
        {
            if (image == null) return false;
            if (!image.Contains(".jpg") && !image.Contains(".png") && !image.Contains(".jpeg") &&
                !image.Contains(".bmp"))
                return false;
            
            return true;
        }

        /// <summary>
        /// Берет из строки типа menuentry 'Linux' random text строку Linux
        /// </summary>
        /// <param name="rawString"></param>
        /// <returns></returns>
        public static string GetBootMenuEntry(string rawString)
        {
            var splitted = rawString.Split('\'');
            return splitted[1];
        }

        /// <summary>
        /// Берет из строки формата GRUB_DEFAULT="1" строку 1
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetValueFrom(string str)
        {
            var splitted = str.Split('\"');
            return splitted[1];
        }
    }
}