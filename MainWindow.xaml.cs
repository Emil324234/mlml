using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Ookii.Dialogs.Wpf;
using System.IO;
using System.Windows.Forms;

namespace AudioPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private List<string> tracks = new List<string>();
        private int currentTrackIndex = -1;
        private bool isPlaying = false;
        private bool isRepeating = false;
        private bool isShuffling = false;

        public MainWindow()
        {
            InitializeComponent();

            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
        }

        private void ChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == true)
            {
                tracks.Clear();
                tracks.AddRange(Directory.GetFiles(folderBrowserDialog.SelectedPath, "*.mp3"));
                trackListBox.ItemsSource = tracks;
                trackListBox.SelectedIndex = 0;
            }
        }

        private void TrackListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (trackListBox.SelectedIndex != -1)
            {
                currentTrackIndex = trackListBox.SelectedIndex;
                PlayTrack();
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                mediaPlayer.Pause();
                isPlaying = false;
                playPauseButton.Content = "Включить";
            }
            else
            {
                mediaPlayer.Play();
                isPlaying = true;
                playPauseButton.Content = "Пауза";
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentTrackIndex > 0)
            {
                currentTrackIndex--;
                PlayTrack();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentTrackIndex < tracks.Count - 1)
            {
                currentTrackIndex++;
                PlayTrack();
            }
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            isRepeating = !isRepeating;
            repeatButton.Content = isRepeating ? "Повторить (Вкл)" : "Повторить (Выкл)";
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            isShuffling = !isShuffling;
            shuffleButton.Content = isShuffling ? "Перемешать (Вкл)" : "Перемешать (Выкл)";
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Position = TimeSpan.FromSeconds(positionSlider.Value);
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = volumeSlider.Value;
        }

        private void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            trackInfoTextBlock.Text = $"Сейчас играет: {tracks[currentTrackIndex]}";
            durationTextBlock.Text = $"Продолжительность: {mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss")}";
            positionSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            isPlaying = true;
            playPauseButton.Content = "Пауза";
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            if (isRepeating)
            {
                PlayTrack();
            }
            else if (isShuffling)
            {
                Random random = new Random();
                currentTrackIndex = random.Next(0, tracks.Count);
                PlayTrack();
            }
            else if (currentTrackIndex < tracks.Count - 1)
            {
                currentTrackIndex++;
                PlayTrack();
            }
            else
            {
                isPlaying = false;
                playPauseButton.Content = "Начать";
            }
        }

        private void MediaPlayer_MediaFailed(object sender, ExceptionEventArgs e)
        {
            MessageBox.Show($"Ошибка: {e.ErrorException.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            currentTimeTextBlock.Text = $"Текущая продолжительность: {mediaPlayer.Position.ToString(@"mm\:ss")}";
            positionSlider.Value = mediaPlayer.Position.TotalSeconds;
        }

        private void PlayTrack()
        {
            mediaPlayer.Open(new Uri(tracks[currentTrackIndex]));
            mediaPlayer.Play();
            trackListBox.SelectedIndex = currentTrackIndex;
        }
    }
}
