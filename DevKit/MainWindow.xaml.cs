using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using LEDPiDevKit;
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
        private readonly Image<Rgba32> _canvas;
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

            _canvas = new Image<Rgba32>(LEDPIProcessorBase.LEDWidth, LEDPIProcessorBase.LEDHeight);
            kit = new LEDPIProcessorDevKit(_canvas);
            ComboBox.ItemsSource = Enum.GetValues(typeof(LEDPIProcessorBase.LEDModules)).Cast<LEDPIProcessorBase.LEDModules>();
            ComboBox.SelectedItem = LEDPIProcessorBase.LEDModules.None;
            PlaylistGrid.ItemsSource = _playlist.ModuleConfigurations;
            Loop.IsChecked = _playlist.Loop;

            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(new TimeSpan(0, 0, 0, 0, 10));

                    lock (_canvas)
                    {
                        BitmapImage bmp;

                        try
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                _canvas.Save(memoryStream, new PngEncoder());

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
                                }
                            });

                        }
                        finally
                        {
                        }
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
                        Duration = 0, Module = (LEDPIProcessorBase.LEDModules) ComboBox.SelectedItem,
                        Parameter = parameterText, OneTime = Onetime.IsChecked.Value
                    },
                }
            };

            kit.RunModule(playlist, false,ShowFrameRate.IsChecked.Value);
        }

        private void ExportButton_OnClick(object sender, RoutedEventArgs e)
        {
            JSON.Text = JsonConvert.SerializeObject(_playlist);
        }

        private void ImportButton_OnClick(object sender, RoutedEventArgs e)
        {
            _playlist = JsonConvert.DeserializeObject<ModulePlaylist>(JSON.Text);
            PlaylistGrid.ItemsSource = _playlist.ModuleConfigurations;
            Loop.IsChecked = _playlist.Loop;
        }

        private void Loop_OnClick(object sender, RoutedEventArgs e)
        {
            _playlist.Loop = Loop.IsChecked.Value;
        }

        private void RunScript_OnClick(object sender, RoutedEventArgs e)
        {
            kit.RunModule(_playlist);
        }

        private void UpInPlaylist_OnClick(object sender, RoutedEventArgs e)
        {
            ModuleConfiguration selectedConfiguration = PlaylistGrid.SelectedItem as ModuleConfiguration;

            if (selectedConfiguration != null)
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
            ModuleConfiguration selectedConfiguration = PlaylistGrid.SelectedItem as ModuleConfiguration;

            if (selectedConfiguration != null)
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
            ModuleConfiguration selectedConfiguration = PlaylistGrid.SelectedItem as ModuleConfiguration;

            if (selectedConfiguration != null)
                _playlist.ModuleConfigurations.Remove(selectedConfiguration);
        }
    }
}
