using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GrubCustomizer
{
    public class GrubSettings
    {
        // Путь до файла настроек
        private const string SettingsFilePath = "/etc/default/grub";
        
        // Путь до файла конфигурации (оттуда берем названия пунктов меню загрузки)
        private const string ConfigFilePath = "/boot/grub/grub.cfg";

        // Пункты меню загрузки
        public List<string> BootMenuEntries { get; set; }
        
        // Какой пункт будет выбран по умолчанию
        public int Default { get; set; } = 0;
        
        // Загружать последний выбранный по умолчанию сразу
        public bool SaveDefault { get; set; } = false;
        
        // Время ожидания выбора пункта меню
        public int Timeout { get; set; } = 10;
        
        // ??? Олег, разберись!
        public string TimeoutStyle { get; set; } = "menu";
        
        // Тема GRUB
        public string Theme { get; set; }
        
        // Разрешение
        public string GfxMode { get; set; }
        
        // Изображение на заднем плане
        public string Background { get; set; }

        // Прочие настройки, которые не трогаем
        private string OthersSettings { get; set; } = "GRUB_DISTRIBUTOR=\"`lsb_release -i -s 2> /dev/null || echo Debian`\"" + '\n' +
                                                 "GRUB_CMDLINE_LINUX=\"\"" + '\n' +
                                                 "GRUB_CMDLINE_LINUX_DEFAULT=\"quiet splash\"" + '\n' +
                                                 "export GRUB_COLOR_NORMAL=\"light-gray/black\"" + '\n' +
                                                 "export GRUB_COLOR_HIGHLIGHT=\"light-green/black\"";

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="isBackup">Нужно ли делать полный бэкап</param>
        public GrubSettings(bool isBackup = false)
        {
            // Если не надо делать полный бэкап, считываем файлы из системы
            // Иначе берём стандартные значения, которые заданы выше
            if (!isBackup)
            {
                ParseGrubConfig();    // Считываем файл /boot/grub/grub.cfg
                ParseGrubSettings();    // Считываем файл /etc/default/grub
            }
        }

        /// <summary>
        /// Конструктор копирования
        /// </summary>
        /// <param name="grubSettings"></param>
        public GrubSettings(GrubSettings grubSettings)
        {
            Default = grubSettings.Default;
            SaveDefault = grubSettings.SaveDefault;
            Background = grubSettings.Background;
            Theme = grubSettings.Theme;
            Timeout = grubSettings.Timeout;
            GfxMode = grubSettings.GfxMode;
            OthersSettings = grubSettings.OthersSettings;
            TimeoutStyle = grubSettings.TimeoutStyle;
            BootMenuEntries = grubSettings.BootMenuEntries;
        }

        /// <summary>
        /// Преобразование настроек в формат настроек файла GRUB (/etc/default/grub)
        /// </summary>
        /// <returns></returns>
        private string ToFile()
        {
            // StringBuilder - это как String, только чутка производительней и его изменять можно
            var settings = new StringBuilder();
            
            // Append - вставить строку в конец
            settings.Append($"GRUB_DEFAULT=\"{Default}\"\n");
            settings.Append($"GRUB_SAVEDEFAULT=\"{SaveDefault}\"\n");
            settings.Append($"GRUB_TIMEOUT=\"{Timeout}\"\n");
            settings.Append($"GRUB_TIMEOUT_STYLE=\"{TimeoutStyle}\"\n");
            settings.Append($"GRUB_THEME=\"{Theme}\"\n");
            settings.Append($"GRUB_GFXMODE=\"{GfxMode}\"\n");
            settings.Append($"GRUB_BACKGROUND=\"{Background}\"\n");
            settings.Append(OthersSettings);
            
            // Возвращаем полученные настройки
            return settings.ToString();
        }

        /// <summary>
        /// Сохранить текущие настройки GRUB
        /// </summary>
        public void Save()
        {
            try
            {
                Console.WriteLine("Запись новых настроек в файл . . .");
                var content = ToFile();
                
                // Открываем файл /etc/default/grub для записи
                using var streamWriter = new StreamWriter(SettingsFilePath);
                
                // Пишем туда всё, что есть
                streamWriter.WriteLine(content);
                
                // Выводим юзеру статус
                Console.WriteLine("Настройки записаны в файл");
                Console.WriteLine("Сохранение настроек GRUB2 . . .");
                
                // Запускаем апдейт настроек GRUB
                // ? - это проверка на null. Эквивалентно следующему коду:
                // var process = Process.Start(...)
                // if (process != null) { process.WaitForExit() }
                Process.Start("sudo", "update-grub")?.WaitForExit();
                Console.WriteLine("Сохранено!");
            }
            catch (Exception ex)
            {
                // При ошибке выводим сообщение
                // Обычно выскакивает, когда пытаешься открыть файл без прав админа
                // Поэтому открывать прогу через sudo
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Парсим /etc/default/grub
        /// </summary>
        private void ParseGrubSettings()
        {
            // В content будем писать считанные строки
            var content = new List<string>();
            
            // Открываем файл для чтения
            using (var streamReader = new StreamReader(SettingsFilePath))
            {
                var other = new StringBuilder();
                
                // Пока файл не закончился
                while (!streamReader.EndOfStream)
                {
                    // Считываем строку
                    var readed = streamReader.ReadLine();

                    // Если строка не пуста и не начинается с # (не комментарий)
                    if (!string.IsNullOrEmpty(readed) && !readed.StartsWith("#"))
                    {
                        // StartsWith - проверка начинается ли строка с заданной строки
                        if (readed.StartsWith("GRUB_DEFAULT="))
                        {
                            Default = int.Parse(StringUtils.GetValueFrom(readed));
                        }
                        else if (readed.StartsWith("GRUB_SAVEDEFAULT="))
                        {
                            SaveDefault = bool.Parse(StringUtils.GetValueFrom(readed));
                        }
                        else if (readed.StartsWith("GRUB_TIMEOUT="))
                        {
                            Timeout = int.Parse(StringUtils.GetValueFrom(readed));
                        }
                        else if (readed.StartsWith("GRUB_TIMEOUT_STYLE="))
                        {
                            TimeoutStyle = StringUtils.GetValueFrom(readed);
                        }
                        else if (readed.StartsWith("GRUB_THEME="))
                        {
                            Theme = StringUtils.GetValueFrom(readed);
                        }
                        else if (readed.StartsWith("GRUB_GFXMODE="))
                        {
                            GfxMode = StringUtils.GetValueFrom(readed);
                        }
                        else if (readed.StartsWith("GRUB_BACKGROUND="))
                        {
                            Background = StringUtils.GetValueFrom(readed);
                        }
                        else
                        {
                            // Если программа не обрабатывает параметр, пишем его в others
                            // others не изменяет программа и просто запишет в конце в файл без изменений
                            other.Append($"{readed}\n");
                        }
                    }
                }

                // Присваиваем others
                OthersSettings = other.ToString();
            }
        }
        
        /// <summary>
        /// Парсим /boot/grub/grub.cfg
        /// </summary>
        private void ParseGrubConfig()
        {
            // content - пункты меню, которые мы считывать будем
            var content = new List<string>();
            
            // Открываем файл для чтения
            using (var streamReader = new StreamReader(ConfigFilePath))
            {
                // Пока не конец файла
                while (!streamReader.EndOfStream)
                {
                    var readed = streamReader.ReadLine();

                    // Здесь нам нужно найти строку формата "menuentry 'XXX' YYY" или submenu 'XXX' YYY
                    // Потому что в этих строках находятся названия пунктов меню
                    if (readed != null && (readed.StartsWith("menuentry") || readed.StartsWith("submenu")))
                    {
                        // Если формат нормальный, запишем в content
                        content.Add(readed);
                    }
                }
                
                // Выделяем память под пункты меню
                BootMenuEntries = new List<string>();

                // Парсим каждый пункт. Забираем из него строку XXX
                foreach (var str in content)
                {
                    BootMenuEntries.Add(StringUtils.GetBootMenuEntry(str));
                }
            }
        }
    }
}