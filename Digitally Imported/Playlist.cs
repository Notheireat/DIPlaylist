using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Digitally_Imported
{
    class Playlist
    {
        /// <summary>
        /// Создание и сохранение плейлиста
        /// </summary>
        public static async Task<string> GoPlaylist()
        {
            try
            {
                List<string> channelsInfo = new List<string>();
                channelsInfo.Add($"#Name:Digitally Imported ({Settings.TmAddress}:{Settings.DiPass})");
                channelsInfo.Add("#Cursor:-1");

                JObject playlistJS = JObject.Parse(Settings.DiPlaylistJS);
                JToken[] channels = playlistJS["channels"].ToArray();

                channelsInfo.Add($"#Summary:{channels.Count().ToString()} / 00:00:00:00 / 0 B");
                channelsInfo.Add("#Flags:2047");
                channelsInfo.Add("#Group:Radio|1");

                int count = 0;
                return await Task.Run(() =>
                {
                    foreach (var data in channels)
                    {
                        count++;
                        channelsInfo.Add($"#Track:{count}|http://prem2.di.fm:80/{data["key"].ToString()}_hi?{Settings.DiListenKey}||||{data["name"].ToString()}|0|0|||0|0|0");
                    }

                    File.WriteAllLines($@"{Directory.GetCurrentDirectory()}\Digitally Imported.aimppl", channelsInfo);
                    return "Плейлист успешно сгенерирован!";
                });
            }
            catch (Exception)
            {
                return "Ошибка при обработке плейлиста";
            }
        }
    }
}
