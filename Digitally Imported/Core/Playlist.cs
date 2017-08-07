using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Digitally_Imported
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

                List<string> channelsInfo = new List<string>
                    {
                        $"#Name:Digitally Imported ({Settings.TmAddress}:{Settings.DiPass})",
                        "#Cursor:-1",
                        $"#Summary:{channels.Count().ToString()} / 00:00:00:00 / 0 B",
                        "#Flags:2047",
                        "#Group:Radio|1"
                    };

                for (int i = 0; i < channels.Length; i++)
                {
                    channelsInfo.Add($"#Track:{i + 1}|http://prem2.di.fm:80/{channels[i]["key"].ToString()}_hi?{Settings.DiListenKey}||||{channels[i]["name"].ToString()}|0|0|||0|0|0");
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
            string patchToFile = $@"{Directory.GetCurrentDirectory()}\Digitally Imported.aimppl";

            try
            {
                File.WriteAllLines(patchToFile, playlist);
            }
            catch (Exception)
            {
                return "Ошибка при сохранении плейлиста";
            }

            return File.Exists(patchToFile) ? "Плейлист успешно сгенерирован!" : "Ошибка при сохранении плейлиста";
        }
    }
}
