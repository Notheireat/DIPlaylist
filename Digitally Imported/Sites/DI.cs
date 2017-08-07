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
        /// <returns>Пароль</returns>
        public static string GetPass(int x)
        {
            string pass = string.Empty;
            Random r = new Random();
            while (pass.Length < x)
            {
                Char c = (char)r.Next(33, 125);
                if (Char.IsLetterOrDigit(c))
                    pass += c;
            }
            return pass;
        }

        /// <summary>
        /// Обновление cookie
        /// </summary>
        private static void RenewCookie()
        {
            CookieCollection cookies = httpHandler.CookieContainer.GetCookies(new Uri(Settings.DiURL));
            foreach (Cookie co in cookies)
            {
                co.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1));
            }
        }

        /// <summary>
        /// Установка заголовка запроса
        /// </summary>
        /// <param name="type">Тип запроса</param>
        private static void SetHeader(string type)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
            httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:54.0) Gecko/20100101 Firefox/54.0");
            if (type == "GET")
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            }
            else if (type == "POST")
            {
                httpClient.DefaultRequestHeaders.Add("Referer", "https://www.di.fm/join");
                httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                httpClient.DefaultRequestHeaders.Add("Accept", "*/*;q=0.5, text/javascript, application/javascript, application/ecmascript, application/x-ecmascript");
            }
        }

        /// <summary>
        /// Получение токена для регистрации
        /// </summary>
        public static async Task<string> GetTokenAsync()
        {
            RenewCookie();

            string responseBody = await GetContentAsync(Settings.DiURL);
            if (String.IsNullOrEmpty(responseBody))
                return $"Проблема с доступом к {Settings.DiURL}";

            Match match = new Regex(@"""csrf-param""\/>.*content=""(.*?)""", RegexOptions.IgnoreCase).Match(responseBody);
            if (!match.Success)
                return "Ошибка при получении DI-токена";
            Settings.DiToken = match.Groups[1].Value.Trim();

            return "Регистрация нового DI-аккаунта...";
        }

        /// <summary>
        /// Регистрация нового аккаунта
        /// </summary>
        public static async Task<string> GoRegAsync()
        {
            Settings.DiPass = GetPass(10);
            FormUrlEncodedContent formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("utf8", "✓"),
                new KeyValuePair<string, string>("authenticity_token", Settings.DiToken),
                new KeyValuePair<string, string>("member[email]", Settings.TmAddress),
                new KeyValuePair<string, string>("member[password]", Settings.DiPass),
                new KeyValuePair<string, string>("member[password_confirmation]", Settings.DiPass)
            });

            string responseBody = await GetContentAsync($"{Settings.DiURL}member", formContent);
            if (String.IsNullOrEmpty(responseBody))
                return $"Проблема с доступом к {Settings.DiURL}";

            Match match = new Regex("\"auth\":true", RegexOptions.IgnoreCase).Match(responseBody);
            if (!match.Success)
                return "Ошибка при регистрации DI-аккаунта";

            return "Получение письма с активацией...";
        }

        /// <summary>
        /// Получение ключа слушателя
        /// </summary>
        public static async Task<string> GetListenKeyAsync()
        {
            string responseBody = await GetContentAsync(Settings.DiPremiumURL);
            if (String.IsNullOrEmpty(responseBody))
                return $"Проблема с доступом к {Settings.DiURL}";

            Match match = new Regex("\"listenKey\":\"(.*?)\",", RegexOptions.IgnoreCase).Match(responseBody);
            if (!match.Success)
                return "Ошибка при получении ключа слушателя";
            Settings.DiListenKey = match.Groups[1].Value.Trim();

            return "Активация премиум подписки на 7 дней...";
        }

        /// <summary>
        /// Активация премиума и получение JSON-строки с информацией для плейлиста
        /// </summary>
        public static async Task<string> GoPremiumAsync()
        {
            string responseBody = await GetContentAsync($"{Settings.DiURL}member/premium/trial/activate");
            if (String.IsNullOrEmpty(responseBody))
                return $"Проблема с доступом к {Settings.DiURL}";

            Match match = new Regex("di\\.app\\.start\\((.*?)\\);", RegexOptions.IgnoreCase).Match(responseBody);
            if (!match.Success)
                return "Ошибка при активации премиум подписки";
            Settings.DiPlaylistJS = match.Groups[1].Value.Trim();

            return "Генерация плейлиста...";
        }

        /// <summary>
        /// Получение содержимого страницы (GET)
        /// </summary>
        /// <param name="url">Ссылка</param>
        /// <returns>Содержимое веб-страницы</returns>
        private static async Task<string> GetContentAsync(string url)
        {
            SetHeader("GET");

            string responseBody = string.Empty;
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return null;
            }

            return responseBody;
        }

        /// <summary>
        /// Получение содержимого страницы (POST)
        /// </summary>
        /// <param name="url">Ссылка</param>
        /// <param name="data">Параметры запроса</param>
        /// <returns>Содержимое веб-страницы</returns>
        private static async Task<string> GetContentAsync(string url, FormUrlEncodedContent data)
        {
            SetHeader("POST");

            string responseBody = string.Empty;
            try
            {
                HttpResponseMessage response = await httpClient.PostAsync(url, data);
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return null;
            }

            return responseBody;
        }
    }
}
