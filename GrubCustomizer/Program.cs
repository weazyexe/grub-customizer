using System;
using System.IO;

namespace GrubCustomizer
{
    class Program
    {
        // Настройки GRUB
        private static GrubSettings _grubSettings;
        
        // Бэкап настроек GRUB
        private static GrubSettings _backupGrubSettings;
        
        // Были ли изменения после открытия приложения
        private static bool _isChanged = false;
        
        // Символ для разделения ширины и высоты в разрешении экрана
        private const char SplitSymbol = 'x';
        
        static void Main(string[] args)
        {
            // Создаём объекты настроек и читаем данные с файлов
            _grubSettings = new GrubSettings();
            _backupGrubSettings = new GrubSettings(_grubSettings);

            while (true)
            {
                // Вывод главного меню
                PrintMainMenu();
                
                // Считываем пункт меню
                var menuEntry = Console.ReadLine();

                Console.Clear();
                
                switch (menuEntry)
                {
                    case "1":
                        // Открытие меню настроек графики
                        GraphicsMenuEntry();
                        break;
                    case "2":
                        // Открытие меню настроек загрузки
                        LoadMenuEntry();
                        break;
                    case "3":
                        // Отмена изменений, берем бэкап
                        _grubSettings = new GrubSettings(_backupGrubSettings);
                        Console.WriteLine("Текущие настройки сброшены");
                        Console.ReadKey();
                        break;
                    case "4":
                        // Сбрасываем настройки до стандартных
                        _grubSettings = new GrubSettings(true);
                        _backupGrubSettings = new GrubSettings(_grubSettings);
                        Console.WriteLine("Настройки сброшены к стандартным");
                        Console.ReadKey();
                        break;
                    case "5":
                        // Сохраняем изменения
                        _grubSettings.Save();
                        _isChanged = false;
                        break;
                    case "6":
                        // Сохраняем изменения и выходим
                        _grubSettings.Save();
                        return;
                    case "7":
                        // Просто выходим
                        return;
                    default:
                        // Если ввели что-то кривое
                        Console.WriteLine("Неверный ввод");
                        Console.ReadKey();
                        break;
                }
                
                Console.Clear();
            }
            
        }

        /// <summary>
        /// Пункт меню настроек графики
        /// </summary>
        private static void GraphicsMenuEntry()
        {
            while (true)
            {
                // Вывод меню настроек графики
                PrintGraphicsMenu();
                
                // Считываем пункт меню
                var optionEntry = Console.ReadLine();

                Console.Clear();
                
                switch (optionEntry)
                {
                    case "1":
                        // Вводим путь до файла с темой
                        Console.Write("Введите путь до файла с темой: ");
                        var pathToFile = Console.ReadLine();

                        // Если файл не существует, кидаем ошибку, иначе меняем тему
                        if (!File.Exists(pathToFile))
                        {
                            Console.WriteLine("Такого файла не существует");
                        }
                        else
                        {
                            _grubSettings.Theme = pathToFile;
                            _isChanged = true;
                            Console.WriteLine("Тема изменена");
                        }

                        Console.ReadKey();
                        break;
                    case "2":
                        // Вводим разрешение формата NxM
                        Console.Write($"Введите разрешение в формате ШИРИНА{SplitSymbol}ВЫСОТА (640{SplitSymbol}480, 1440{SplitSymbol}900 и тп): ");
                        var resolution = Console.ReadLine();

                        // Если введенная строка неверного формата, то кидаем ошибку. Иначе - изменяем разрешение
                        if (!StringUtils.IsResolution(resolution, SplitSymbol))
                        {
                            Console.WriteLine("Введённое разрешение имеет неверный формат");
                        }
                        else
                        {
                            _grubSettings.GfxMode = resolution;
                            _isChanged = true;
                            Console.WriteLine("Разрешение изменено");
                        }
                        
                        Console.ReadKey();
                        break;
                    case "3":
                        // Вводим путь до файла с изображением
                        Console.Write("Введите путь до файла с изображением: ");
                        var pathToImage = Console.ReadLine();

                        // Если файла не существует или файл не является изображением, кидаем ошибку. Иначе - меняем background
                        if (!File.Exists(pathToImage) || !StringUtils.IsImage(pathToImage))
                        {
                            Console.WriteLine("Такого файла не существует или файл не является изображением");
                        }
                        else
                        {
                            _grubSettings.Background = pathToImage;
                            _isChanged = true;
                            Console.WriteLine("Фоновое изображение изменено");
                        }
                        
                        Console.ReadKey();
                        break;
                    case "0":
                        // Идем обратно в главное меню
                        return;
                    default:
                        // Если ввод был кривой
                        Console.WriteLine("Неверный ввод.");
                        Console.ReadKey();
                        break;
                }
                
                Console.Clear();
            }
        }

