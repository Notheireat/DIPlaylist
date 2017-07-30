using System;
using System.Net;
using System.Net.Http;
using System.Threading;
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
        public static async Task<string> GetTempMail()
        {
            SetHeader();
            string responseBody = "";
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(Settings.TmURL);
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return "Проблема с доступом к " + Settings.TmURL; ;
            }

            Match match = new Regex("mail opentip\" value=\"(.*?)\"", RegexOptions.IgnoreCase).Match(responseBody);
            if (match.Success)
            {
                Settings.TmAddress = match.Groups[1].Value.Trim();
                return "Получение DI-токена...";
            }
            else
            {
                return "Ошибка при получении временной почты";
            }
        }

        /// <summary>
        /// Получение письма с премиум-ссылкой
        /// </summary>
        public static async Task<string> GetLetter()
        {
            SetHeader();
            // 10 попыток с задержкой в 3 секунды на получение письма
            for (int i = 0; i < 10; i++)
            {
                await Task.Run(() =>
                {
                    Thread.Sleep(3000);
                });
                string responseBody = "";

                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(Settings.TmURL);
                    responseBody = await response.Content.ReadAsStringAsync();
                }
                catch (Exception)
                {
                    return "Проблема с доступом к " + Settings.TmURL;
                }

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
        public static async Task<string> GetPremiumURL()
        {
            SetHeader();
            string responseBody = "";
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(Settings.TmURL + "view/" + Settings.TmLetterID);
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return "Проблема с доступом к " + Settings.TmURL; ;
            }

            Match match = new Regex("Activate your free Premium trial today to enjoy(.*\n)*<a href=\"(.*?)\" target=\"_blank\" tabindex", RegexOptions.IgnoreCase).Match(responseBody);
            if (match.Success)
            {
                Settings.DiPremiumURL = match.Groups[2].Value.Trim();
                return "Активация премиум подписки на 7 дней...";
            }
            else
            {
                return "Ошибка при получении премиум ключа";
            }
        }
    }
}
