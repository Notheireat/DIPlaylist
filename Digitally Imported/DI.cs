using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Digitally_Imported
{
    class DI
    {
        private static HttpClientHandler httpHandler = new HttpClientHandler
        {
            CookieContainer = new CookieContainer(),
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        private static HttpClient httpClient = new HttpClient(httpHandler)
        {
            Timeout = TimeSpan.FromSeconds(150)
        };

        /// <summary>
        /// Генерация пароля
        /// </summary>
        /// <param name="x">Количество символов</param>
        /// <returns></returns>
        public static string GetPass(int x)
        {
            string pass = "";
            var r = new Random();
            while (pass.Length < x)
            {
                Char c = (char)r.Next(33, 125);
                if (Char.IsLetterOrDigit(c))
                    pass += c;
            }
            return pass;
        }

        /// <summary>
        /// Установка шапки запроса
        /// </summary>
        /// <param name="type">Тип запроса</param>
        private static void SetHeader(string type)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
            httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:54.0) Gecko/20100101 Firefox/54.0");
            switch (type)
            {
                case "GET":
                    httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                    break;
                case "POST":
                    httpClient.DefaultRequestHeaders.Add("Referer", "https://www.di.fm/join");
                    httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    httpClient.DefaultRequestHeaders.Add("Accept", "*/*;q=0.5, text/javascript, application/javascript, application/ecmascript, application/x-ecmascript");
                    break;
            }
        }

        /// <summary>
        /// Получение токена для регистрации
        /// </summary>
        public static async Task<string> GetToken()
        {
            SetHeader("GET");
            string responseBody = "";
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(Settings.DiURL);
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return "Проблема с доступом к " + Settings.DiURL;
            }

            Match match = new Regex(@"""csrf-param""\/>.*content=""(.*?)""", RegexOptions.IgnoreCase).Match(responseBody);
            if (match.Success)
            {
                Settings.DiToken = match.Groups[1].Value.Trim();
                return "Регистрация нового DI-аккаунта...";
            }
            else
            {
                return "Ошибка при получении DI-токена";
            }
        }

        /// <summary>
        /// Регистрация нового аккаунта
        /// </summary>
        public static async Task<string> GoReg()
        {
            SetHeader("POST");
            Settings.DiPass = GetPass(10);
            FormUrlEncodedContent formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("utf8", "✓"),
                new KeyValuePair<string, string>("authenticity_token", Settings.DiToken),
                new KeyValuePair<string, string>("member[email]", Settings.TmAddress),
                new KeyValuePair<string, string>("member[password]", Settings.DiPass),
                new KeyValuePair<string, string>("member[password_confirmation]", Settings.DiPass)
            });
            string responseBody = "";
            try
            {
                HttpResponseMessage response = await httpClient.PostAsync(Settings.DiURL + "member", formContent);
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return "Проблема с доступом к " + Settings.DiURL;
            }

            if (responseBody.Contains("\"auth\":true"))
            {
                return "Получение новой почты...";
            }
            else
            {
                return "Ошибка при регистрации DI-аккаунта";
            }
        }

        /// <summary>
        /// Активация премиума и получение JSON-строки с информацией для плейлиста
        /// </summary>
        public static async Task<string> GoPremium()
        {
            SetHeader("GET");
            string responseBody = "";
            try
            {
                // Активация премиум ссылки из письма
                HttpResponseMessage response = await httpClient.GetAsync(Settings.DiPremiumURL);
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return "Проблема с доступом к " + Settings.DiURL; ;
            }
            
            Match match = new Regex("\"listenKey\":\"(.*?)\",", RegexOptions.IgnoreCase).Match(responseBody);
            if (match.Success)
            {
                try
                {
                    // Активация премиума
                    HttpResponseMessage response = await httpClient.GetAsync(Settings.DiURL + "member/premium/trial/activate");
                }
                catch (Exception)
                {
                    return "Проблема с доступом к " + Settings.DiURL;
                }
                Settings.DiListenKey = match.Groups[1].Value.Trim();

                // Получение JSON плейлиста
                match = new Regex("di\\.app\\.start\\((.*?)\\);", RegexOptions.IgnoreCase).Match(responseBody);
                if (match.Success)
                {
                    Settings.DiPlaylistJS = match.Groups[1].Value.Trim();
                    return "Генерация плейлиста...";
                }
                else
                {
                    return "Ошибка при получении настроек плейлиста";
                }
            }
            else
            {
                return "Ошибка при активация премиума";
            }
        }
    }
}
