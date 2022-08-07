using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using LEDPiLib;
using LEDPiLib.DataItems;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace DevKit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly LEDPIProcessorDevKit kit;

        private ModulePlaylist _playlist = new ModulePlaylist()
            {ModuleConfigurations = new ObservableCollection<ModuleConfiguration>()};

        public ModulePlaylist Playlist {
            get
            {
                return _playlist;
            }

        }

        public MainWindow()
        {
            InitializeComponent();

            Image<Rgba32> canvas = new Image<Rgba32>(LEDPIProcessorBase.LEDWidth, LEDPIProcessorBase.LEDHeight);
            kit = new LEDPIProcessorDevKit(canvas);
            ComboBox.ItemsSource = Enum.GetValues(typeof(LEDPIProcessorBase.LEDModules)).Cast<LEDPIProcessorBase.LEDModules>().Where(c => c >= 0).OrderBy(b => b.ToString());
            ComboBox.SelectedIndex = 0;
            PlaylistGrid.ItemsSource = _playlist.ModuleConfigurations;
            Loop.IsChecked = _playlist.Loop;

            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);

                    lock (canvas)
                    {
                        BitmapImage bmp;

                        using (var memoryStream = new MemoryStream())
                        {
                            canvas.Save(memoryStream, new PngEncoder());

                            memoryStream.Seek(0, SeekOrigin.Begin);

                            bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.StreamSource = memoryStream;
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.EndInit();
                            bmp.Freeze();
                        }

                        Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                LEDImage.Source = bmp;
                                LEDImageOriginal.Source = bmp;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        });
                    }
                }
            });
        }

        private void Start_OnClick(object sender, RoutedEventArgs e)
        {
            string parameterText = Parameter.Text;

            if (ComboBox.SelectedItem == null)
                return;

            if (((LEDPIProcessorBase.LEDModules) ComboBox.SelectedItem) == LEDPIProcessorBase.LEDModules.Pictures && string.IsNullOrEmpty(parameterText))
                parameterText = "c:\\Temp\\Bilder";

            ModulePlaylist playlist = new ModulePlaylist()
            {
                Loop = false,
                ModuleConfigurations = new ObservableCollection<ModuleConfiguration>()
                {
                    new ModuleConfiguration()
                    {
                        Duration = String.IsNullOrEmpty(Duration.Text) ? 0 : Convert.ToInt32(Duration.Text), Module = (LEDPIProcessorBase.LEDModules) ComboBox.SelectedItem,
                        Parameter = parameterText, OneTime = Onetime.IsChecked != null && Onetime.IsChecked.Value
                    },
                }
            };

            kit.RunModule(playlist, false,ShowFrameRate.IsChecked != null && ShowFrameRate.IsChecked.Value);
        }

        private void ExportButton_OnClick(object sender, RoutedEventArgs e)
        {
            JSON.Text = JsonConvert.SerializeObject(_playlist);
        }

        private void ImportButton_OnClick(object sender, RoutedEventArgs e)
        {
            _playlist = JsonConvert.DeserializeObject<ModulePlaylist>(JSON.Text);
            if (_playlist != null)
            {
                PlaylistGrid.ItemsSource = _playlist.ModuleConfigurations;
                Loop.IsChecked = _playlist.Loop;
            }
        }

        private void Loop_OnClick(object sender, RoutedEventArgs e)
        {
            if (Loop.IsChecked != null) _playlist.Loop = Loop.IsChecked.Value;
        }

        private void RunScript_OnClick(object sender, RoutedEventArgs e)
        {
            kit.RunModule(_playlist);
        }

        private void UpInPlaylist_OnClick(object sender, RoutedEventArgs e)
        {
            if (PlaylistGrid.SelectedItem is ModuleConfiguration selectedConfiguration)
            {
                int currentIndex = _playlist.ModuleConfigurations.IndexOf(selectedConfiguration);
                
                if (currentIndex > 0)
                {
                    _playlist.ModuleConfigurations.Move(currentIndex, currentIndex - 1);
                }
            }
        }

        private void DownInPlaylist_OnClick(object sender, RoutedEventArgs e)
        {
            if (PlaylistGrid.SelectedItem is ModuleConfiguration selectedConfiguration)
            {
                int currentIndex = _playlist.ModuleConfigurations.IndexOf(selectedConfiguration);

                if (currentIndex < _playlist.ModuleConfigurations.Count -1)
                {
                    _playlist.ModuleConfigurations.Move(currentIndex + 1, currentIndex);
                }
            }
        }

        private void DeleteInPlaylist_OnClick(object sender, RoutedEventArgs e)
        {
            if (PlaylistGrid.SelectedItem is ModuleConfiguration selectedConfiguration)
                _playlist.ModuleConfigurations.Remove(selectedConfiguration);
        }
    }
}
