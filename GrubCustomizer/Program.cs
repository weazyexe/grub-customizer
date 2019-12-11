using System;
using System.IO;

namespace GrubCustomizer
{
    class Program
    {
        private static GrubSettings _grubSettings;
        private static GrubSettings _backupGrubSettings;
        private static bool _isChanged = false;
        
        private const char SplitSymbol = 'x';
        
        static void Main(string[] args)
        {
            _grubSettings = new GrubSettings();
            _backupGrubSettings = new GrubSettings(_grubSettings);

            while (true)
            {
                PrintMainMenu();
                var menuEntry = Console.ReadLine();

                Console.Clear();
                
                switch (menuEntry)
                {
                    case "1":
                        GraphicsMenuEntry();
                        break;
                    case "2":
                        LoadMenuEntry();
                        break;
                    case "3":
                        _grubSettings = new GrubSettings(_backupGrubSettings);
                        Console.WriteLine("Текущие настройки сброшены");
                        Console.ReadKey();
                        break;
                    case "4":
                        _grubSettings = new GrubSettings();
                        _backupGrubSettings = new GrubSettings(_grubSettings);
                        Console.WriteLine("Настройки сброшены к стандартным");
                        Console.ReadKey();
                        break;
                    case "5":
                        _grubSettings.Save();
                        _isChanged = false;
                        break;
                    case "6":
                        _grubSettings.Save();
                        return;
                    case "7":
                        return;
                    default:
                        Console.WriteLine("Неверный ввод");
                        Console.ReadKey();
                        break;
                }
                
                Console.Clear();
            }
            
        }

        private static void GraphicsMenuEntry()
        {
            while (true)
            {
                PrintGraphicsMenu();
                var optionEntry = Console.ReadLine();

                Console.Clear();
                
                switch (optionEntry)
                {
                    case "1":
                        Console.Write("Введите путь до файла с темой: ");
                        var pathToFile = Console.ReadLine();

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
                        Console.Write($"Введите разрешение в формате ШИРИНА{SplitSymbol}ВЫСОТА (640{SplitSymbol}480, 1440{SplitSymbol}900 и тп): ");
                        var resolution = Console.ReadLine();

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
                        Console.Write("Введите путь до файла с изображением: ");
                        var pathToImage = Console.ReadLine();

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
                        return;
                    default:
                        Console.WriteLine("Неверный ввод.");
                        Console.ReadKey();
                        break;
                }
                
                Console.Clear();
            }
        }

        private static void LoadMenuEntry()
        {
            while (true)
            {
                PrintLoadMenu();
                var optionEntry = Console.ReadLine();
                
                Console.Clear();

                switch (optionEntry)
                {
                    case "1":
                        Console.WriteLine("Выберите пункт по умолчанию (отмена - 0):");
                        
                        var index = 1;
                        foreach (var entry in _grubSettings.BootMenuEntries)
                            Console.WriteLine($"{index++}. {entry}");
                        
                        Console.Write("> ");
                        int menuEntry;
                        var parsed = int.TryParse(Console.ReadLine(), out menuEntry);

                        if (parsed)
                        {
                            if (menuEntry == 0)
                            {
                                break;
                            }
                            
                            if (menuEntry > _grubSettings.BootMenuEntries.Count || menuEntry < 0)
                            {
                                Console.WriteLine($"Нет записи с номером {menuEntry}");
                            }
                            else
                            {
                                _grubSettings.Default = menuEntry - 1;
                                _isChanged = true;
                                Console.WriteLine($"Пункт по умолчанию выбран {_grubSettings.BootMenuEntries[menuEntry - 1]}");
                            }
                            Console.ReadKey();
                        }
                        else
                        {
                            Console.WriteLine("Некорректный ввод номера");
                            Console.ReadKey();
                        }
                        
                        break;
                    case "2":
                        _isChanged = true;
                        _grubSettings.SaveDefault = !_grubSettings.SaveDefault;
                        break;
                    case "3":
                        Console.Write("Введите таймаут (в секундах): ");
                        int timeout;
                        var isParsed = int.TryParse(Console.ReadLine(), out timeout);

                        if (isParsed)
                        {
                            if (timeout < 0)
                            {
                                Console.WriteLine("Таймаут не может быть меньше нуля");
                            }
                            else
                            {
                                _grubSettings.Timeout = timeout;
                                _isChanged = true;
                                Console.WriteLine($"Таймаут изменён на значение {timeout}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Некорректный ввод");
                        }
                        
                        Console.ReadKey();
                        break;
                    case "4":
                        Console.Write("Введите стиль (menu, countdown или hidden): ");
                        var style = Console.ReadLine();

                        if (style != null && (style == "menu" || style == "hidden" || style == "countdown"))
                        {
                            _grubSettings.TimeoutStyle = style;
                            _isChanged = true;
                            Console.WriteLine($"Стиль таймаута был изменен на значение {style}");
                        }
                        else
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