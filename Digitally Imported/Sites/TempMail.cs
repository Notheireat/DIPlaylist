using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Digitally_Imported
{
    class TempMail
    {
        private static HttpClientHandler httpHandler = new HttpClientHandler
        {
            CookieContainer = new CookieContainer(),
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        private static HttpClient httpClient = new HttpClient(httpHandler)
        {
            Timeout = TimeSpan.FromSeconds(150),
        };

        /// <summary>
        /// Обновление cookie
        /// </summary>
        private static void RenewCookie()
        {
            CookieCollection cookies = httpHandler.CookieContainer.GetCookies(new Uri(Settings.TmURL));
            foreach (Cookie co in cookies)
            {
                co.Expires = DateTime.Now.Subtract(TimeSpan.FromDays(1));
            }
        }

        /// <summary>
        /// Установка заголовака запроса
        /// </summary>
        private static void SetHeader()
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
            httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:54.0) Gecko/20100101 Firefox/54.0");
        }

        /// <summary>
        /// Получение временного почтового адреса
        /// </summary>
        public static async Task<string> GetTempMailAsync()
        {
            RenewCookie();

            string responseBody = await GetContentAsync(Settings.TmURL);
            if (String.IsNullOrEmpty(responseBody))
                return $"Проблема с доступом к {Settings.TmURL}";

            Match match = new Regex("mail opentip\" value=\"(.*?)\"", RegexOptions.IgnoreCase).Match(responseBody);
            if (!match.Success)
                return "Ошибка при получении временной почты";
            Settings.TmAddress = match.Groups[1].Value.Trim();

            return "Получение DI-токена...";
        }

        /// <summary>
        /// Получение письма с премиум-ссылкой
        /// </summary>
        public static async Task<string> GetLetterAsync()
        {
            // 20 попыток с задержкой в 3 секунды на получение письма
            for (int i = 0; i < 20; i++)
            {
                await Task.Delay(3000);

                string responseBody = await GetContentAsync(Settings.TmURL);
                if (String.IsNullOrEmpty(responseBody))
                    return $"Проблема с доступом к {Settings.TmURL}";

                Match match = new Regex("view\\/(.*?)\"", RegexOptions.IgnoreCase).Match(responseBody);
                if (match.Success)
                {
                    Settings.TmLetterID = match.Groups[1].Value.Trim();

                    return "Получение ключа на временный премиум...";
                }
            }

            return "Ошибка при получении письма с активацией";
        }

        /// <summary>
        /// Получение премиум-ссылки из письма
        /// </summary>
        public static async Task<string> GetPremiumURLAsync()
        {
            string responseBody = await GetContentAsync($"{Settings.TmURL}view/{Settings.TmLetterID}");
            if (String.IsNullOrEmpty(responseBody))
                return $"Проблема с доступом к {Settings.TmURL}";

            Match match = new Regex("trial today to enjoy:(.*\n)*<a href=\"(.*?)\" target=\"_blank\" tabindex", RegexOptions.IgnoreCase).Match(responseBody);
            if (!match.Success)
                return "Ошибка при получении премиум ключа";
            Settings.DiPremiumURL = match.Groups[2].Value.Trim();

            return "Получение ключа слушателя...";
        }

        /// <summary>
        /// Получение содержимого страницы (GET)
        /// </summary>
        /// <param name="url">Ссылка</param>
        /// <returns>Содержимое веб-страницы</returns>
        private static async Task<string> GetContentAsync(string url)
        {
            SetHeader();

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
    }
}
