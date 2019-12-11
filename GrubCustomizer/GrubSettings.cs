using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace GrubCustomizer
{
    public class GrubSettings
    {
        private const string PlaceholderFilePath = "/home/weazy/Desktop/grub";
        private const string SettingsFilePath = "/etc/default/grub";
        private const string ConfigFilePath = "/boot/grub/grub.cfg";

        public List<string> BootMenuEntries { get; set; }
        
        public int Default { get; set; } = 0;
        public bool SaveDefault { get; set; } = false;
        public int Timeout { get; set; } = 10;
        public string TimeoutStyle { get; set; } = "menu";
        public string Theme { get; set; }
        public string GfxMode { get; set; }
        public string Background { get; set; }

        private string OthersSettings { get; set; } = "GRUB_DISTRIBUTOR=\"`lsb_release -i -s 2> /dev/null || echo Debian`\"" + '\n' +
                                                 "GRUB_CMDLINE_LINUX=\"\"" + '\n' +
                                                 "GRUB_CMDLINE_LINUX_DEFAULT=\"quiet splash\"" + '\n' +
                                                 "export GRUB_COLOR_NORMAL=\"light-gray/black\"" + '\n' +
                                                 "export GRUB_COLOR_HIGHLIGHT=\"light-green/black\"";

        public GrubSettings()
        {
            ParseGrubConfig();
            ParseGrubSettings();
        }

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
        
        public string ToFile()
        {
            var settings = new StringBuilder();
            settings.Append($"GRUB_DEFAULT=\"{Default}\"\n");
            settings.Append($"GRUB_SAVEDEFAULT=\"{SaveDefault}\"\n");
            settings.Append($"GRUB_TIMEOUT=\"{Timeout}\"\n");
            settings.Append($"GRUB_TIMEOUT_STYLE=\"{TimeoutStyle}\"\n");
            settings.Append($"GRUB_THEME=\"{Theme}\"\n");
            settings.Append($"GRUB_GFXMODE=\"{GfxMode}\"\n");
            settings.Append($"GRUB_BACKGROUND=\"{Background}\"\n");
            settings.Append(OthersSettings);
            
            return settings.ToString();
        }

        public void Save()
        {
            try
            {
                Console.WriteLine("Запись новых настроек в файл . . .");
                var content = ToFile();
                using var streamWriter = new StreamWriter(SettingsFilePath);
                streamWriter.WriteLine(content);
                Console.WriteLine("Настройки записаны в файл");
                Console.WriteLine("Сохранение настроек GRUB2 . . .");
                Process.Start("sudo", "update-grub")?.WaitForExit();
                Console.WriteLine("Сохранено!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        private void ParseGrubSettings()
        {
            var content = new List<string>();
            using (var streamReader = new StreamReader(SettingsFilePath))
            {
                var other = new StringBuilder();
                while (!streamReader.EndOfStream)
                {
                    var readed = streamReader.ReadLine();

                    if (!string.IsNullOrEmpty(readed) && !readed.StartsWith("#"))
                    {
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
                            other.Append($"{readed}\n");
                        }
                    }
                }

                OthersSettings = other.ToString();
            }
        }
        
        private void ParseGrubConfig()
        {
            var content = new List<string>();
            using (var streamReader = new StreamReader(ConfigFilePath))
            {
                // Считываем конфиг граба
                while (!streamReader.EndOfStream)
                {
                    var readed = streamReader.ReadLine();

                    if (readed != null && (readed.StartsWith("menuentry") || readed.StartsWith("submenu")))
                    {
                        content.Add(readed);
                    }
                }
                
                BootMenuEntries = new List<string>();

                foreach (var str in content)
                {
                    BootMenuEntries.Add(StringUtils.GetBootMenuEntry(str));
                }
            }
        }
    }
}