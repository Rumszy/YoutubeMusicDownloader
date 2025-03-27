using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YoutubeMusicDownloader.Models;
using YoutubeMusicDownloader.Services;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using NReco.VideoConverter;

namespace YoutubeMusicDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        //Settings
        public SettingsModel _settingsModel;
        public async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            //Disable UI
            EnableDisableUI();

            //Get Selected Path
            string selectedOption = PathGroup.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked == true).Name.ToString();
            string downloadPath = GetDownloadPath(selectedOption);
            string url = Url.Text;

            //YoutubeExplode Client
            var youtubeClient = new YoutubeClient();
            var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(url);
            var contentData = await youtubeClient.Videos.GetAsync(url);

            string fileName = GetFilenameWithoutSpecialCharacters(contentData.Title);

            // Get the highest bitrate audio-only stream
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            // Get the actual stream
            var stream = await youtubeClient.Videos.Streams.GetAsync(streamInfo);

            // Download the stream to a file
            await youtubeClient.Videos.Streams.DownloadAsync(streamInfo, $"{downloadPath}\\{fileName}.{streamInfo.Container}");

            // Convert video to MP3
            var videoConverter = new FFMpegConverter();
            videoConverter.ConvertMedia($"{downloadPath}\\{fileName}.{streamInfo.Container}", $"{downloadPath}\\{fileName}.mp3", "mp3");

            //Delete Video with Original Format
            File.Delete($"{downloadPath}\\{fileName}.{streamInfo.Container}");

            //Clear URL
            Url.Text = "";

            //Enable UI
            EnableDisableUI();
        }

        public void EnableDisableUI()
        {
            Url.IsEnabled = !Url.IsEnabled;
            btnDownload.IsEnabled = !btnDownload.IsEnabled;
            pathOne.IsEnabled = !pathOne.IsEnabled;
            pathTwo.IsEnabled = !pathTwo.IsEnabled;
            pathThree.IsEnabled = !pathThree.IsEnabled;
            pathFour.IsEnabled = !pathFour.IsEnabled;
            pathFive.IsEnabled = !pathFive.IsEnabled;
            pathSix.IsEnabled = !pathSix.IsEnabled;
            Status.Visibility = Status.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
        }
        public string GetFilenameWithoutSpecialCharacters(string name)
        {
            // Define a list of invalid characters for filenames
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();

            // Remove invalid characters from the input string
            foreach (char c in invalidChars)
            {
                name = name.Replace(c.ToString(), string.Empty);
            }

            return name;
        }
        public string GetDownloadPath(string option)
        {
            switch (option)
            {
                case "pathOne": return _settingsModel.PathOne;
                case "pathTwo": return _settingsModel.PathTwo;
                case "pathThree": return _settingsModel.PathThree;
                case "pathFour": return _settingsModel.PathFour;
                case "pathFive": return _settingsModel.PathFive;
                case "pathSix": return _settingsModel.PathSix;
            }

            return "";
        }
        public void SetPaths()
        {
            pathOne.Content = _settingsModel.PathOne.Split("\\").Last();
            pathTwo.Content = _settingsModel.PathTwo.Split("\\").Last();
            pathThree.Content = _settingsModel.PathThree.Split("\\").Last();
            pathFour.Content = _settingsModel.PathFour.Split("\\").Last();
            pathFive.Content = _settingsModel.PathFive.Split("\\").Last();
            pathSix.Content = _settingsModel.PathSix.Split("\\").Last();
        }

        public MainWindow() 
        {
            //Initialize Component
            InitializeComponent();

            //Load Settings
            SettingsModel settings = JsonConvert.DeserializeObject<SettingsModel>(File.ReadAllText("appsettings.json"));
            _settingsModel = settings;

            //Set Paths
            SetPaths();
        }
    }
}