        /// <summary>
        /// Пункт меню настроек загрузки
        /// </summary>
        private static void LoadMenuEntry()
        {
            while (true)
            {
                // Вывод меню, ввод пункта меню
                PrintLoadMenu();
                var optionEntry = Console.ReadLine();
                
                Console.Clear();

                switch (optionEntry)
                {
                    case "1":
                        Console.WriteLine("Выберите пункт по умолчанию (отмена - 0):");
                        
                        // Выводим считанные из файла пункты меню
                        var index = 1;
                        foreach (var entry in _grubSettings.BootMenuEntries)
                            Console.WriteLine($"{index++}. {entry}");
                        
                        Console.Write("> ");
                        int menuEntry;
                        
                        // Считываем данные. Данная конструкция возвращает значение bool, записываем значение в parsed.
                        // Если вернет true - ввод корректный
                        // false - ввод был некорректен
                        var parsed = int.TryParse(Console.ReadLine(), out menuEntry);

                        // Если ввели нормально
                        if (parsed)
                        {
                            // Если хотим выйти обратно в меню
                            if (menuEntry == 0)
                            {
                                break;
                            }
                            
                            // Если ввели число, которое не входит в диапазон возможных значений
                            if (menuEntry > _grubSettings.BootMenuEntries.Count || menuEntry < 0)
                            {
                                Console.WriteLine($"Нет записи с номером {menuEntry}");
                            }
                            else // Иначе меняем
                            {
                                _grubSettings.Default = menuEntry - 1;
                                _isChanged = true;
                                Console.WriteLine($"Пункт по умолчанию выбран {_grubSettings.BootMenuEntries[menuEntry - 1]}");
                            }
                            Console.ReadKey();
                        }
                        else
                        {
                            // Если ввели дичь
                            Console.WriteLine("Некорректный ввод номера");
                            Console.ReadKey();
                        }
                        
                        break;
                    case "2":
                        // Просто меняем SAVE_DEFAULT на противоположный
                        _isChanged = true;
                        _grubSettings.SaveDefault = !_grubSettings.SaveDefault;
                        break;
                    case "3":
                        Console.Write("Введите таймаут (в секундах): ");
                        int timeout;
                        // Объяснение этого выше
                        var isParsed = int.TryParse(Console.ReadLine(), out timeout);

                        if (isParsed)
                        {
                            // Если ввели меньше 0
                            if (timeout < 0)
                            {
                                Console.WriteLine("Таймаут не может быть меньше нуля");
                            }
                            else // Иначе записываем
                            {
                                _grubSettings.Timeout = timeout;
                                _isChanged = true;
                                Console.WriteLine($"Таймаут изменён на значение {timeout}");
                            }
                        }
                        else
                        {
                            // Если ввели дичь
                            Console.WriteLine("Некорректный ввод");
                        }
                        
                        Console.ReadKey();
                        break;
                    case "4":
                        // Вводим стиль
                        Console.Write("Введите стиль (menu, countdown или hidden): ");
                        var style = Console.ReadLine();

                        // Если ввели норм (одно из трех значений)
                        if (style != null && (style == "menu" || style == "hidden" || style == "countdown"))
                        {
                            _grubSettings.TimeoutStyle = style;
                            _isChanged = true;
                            Console.WriteLine($"Стиль таймаута был изменен на значение {style}");
                        }
                        else // Если криво
                        {
                            Console.WriteLine("Некорректный ввод");
                        }
                        
                        Console.ReadKey();
                        break;
                    case "0":
                        return;
                }

                
                Console.Clear();
            }
        }
        
        /// <summary>
        /// Вывод главного меню
        /// </summary>
        private static void PrintMainMenu()
        {
            Console.WriteLine($"GRUB2 Customizer{(_isChanged ? "*" : "")} - главное меню");
            Console.WriteLine("Выберите пункт меню:");
            Console.WriteLine("1. Настройки графики");
            Console.WriteLine("2. Настройки загрузки");
            Console.WriteLine("3. Сбросить текущие настройки");
            Console.WriteLine("4. Сбросить настройки к стандартным");
            Console.WriteLine("5. Сохранить");
            Console.WriteLine("6. Сохранить и выйти");
            Console.WriteLine("7. Выход без сохранения");
            Console.Write("\n> ");
        }

        /// <summary>
        /// Вывод меню изменения графики
        /// </summary>
        private static void PrintGraphicsMenu()
        {
            Console.WriteLine($"GRUB2 Customizer{(_isChanged ? "*" : "")} - настройки графики");
            Console.WriteLine("Выберите пункт меню:");
            Console.WriteLine("1. Тема");
            Console.WriteLine("2. Разрешение");
            Console.WriteLine("3. Изображение на заднем плане");
            Console.WriteLine("0. Назад");
            Console.Write("\n> ");
        }

        /// <summary>
        /// Вывод меню изменения загрузки
        /// </summary>
        private static void PrintLoadMenu()
        {
            Console.WriteLine($"GRUB2 Customizer{(_isChanged ? "*" : "")} - настройки загрузки");
            Console.WriteLine("Выберите пункт меню:");
            Console.WriteLine($"1. Выбранный элемент по умолчанию ({_grubSettings.BootMenuEntries[_grubSettings.Default]})");
            Console.WriteLine($"2. Загружать последний выбор - {_grubSettings.SaveDefault}");
            Console.WriteLine($"3. Таймаут ({_grubSettings.Timeout} секунд)");
            Console.WriteLine($"4. Стиль таймаута ({_grubSettings.TimeoutStyle})");
            Console.WriteLine("0. Назад");
            Console.Write("\n> ");
        }
    }
}