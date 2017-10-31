using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace DIPlaylist
{
    class Playlist
    {
        /// <summary>
        /// Создание и сохранение плейлиста
        /// </summary>
        public static string GoPlaylist()
        {
            List<string> playlist = GeneratePlaylist();

            return playlist != null ? SavePlaylist(playlist) : "Ошибка при обработке плейлиста";
        }

        /// <summary>
        /// Создание плейлиста
        /// </summary>
        /// <returns>Плейлист</returns>
        private static List<string> GeneratePlaylist()
        {
            try
            {
                JToken[] channels = JObject.Parse(Settings.DiPlaylistJS)["channels"].ToArray();
                string[] channelsArray = new string[channels.Count()];
                List<string> channelsInfo = new List<string>();

                if (Settings.PlaylistVersion == 3)
                {
                    channelsInfo.Add($"#Name:Digitally Imported ({Settings.TmAddress}:{Settings.DiPass})");
                    channelsInfo.Add("#Flags:2047");
                    channelsInfo.Add("#Group:Radio|1");

                    for (int i = 0; i < channels.Length; i++)
                    {
                        channelsArray[i] = $"http://prem2.di.fm:80/{channels[i]["key"].ToString()}_hi?{Settings.DiListenKey}||||{channels[i]["name"].ToString()}|0|0|||0|0|0";
                    }
                    Array.Sort(channelsArray);

                    for (int i = 0; i < channelsArray.Length; i++)
                    {
                        channelsInfo.Add($"#Track:{i + 1}|{channelsArray[i]}");
                    }
                }
                else
                {
                    channelsInfo.Add("#-----SUMMARY-----#");
                    channelsInfo.Add($"Name=Digitally Imported{Environment.NewLine}");
                    channelsInfo.Add("#-----SETTINGS-----#");
                    channelsInfo.Add("Flags=2047");
                    channelsInfo.Add("FormatMainLine=%IF(%Artist,%Artist - %Title,%Title)");
                    channelsInfo.Add("FormatSecondLine=%FileFormat :: %SampleRate, %BitRate, %FileSize");
                    channelsInfo.Add($"GroupFormatLine=%FileDir{Environment.NewLine}");
                    channelsInfo.Add("#-----CONTENT-----#");
                    channelsInfo.Add("-Radio");

                    for (int i = 0; i < channels.Length; i++)
                    {
                        channelsArray[i] = $"http://prem2.di.fm:80/{channels[i]["key"].ToString()}_hi?{Settings.DiListenKey}|{channels[i]["name"].ToString()}||||||||||0|0|0|0|0|0|1|{i.ToString()}||";
                    }
                    Array.Sort(channelsArray);

                    for (int i = 0; i < channelsArray.Length; i++)
                    {
                        channelsInfo.Add(channelsArray[i]);
                    }
                }

                return channelsInfo;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Сохранение плейлиста
        /// </summary>
        /// <param name="playlist">Плейлист</param>
        /// <returns>Результат сохранения</returns>
        private static string SavePlaylist(List<string> playlist)
        {
            string savePatch = Settings.PlaylistSavePatch;
            if (String.IsNullOrEmpty(savePatch))
            {
                savePatch = $@"{Directory.GetCurrentDirectory()}\Digitally Imported";
                savePatch += Settings.PlaylistVersion == 3 ? ".aimppl" : ".aimppl4";
            }

            try
            {
                File.WriteAllLines(savePatch, playlist);
            }
            catch (Exception)
            {
                return "Ошибка при сохранении плейлиста";
            }

            return File.Exists(savePatch) ? "Плейлист успешно сгенерирован!" : "Ошибка при сохранении плейлиста";
        }
    }
}
