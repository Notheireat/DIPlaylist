﻿namespace DIPlaylist
{
    class Settings
    {
        /// <summary>
        /// Актуальная ссылка на Temp-mail
        /// </summary>
        public static string TmURL { get; set; } = "https://temp-mail.org/en/";

        /// <summary>
        /// Временный почтовый адрес
        /// </summary>
        public static string TmAddress { get; set; }

        /// <summary>
        /// Идентификатор письма с премиум-ссылкой
        /// </summary>
        public static string TmLetterID { get; set; }

        /// <summary>
        /// Актуальная ссылка на DI.fm
        /// </summary>
        public static string DiURL { get; set; } = "https://www.di.fm/";

        /// <summary>
        /// Токен для регистрации
        /// </summary>
        public static string DiToken { get; set; }

        /// <summary>
        /// Пароль от аккаунта
        /// </summary>
        public static string DiPass { get; set; }

        /// <summary>
        /// Премиум-ссылка из письма
        /// </summary>
        public static string DiPremiumURL { get; set; }

        /// <summary>
        /// Ключ слушателя
        /// </summary>
        public static string DiListenKey { get; set; }

        /// <summary>
        /// JSON-строка с информацией для плейлиста
        /// </summary>
        public static string DiPlaylistJS { get; set; }

        /// <summary>
        /// Версия плейлиста
        /// </summary>
        public static int PlaylistVersion { get; set; }

        /// <summary>
        /// Путь сохранения плейлиста
        /// </summary>
        public static string PlaylistSavePatch { get; set; }
    }
}
