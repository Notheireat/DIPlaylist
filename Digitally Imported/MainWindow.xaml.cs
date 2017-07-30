using System;
using System.Windows;
using System.Windows.Media;

namespace Digitally_Imported
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Настройка шрифта для статусного lable'а и установка активности кнопки
        /// </summary>
        /// <param name="status">Статус операции</param>
        private void Status(bool status)
        {
            if (status)
            {
                statusLabel.Foreground = Brushes.Green;
                statusLabel.FontWeight = FontWeights.Bold;
            }
            else
            {
                statusLabel.Foreground = Brushes.Red;
            }
            btnStart.IsEnabled = true;
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            progressBar.Value = 0;
            statusLabel.FontWeight = FontWeights.Normal;
            statusLabel.Foreground = Brushes.Black;
            statusLabel.Content = "Создание временного почтового адреса...";

            progressBar.Value++;
            // Получение временного почтового адреса
            statusLabel.Content = await TempMail.GetTempMail();
            if (String.IsNullOrEmpty(Settings.TmAddress))
            {
                Status(false);
                return;
            }

            progressBar.Value++;
            // Получение токена для регистрации
            statusLabel.Content = await DI.GetToken();
            if (String.IsNullOrEmpty(Settings.DiToken))
            {
                Status(false);
                return;
            }

            progressBar.Value++;
            // Регистрация нового аккаунта
            statusLabel.Content = await DI.GoReg();
            if (statusLabel.Content.ToString().ToLower().Contains("di"))
            {
                Status(false);
                return;
            }

            progressBar.Value++;
            // Получение письма с премиум-ссылкой
            statusLabel.Content = await TempMail.GetLetter();
            if (String.IsNullOrEmpty(Settings.TmLetterID))
            {
                Status(false);
                return;
            }

            progressBar.Value++;
            // Получение премиум-ссылки из письма
            statusLabel.Content = await TempMail.GetPremiumURL();
            if (String.IsNullOrEmpty(Settings.DiPremiumURL))
            {
                Status(false);
                return;
            }

            progressBar.Value++;
            // Активация премиума и получение JSON-строки с информацией для плейлиста
            statusLabel.Content = await DI.GoPremium();
            if (String.IsNullOrEmpty(Settings.DiPlaylistJS))
            {
                Status(false);
                return;
            }

            progressBar.Value++;
            // Создание и сохранение плейлиста
            statusLabel.Content = await Playlist.GoPlaylist();
            if (statusLabel.Content.ToString().ToLower().Contains("ошибка"))
            {
                Status(false);
                return;
            }
            Status(true);
        }
    }
}
