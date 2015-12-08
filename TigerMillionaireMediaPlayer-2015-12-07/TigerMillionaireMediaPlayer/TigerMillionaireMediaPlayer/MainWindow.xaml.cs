using System;
using System.IO;
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
using Microsoft.Win32;
using System.Windows.Threading;
using System.Data.SQLite;

namespace TMMediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        # region Global Variables
        Boolean playing, muted, repeating, shuffle, activated, fastForwardActivated, rewindActivated, flashing, changedByMe, video, color, fullScreen, cdFound;
        char flashButton = '0';
        String name = null, title = null, artist = null, album = null, year = null;
        static double volumeValue = 10.0;
        private System.Windows.Media.MediaPlayer mediaPlayer = new System.Windows.Media.MediaPlayer();
        static System.Collections.Specialized.StringCollection log = new System.Collections.Specialized.StringCollection();
        private SQLiteConnection sqliteConn;
        private SQLiteCommand sqliteCmd;
        List<FileInfo> audioFiles = new List<FileInfo>();
        List<FileInfo> cdfiles = new List<FileInfo>();
        List<FileInfo> tempAudioFiles = new List<FileInfo>();
        List<List<FileInfo>> playlists = new List<List<FileInfo>>();
        DispatcherTimer flash = new DispatcherTimer();
        DispatcherTimer sliderUpdate = new DispatcherTimer();
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer rewind = new DispatcherTimer();
        DispatcherTimer cdCheck = new DispatcherTimer();
        # endregion

        # region Main
        public MainWindow()
        {
            InitializeComponent();
            TotalTime.Content = "0:00";
            TimeSlider.Maximum = 180;
            TimeSlider.IsMoveToPointEnabled = true;
            VolumeSlider.IsMoveToPointEnabled = true;
            VolumeSlider.Value = (int)(volumeValue);
            PauseButton.Opacity = 0;
            VolumeButtonOff.Visibility = Visibility.Hidden;
            PlayButton.Content = FindResource("PlayGray");
            PauseButton.Content = FindResource("PauseGray");
            RewindButton.Content = FindResource("RewindGray");
            FastForwardButton.Content = FindResource("FFGray");
            PreviousButton.Content = FindResource("PreviousGray");
            SkipButton.Content = FindResource("SkipGray");
            StopButton.Content = FindResource("StopGray");
            flash.Tick += new EventHandler(Flash_Tick);
            flash.Interval = new TimeSpan(0, 0, 0, 0, 150);
            flash.IsEnabled = false;
            sliderUpdate.Tick += new EventHandler(SliderUpdate_Tick);
            sliderUpdate.Interval = new TimeSpan(0, 0, 0, 0, 25);
            sliderUpdate.IsEnabled = false;
            rewind.Tick += new EventHandler(Rewind_Tick);
            rewind.Interval = new TimeSpan(0, 0, 0, 0, 25);
            rewind.IsEnabled = false;
            cdCheck.Tick += new EventHandler(CheckCD_Tick);
            cdCheck.Interval = new TimeSpan(0, 0, 5);
            cdCheck.IsEnabled = true;
            Player.LoadedBehavior = MediaState.Manual;
            Player.UnloadedBehavior = MediaState.Manual;
            FileSearch();
            ReadState();
            LeftBox.SelectedIndex = 0;
            SearchBox.Focus();
            VolumeSlider.Value = 5;
            SongName.Visibility = Visibility.Hidden;
            ComboBox1.Items.Add("Create playlist");
            int i = 0;
            for (; i < playlists.Count - 1; i++)
                ComboBox1.Items.Add("Playlist" + (i + 1));
            //if(playlists.Count > 0)
            //  if (playlists[i - 1].Count == 0)
            //    ComboBox1.Items.RemoveAt(i);
            this.Loaded += new RoutedEventHandler(MainLoaded);
        }
        # endregion

        # region Base Functions
        private void About()
        {
            MessageBox.Show("Tiger Millionaire Media Player\r\n\r\nCreated By:\r\n     Adam Scott\r\n     Micah Netz\r\n     Shawn Todd\r\n\r\nShortcuts:\r\n\r\n    Ctrl+N  -  New Window\r\n    Ctrl+O  -  Open File\r\n    Alt+F4  -  Close Window\r\n    Ctrl+P  -  Toggle Play/Pause\r\n    Ctrl+S  -  Stop Song\r\n    Ctrl+F  -  Fast Forward\r\n    Ctrl+R  -  Rewind\r\n    Ctrl+Right  -  Skip\r\n    Ctrl+Left  -  Previous\r\n    F10  -  Toggle Mute\r\n    F11  -  Volume Down\r\n    F12  -  Volume Up\r\n    Ctrl+Q  -  Toggle Repeat\r\n    Ctrl+H  -  Display Help\r\n    Alt+Enter  -  Toggle Full Screen\r\n    Ctrl+W  -  Toggle Video Display\r\n    Ctrl+C  -  Change Display Color\r\n\r\nVersion 1.1", "About TMMP");
        }
        private Boolean AddToPlaylist()
        {
            if (LeftBox.SelectedIndex >= 0)
            {
                if (playlists.Count == 0)
                {
                    playlists.Add(new List<FileInfo>());
                    ComboBox1.Items.Add("Playlist" + ComboBox1.Items.Count);
                }
                FileInfo selectedSong = new FileInfo(tempAudioFiles[LeftBox.SelectedIndex].FullName);
                if (ComboBox1.SelectedIndex > 0)
                    playlists[ComboBox1.SelectedIndex - 1].Add(selectedSong);
                else
                {
                    ComboBox1.SelectedIndex = ComboBox1.Items.Count - 1;
                    playlists[ComboBox1.SelectedIndex - 1].Add(selectedSong);
                }
                RightBox.Items.Add(RemoveTrackNumbers(RemoveExtension(selectedSong.Name)));
                return true;
            }
            return false;
        }
        private Boolean Color()
        {
            if (!color)
            {
                LinearGradientBrush blue = (LinearGradientBrush)Canvas1.Background;
                GradientStopCollection stops = blue.GradientStops;
                GradientStop red = stops[1];
                red.Color = Colors.DeepSkyBlue;
                stops[1] = red;
                blue.GradientStops = stops;
                Canvas1.Background = blue;
                CurrentTime.Foreground = new SolidColorBrush(Colors.DeepSkyBlue);
                TotalTime.Foreground = new SolidColorBrush(Colors.DeepSkyBlue);
                SongName.Foreground = new SolidColorBrush(Colors.DeepSkyBlue);
                LeftBox.Foreground = new SolidColorBrush(Colors.DeepSkyBlue);
                RightBox.Foreground = new SolidColorBrush(Colors.DeepSkyBlue);
                SearchBox.Foreground = new SolidColorBrush(Colors.DeepSkyBlue);
                PlayButton.Content = FindResource("PlayBlue");
                PauseButton.Content = FindResource("PauseBlue");
                FastForwardButton.Content = FindResource("FFBlue");
                RewindButton.Content = FindResource("RewindBlue");
                SkipButton.Content = FindResource("SkipBlue");
                PreviousButton.Content = FindResource("PreviousBlue");
                StopButton.Content = FindResource("StopBlue");
                if (repeating)
                    RepeatButton.Content = FindResource("RepeatBlue");
                if (shuffle)
                    ShuffleButton.Content = FindResource("ShuffleBlue");
                VolumeButtonOn.Content = FindResource("VolumeOnBlue");
                VolumeButtonOff.Content = FindResource("VolumeOffBlue");
                color = true;
            }
            else
            {
                LinearGradientBrush red = (LinearGradientBrush)Canvas1.Background;
                GradientStopCollection stops = red.GradientStops;
                GradientStop blue = stops[1];
                blue.Color = Colors.OrangeRed;
                stops[1] = blue;
                red.GradientStops = stops;
                Canvas1.Background = red;
                CurrentTime.Foreground = new SolidColorBrush(Colors.OrangeRed);
                TotalTime.Foreground = new SolidColorBrush(Colors.OrangeRed);
                SongName.Foreground = new SolidColorBrush(Colors.OrangeRed);
                LeftBox.Foreground = new SolidColorBrush(Colors.OrangeRed);
                RightBox.Foreground = new SolidColorBrush(Colors.OrangeRed);
                SearchBox.Foreground = new SolidColorBrush(Colors.OrangeRed);
                PlayButton.Content = FindResource("PlaySource");
                PauseButton.Content = FindResource("PauseSource");
                FastForwardButton.Content = FindResource("FFSource");
                RewindButton.Content = FindResource("RewindSource");
                SkipButton.Content = FindResource("SkipSource");
                PreviousButton.Content = FindResource("PreviousSource");
                StopButton.Content = FindResource("StopSource");
                if (repeating)
                    RepeatButton.Content = FindResource("RepeatSource");
                if (shuffle)
                    ShuffleButton.Content = FindResource("ShuffleSource");
                VolumeButtonOn.Content = FindResource("VolumeOnSource");
                VolumeButtonOff.Content = FindResource("VolumeOffSource");
                color = false;
            }
            return color;
        }
        private Boolean ComboBox1SelectionChanged()
        {
            if (ComboBox1.SelectedIndex == 0)
            {
                playlists.Add(new List<FileInfo>());
                ComboBox1.Items.Add("Playlist" + ComboBox1.Items.Count);
                return false;
            }
            else
            {
                RightBox.Items.Clear();
                for (int i = 0; i < playlists[ComboBox1.SelectedIndex - 1].Count; i++)
                    RightBox.Items.Add(RemoveTrackNumbers(RemoveExtension(playlists[ComboBox1.SelectedIndex - 1][i].Name)));
                return true;
            }
        }
        private void CreateDatabase()
        {
            try
            {
                File.Delete("database.db");
                SQLiteConnection.CreateFile("database.db");
                sqliteConn = new SQLiteConnection("Data Source=" + Environment.CurrentDirectory + "\\database.db;Version=3;");
                sqliteConn.Open();

                sqliteCmd = sqliteConn.CreateCommand();

                sqliteCmd.CommandText = "CREATE TABLE IF NOT EXISTS songs (primkey int, filename varchar(200), title varchar(200), artist varchar(200), album varchar(200), year varchar(10));";
                sqliteCmd.ExecuteNonQuery();
            }
            catch (Exception e) { Console.WriteLine(e); }
            //sqliteConn.Close();
        }
        private Boolean FastForward()
        {
            if (activated)
            {
                if (!fastForwardActivated)
                {
                    if (rewindActivated)
                        Rewind();
                    FastForwardButton.Content = FindResource("FFGray");
                    if (!video)
                        mediaPlayer.SpeedRatio = 2;
                    else
                        Player.SpeedRatio = 2;
                    fastForwardActivated = true;
                }
                else
                {
                    if (!color)
                        FastForwardButton.Content = FindResource("FFSource");
                    else
                        FastForwardButton.Content = FindResource("FFBlue");
                    if (!video)
                        mediaPlayer.SpeedRatio = 1;
                    else
                        Player.SpeedRatio = 1;
                    fastForwardActivated = false;
                }
                return true;
            }
            return false;
        }
        private void FileSearch()
        {
            CreateDatabase();

            LeftBox.Items.Clear();
            tempAudioFiles.Clear();
            audioFiles.Clear();

            string[] drives = { "C:\\" };

            foreach (string dr in drives)
            {
                System.IO.DriveInfo di = new System.IO.DriveInfo(dr);

                // Here we skip the drive if it is not ready to be read. This
                // is not necessarily the appropriate action in all scenarios.
                if (!di.IsReady)
                {
                    Console.WriteLine("The drive {0} could not be read", di.Name);
                    continue;
                }
                String path = Environment.CurrentDirectory;
                path = path.Substring(0, path.IndexOf("bin") - 1);
                System.IO.DirectoryInfo rootDir = new DirectoryInfo(path);
                WalkDirectoryTree(rootDir, drives[0]);
                rootDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Music");
                WalkDirectoryTree(rootDir, drives[0]);
                rootDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Videos");
                WalkDirectoryTree(rootDir, drives[0]);
            }
            for (int i = 0; i < audioFiles.Count; i++)
            {
                LeftBox.Items.Add(RemoveTrackNumbers(RemoveExtension(audioFiles[i].Name)));
                tempAudioFiles.Add(audioFiles[i]);
            }
            LeftBox.SelectedIndex = 0;
            if (cdfiles.Count >= 0)
                cdFound = false;
            FileSearchCDDrive(); // May need to be removed
        }
        private Boolean FileSearchCDDrive()
        {
            cdfiles.Clear();

            string[] drives = { "D:\\" };

            foreach (string dr in drives)
            {
                System.IO.DriveInfo di = new System.IO.DriveInfo(dr);

                // Here we skip the drive if it is not ready to be read. This
                // is not necessarily the appropriate action in all scenarios.
                if (!di.IsReady)
                {
                    Console.WriteLine("The drive {0} could not be read", di.Name);
                    continue;
                }
                System.IO.DirectoryInfo rootDir = new DirectoryInfo(drives[0]);
                WalkDirectoryTree(rootDir, drives[0]);
            }
            if (!cdFound)
            {
                for (int i = 0; i < cdfiles.Count; i++)
                {
                    LeftBox.Items.Add("CD: " + RemoveTrackNumbers(RemoveExtension(cdfiles[i].Name)));
                    tempAudioFiles.Add(cdfiles[i]);
                }
                LeftBox.SelectedIndex = 0;
                cdFound = true;
            }
            if (cdfiles.Count == 0)
            {
                for (int i = 0; i < LeftBox.Items.Count; i++)
                {
                    String item = (String)LeftBox.Items[i];
                    if (item[0] == 'C' && item[1] == 'D' && item[2] == ':')
                        LeftBox.Items.Remove(item);
                }
                cdFound = false;
            }
            else
                cdFound = true;
            return cdFound;
        }
        private Boolean Flash()
        {
            if (flashing)
            {
                if (flashButton == 'b')
                    PreviousButton.Content = FindResource("PreviousGray");
                else if (flashButton == 'n')
                    SkipButton.Content = FindResource("SkipGray");
                else if (flashButton == 's')
                    StopButton.Content = FindResource("StopGray");
                flashing = false;
                return true;
            }
            else
            {
                if (flashButton == 'b')
                {
                    if (!color)
                        PreviousButton.Content = FindResource("PreviousSource");
                    else
                        PreviousButton.Content = FindResource("PreviousBlue");
                }
                else if (flashButton == 'n')
                {
                    if (!color)
                        SkipButton.Content = FindResource("SkipSource");
                    else
                        SkipButton.Content = FindResource("SkipBlue");
                }
                else if (flashButton == 's')
                {
                    if (!color)
                        StopButton.Content = FindResource("StopSource");
                    else
                        StopButton.Content = FindResource("StopBlue");
                }
                flash.IsEnabled = false;
                return false;
            }
        }
        private Boolean FullScreen()
        {
            if (!fullScreen)
            {
                Player.Height = Canvas1.Height - 15;
                Player.Width = Canvas1.Width;
                Canvas.SetTop(Player, 0);
                Canvas.SetLeft(Player, 0);
                MediaBacking.Height = Canvas1.Height - 15;
                MediaBacking.Width = Canvas1.Width;
                Canvas.SetTop(MediaBacking, 0);
                Canvas.SetLeft(MediaBacking, 0);
                if (Player.Visibility.Equals(Visibility.Visible))
                    SongName.Visibility = Visibility.Visible;
                Player.Focus();
            }
            else
            {
                Player.Height = Canvas1.Height - 200;
                Player.Width = Canvas1.Width - 30;
                Canvas.SetTop(Player, 15);
                Canvas.SetLeft(Player, 15);
                MediaBacking.Height = Canvas1.Height - 200;
                MediaBacking.Width = Canvas1.Width - 30;
                Canvas.SetTop(MediaBacking, 15);
                Canvas.SetLeft(MediaBacking, 15);
                if (Player.Visibility.Equals(Visibility.Visible))
                    SongName.Visibility = Visibility.Visible;
            }
            fullScreen = !fullScreen;
            return fullScreen;
        }
        private void HideControls()
        {
            PlayButton.Visibility = Visibility.Hidden;
            PauseButton.Visibility = Visibility.Hidden;
            FastForwardButton.Visibility = Visibility.Hidden;
            RewindButton.Visibility = Visibility.Hidden;
            SkipButton.Visibility = Visibility.Hidden;
            PreviousButton.Visibility = Visibility.Hidden;
            StopButton.Visibility = Visibility.Hidden;
            RepeatButton.Visibility = Visibility.Hidden;
            ShuffleButton.Visibility = Visibility.Hidden;
            VolumeButtonOn.Visibility = Visibility.Hidden;
            VolumeButtonOff.Visibility = Visibility.Hidden;
            VolumeSlider.Visibility = Visibility.Hidden;
            TimeSlider.Visibility = Visibility.Hidden;
            CurrentTime.Visibility = Visibility.Hidden;
            TotalTime.Visibility = Visibility.Hidden;
            SongName.Visibility = Visibility.Hidden;
        }
        private Boolean LeftBoxDoubleClick(ListBox Listbox, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(Listbox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                if (Listbox.Name.Equals("LeftBox"))
                    Open(tempAudioFiles[Listbox.SelectedIndex].FullName);
                else
                    Open(playlists[ComboBox1.SelectedIndex - 1][RightBox.SelectedIndex].FullName);
                return true;
            }
            return false;
        }
        private void LeftBoxKeyDown(ListBox Listbox, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var item = ItemsControl.ContainerFromElement(Listbox, e.OriginalSource as DependencyObject) as ListBoxItem;
                if (item != null)
                {
                    if (Listbox.Name.Equals("LeftBox"))
                        Open(tempAudioFiles[Listbox.SelectedIndex].FullName);
                    else
                        Open(playlists[ComboBox1.SelectedIndex - 1][RightBox.SelectedIndex].FullName);
                }
            }
            else if (e.Key == Key.Left)
            {
                if (Listbox.SelectedIndex == 0)
                {
                    if (Listbox.Name.Equals("LeftBox"))
                    {
                        e.Handled = true;
                        Keyboard.Focus(SearchBox);
                        SearchBox.SelectionStart = 0;
                        SearchBox.SelectAll();
                    }
                    else
                    {
                        Listbox.SelectedIndex = Listbox.Items.Count - 1;
                        Listbox.ScrollIntoView(Listbox.SelectedItem);
                    }
                }
                else
                {
                    Listbox.SelectedIndex = Listbox.SelectedIndex - 1;
                    Listbox.ScrollIntoView(Listbox.SelectedItem);
                }
            }
            else if (e.Key == Key.Right)
            {
                if (Listbox.SelectedIndex == Listbox.Items.Count - 1)
                {
                    Listbox.SelectedIndex = 0;
                    Listbox.ScrollIntoView(Listbox.SelectedItem);
                }
                else
                {
                    Listbox.SelectedIndex = Listbox.SelectedIndex + 1;
                    Listbox.ScrollIntoView(Listbox.SelectedItem);
                }
            }
            else if (e.Key == Key.Down || e.Key == Key.Up)
            {
                e.Handled = true;
            }
        }
        private Boolean MainLoaded()
        {
            if (Application.Current.Properties["InputFile"] != null)
            {
                string fname = Application.Current.Properties["InputFile"].ToString();
                Open(fname);
                LeftBox.SelectedIndex = 0;
                Repeat();
                return true;
            }
            return false;
        }
        private Boolean MainMouseMove()
        {
            PlayButton.Visibility = Visibility.Visible;
            PauseButton.Visibility = Visibility.Visible;
            FastForwardButton.Visibility = Visibility.Visible;
            RewindButton.Visibility = Visibility.Visible;
            SkipButton.Visibility = Visibility.Visible;
            PreviousButton.Visibility = Visibility.Visible;
            StopButton.Visibility = Visibility.Visible;
            RepeatButton.Visibility = Visibility.Visible;
            ShuffleButton.Visibility = Visibility.Visible;
            VolumeButtonOn.Visibility = Visibility.Visible;
            VolumeButtonOff.Visibility = Visibility.Visible;
            VolumeSlider.Visibility = Visibility.Visible;
            TimeSlider.Visibility = Visibility.Visible;
            CurrentTime.Visibility = Visibility.Visible;
            TotalTime.Visibility = Visibility.Visible;
            if (!fullScreen)
            {
                timer.Stop();
                return false;
            }
            else
            {
                if (Player.Visibility.Equals(Visibility.Visible))
                    SongName.Visibility = Visibility.Visible;
                if (timer != null)
                {
                    timer.Tick -= Timer_Tick;
                }
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Tick += new EventHandler(Timer_Tick);
                timer.Start();
                return true;
            }
        }
        private void New()
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
        private Boolean Open(String file)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            bool? open = false;
            if (file.Equals(""))
            {
                openFileDialog.Filter = "Media files (*.mp3, *.mp4, ...)|*.mp3;*.wav;*.m4a;*.au;*.aiff;*.aif;*.wma;*.mp4;*.mov;*.avi;*.amv;*.wmv|Audio files (*.mp3, *.wav, ...)|*.mp3;*.wav;*.m4a;*.au;*.aiff;*.aif;*.wma|Video files (*.mp4, *.mov, ...)|*.mp4;*.mov;*.avi;*.amv;*.wmv|All files (*.*)|*.*";
                open = openFileDialog.ShowDialog();
            }
            if ((bool)open || !file.Equals(""))
            {
                if (playing)
                    Player.Stop();
                if (rewindActivated)
                    Rewind();
                if (fastForwardActivated)
                    FastForward();
                String str = null;
                if (!file.Equals(""))
                    str = file;
                else
                    str = openFileDialog.FileName;
                name = str;
                String ext = str;
                while (ext.Substring(ext.IndexOf(".") + 1).Contains("."))
                {
                    ext = ext.Substring(ext.IndexOf(".") + 1);
                }
                ext = ext.Substring(ext.IndexOf("."));
                Uri opened = new Uri(str);
                if (ext.Equals(".mp3") || ext.Equals(".wav") || ext.Equals(".m4a") || ext.Equals(".au") || ext.Equals(".aiff") || ext.Equals(".aif") || ext.Equals(".wma"))
                    video = false;
                else
                    video = true;
                try
                {
                    if (!video)
                    {
                        Player.Close();
                        SongName.Visibility = Visibility.Hidden;
                        Player.Visibility = Visibility.Hidden;
                        MediaBacking.Visibility = Visibility.Hidden;
                        MShowVideoWindow.Header = "Show _Video";
                        mediaPlayer.Open(opened);
                    }
                    else
                    {
                        mediaPlayer.Close();
                        SongName.Visibility = Visibility.Visible;
                        Player.Visibility = Visibility.Visible;
                        MediaBacking.Visibility = Visibility.Visible;
                        MShowVideoWindow.Header = "Hide _Video";
                        Player.Source = opened;
                        Player.Play();
                        Player.Pause();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
                if (!color)
                {
                    PlayButton.Content = FindResource("PlaySource");
                    PauseButton.Content = FindResource("PauseSource");
                    RewindButton.Content = FindResource("RewindSource");
                    FastForwardButton.Content = FindResource("FFSource");
                    PreviousButton.Content = FindResource("PreviousSource");
                    SkipButton.Content = FindResource("SkipSource");
                    StopButton.Content = FindResource("StopSource");
                }
                else
                {
                    PlayButton.Content = FindResource("PlayBlue");
                    PauseButton.Content = FindResource("PauseBlue");
                    RewindButton.Content = FindResource("RewindBlue");
                    FastForwardButton.Content = FindResource("FFBlue");
                    PreviousButton.Content = FindResource("PreviousBlue");
                    SkipButton.Content = FindResource("SkipBlue");
                    StopButton.Content = FindResource("StopBlue");
                }
                if (!video)
                {
                    while (!mediaPlayer.NaturalDuration.HasTimeSpan) ;
                    if (mediaPlayer.NaturalDuration.TimeSpan.Seconds < 10)
                        TotalTime.Content = mediaPlayer.NaturalDuration.TimeSpan.Minutes + ":0" + mediaPlayer.NaturalDuration.TimeSpan.Seconds;
                    else
                        TotalTime.Content = mediaPlayer.NaturalDuration.TimeSpan.Minutes + ":" + mediaPlayer.NaturalDuration.TimeSpan.Seconds;
                    TimeSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.Minutes * 60 * 1000 + mediaPlayer.NaturalDuration.TimeSpan.Seconds * 1000 + mediaPlayer.NaturalDuration.TimeSpan.Milliseconds;
                }
                else
                {
                    while (!Player.NaturalDuration.HasTimeSpan) ;
                    if (Player.NaturalDuration.TimeSpan.Seconds < 10)
                        TotalTime.Content = Player.NaturalDuration.TimeSpan.Minutes + ":0" + Player.NaturalDuration.TimeSpan.Seconds;
                    else
                        TotalTime.Content = Player.NaturalDuration.TimeSpan.Minutes + ":" + Player.NaturalDuration.TimeSpan.Seconds;
                    TimeSlider.Maximum = Player.NaturalDuration.TimeSpan.Minutes * 60 * 1000 + Player.NaturalDuration.TimeSpan.Seconds * 1000 + Player.NaturalDuration.TimeSpan.Milliseconds;
                }
                sliderUpdate.IsEnabled = true;
                activated = true;
                for (int j = 0; j < tempAudioFiles.Count; j++)
                    if (tempAudioFiles[j].FullName.Contains(str))
                        str = tempAudioFiles[j].Name;
                str = RemoveTrackNumbers(RemoveExtension(str));
                //if(str.Length > 22)
                //    str = str.Substring(0, 22);
                SongName.Content = str;
                SongNameBlack.Content = str;
                Player.Stop();
                if (playing)
                    PlayPause();
                PlayPause();
                return true;
            }
            return false;
        }
        private Boolean PlayPause()
        {
            if (activated)
            {
                if (!playing)
                {
                    PlayButton.Opacity = 0;
                    PauseButton.Opacity = 100;
                    MPlay.Header = "_Pause";
                    if (!video)
                        mediaPlayer.Play();
                    else
                    {
                        Player.Play();
                    }
                }
                else
                {
                    PauseButton.Opacity = 0;
                    PlayButton.Opacity = 100;
                    MPlay.Header = "_Play";
                    if (!video)
                        mediaPlayer.Pause();
                    else
                    {
                        Player.Pause();
                    }
                }
                playing = !playing;
            }
            return playing;
        }
        private Boolean Previous()
        {
            if (activated)
            {
                flashing = true;
                flashButton = 'b';
                flash.IsEnabled = true;

                if (!video)
                {
                    if (mediaPlayer.Position > new TimeSpan(0, 0, 1))
                    {
                        mediaPlayer.Position = new TimeSpan(0, 0, 0);
                    }
                    else
                    {
                        if (LeftBox.SelectedIndex >= 0)
                        {
                            int i = 0;
                            for (; i < tempAudioFiles.Count; i++)
                                if (tempAudioFiles[i].FullName.Contains(name))
                                    break;
                            mediaPlayer.Stop();
                            if (i == 0)
                            {
                                Open(tempAudioFiles[tempAudioFiles.Count - 1].FullName);
                                LeftBox.SelectedItem = LeftBox.Items[tempAudioFiles.Count - 1];
                                LeftBox.ScrollIntoView(LeftBox.Items[tempAudioFiles.Count - 1]);
                            }
                            else
                            {
                                Open(tempAudioFiles[i - 1].FullName);
                                LeftBox.SelectedItem = LeftBox.Items[i - 1];
                                LeftBox.ScrollIntoView(LeftBox.Items[i - 1]);
                            }
                        }
                        else if (RightBox.SelectedIndex >= 0)
                        {
                            int i = 0;
                            for (; i < playlists[ComboBox1.SelectedIndex - 1].Count; i++)
                                if (playlists[ComboBox1.SelectedIndex - 1][i].FullName.Contains(name))
                                    break;
                            mediaPlayer.Stop();
                            if (i == 0)
                            {
                                Open(playlists[ComboBox1.SelectedIndex - 1][playlists[ComboBox1.SelectedIndex - 1].Count - 1].FullName);
                                RightBox.SelectedItem = RightBox.Items[playlists[ComboBox1.SelectedIndex - 1].Count - 1];
                                RightBox.ScrollIntoView(RightBox.Items[playlists[ComboBox1.SelectedIndex - 1].Count - 1]);
                            }
                            else
                            {
                                Open(playlists[ComboBox1.SelectedIndex - 1][i - 1].FullName);
                                RightBox.SelectedItem = RightBox.Items[i - 1];
                                RightBox.ScrollIntoView(RightBox.Items[i - 1]);
                            }
                        }
                        mediaPlayer.Position = new TimeSpan(0, 0, 0);
                    }
                }
                else
                {
                    if (Player.Position > new TimeSpan(0, 0, 1))
                    {
                        Player.Position = new TimeSpan(0, 0, 0);
                    }
                    else
                    {
                        if (LeftBox.SelectedIndex >= 0)
                        {
                            int i = 0;
                            for (; i < tempAudioFiles.Count; i++)
                                if (tempAudioFiles[i].FullName.Contains(name))
                                    break;
                            Player.Stop();
                            if (i == 0)
                            {
                                Open(tempAudioFiles[tempAudioFiles.Count - 1].FullName);
                                LeftBox.SelectedItem = LeftBox.Items[tempAudioFiles.Count - 1];
                                LeftBox.ScrollIntoView(LeftBox.Items[tempAudioFiles.Count - 1]);
                            }
                            else
                            {
                                Open(tempAudioFiles[i - 1].FullName);
                                LeftBox.SelectedItem = LeftBox.Items[i - 1];
                                LeftBox.ScrollIntoView(LeftBox.Items[i - 1]);
                            }
                        }
                        else if (RightBox.SelectedIndex >= 0)
                        {
                            int i = 0;
                            for (; i < playlists[ComboBox1.SelectedIndex - 1].Count; i++)
                                if (playlists[ComboBox1.SelectedIndex - 1][i].FullName.Contains(name))
                                    break;
                            Player.Stop();
                            if (i == 0)
                            {
                                Open(playlists[ComboBox1.SelectedIndex - 1][playlists[ComboBox1.SelectedIndex - 1].Count - 1].FullName);
                                RightBox.SelectedItem = RightBox.Items[playlists[ComboBox1.SelectedIndex - 1].Count - 1];
                                RightBox.ScrollIntoView(RightBox.Items[playlists[ComboBox1.SelectedIndex - 1].Count - 1]);
                            }
                            else
                            {
                                Open(playlists[ComboBox1.SelectedIndex - 1][i - 1].FullName);
                                RightBox.SelectedItem = RightBox.Items[i - 1];
                                RightBox.ScrollIntoView(RightBox.Items[i - 1]);
                            }
                        }
                        Player.Position = new TimeSpan(0, 0, 0);
                    }
                }
            }
            return true;
        }
        private Boolean ReadFileInfo(String file)
        {/*
            try
            {
                byte[] b = new byte[128];
                string sTitle;
                string sSinger;
                string sAlbum;
                string sYear;
                string sComm;
                FileStream fs = new FileStream(file, FileMode.Open);
                fs.Seek(-128, SeekOrigin.End);
                fs.Read(b, 0, 128);
                bool isSet = false;
                String sFlag = System.Text.Encoding.Default.GetString(b, 0, 3);
                if (sFlag.CompareTo("TAG") == 0)
                {
                    //System.Console.WriteLine("Tag   is   setted! ");
                    isSet = true;
                }

                if (isSet)
                {
                    //get   title   of   song; 
                    sTitle = System.Text.Encoding.Default.GetString(b, 3, 30);
                    title = sTitle;
                    //get   singer; 
                    sSinger = System.Text.Encoding.Default.GetString(b, 33, 30);
                    artist = sSinger;
                    //get   album; 
                    sAlbum = System.Text.Encoding.Default.GetString(b, 63, 30);
                    album = sAlbum;
                    //get   Year   of   publish; 
                    sYear = System.Text.Encoding.Default.GetString(b, 93, 4);
                    year = sYear;
                    //get   Comment; 
                    sComm = System.Text.Encoding.Default.GetString(b, 97, 30);
                    //MessageBox.Show("Comment: " + sComm);
                }
                return true;
            }
            catch (Exception e) { Console.WriteLine(e); return false; }*/

            /*
                byte[] b = new byte[128];
                string sTitle = "";
                //string sArtist = "";
                FileStream fs = new FileStream(file, FileMode.Open);
                int i = 0;
                for (; i < 10000000; i += 8)
                {
                    fs.Seek(i, SeekOrigin.Begin);
                    fs.Read(b, 0, 128);
                    sTitle = System.Text.Encoding.Default.GetString(b, 0, 25).ToString();
                    if (sTitle.Contains("©nam"))
                        break;
                }
                fs.Seek(i + 37, SeekOrigin.Begin);
                fs.Read(b, 0, 128);
                sTitle = System.Text.Encoding.Default.GetString(b, 0, 19).ToString();
                */

            return true;
        }
        private void ReadState()
        {
            try
            {
                StreamReader reader = new StreamReader(Environment.CurrentDirectory + "\\savedata\\playlists.sav");
                if (reader.ReadLine() == null)
                    return;
                reader.Close();
                reader = new StreamReader(Environment.CurrentDirectory + "\\savedata\\playlists.sav");
                String temp = null;
                String files = "";
                int i = 0;
                for (; !(temp = reader.ReadLine()).Equals(""); i++)
                {
                    if (temp == null)
                        break;
                    playlists.Add(new List<FileInfo>());
                    files += temp;
                    StreamReader playlistFile = new StreamReader(Environment.CurrentDirectory + "\\savedata\\playlist" + (i + 1) + ".sav");
                    String temp2 = null;
                    for (temp2 = playlistFile.ReadLine(); temp2 != null && !temp2.Equals("\r\n") && !temp2.Equals(""); temp2 = playlistFile.ReadLine())
                    {
                        if (!temp2.Equals(" ") && !temp2.Equals("\r\n") && temp2 != null)
                            playlists[i].Add(new FileInfo(temp2));
                    }
                    //playlistFile.WriteLine(mediaFiles);
                    playlistFile.Close();
                }
                //writer.WriteLine(files);
                reader.Close();
                //if(ComboBox1.Items.Count > 1)
                //  ComboBox1.SelectedIndex = 1;
            }
            catch (Exception e) { Console.WriteLine("" + e); return; }
        }
        private String RemoveExtension(String file)
        {
            if (file.Contains(".mp3"))
                file = file.Substring(0, file.IndexOf(".mp3"));
            else if (file.Contains(".wav"))
                file = file.Substring(0, file.IndexOf(".wav"));
            else if (file.Contains(".m4a"))
                file = file.Substring(0, file.IndexOf(".m4a"));
            else if (file.Contains(".au"))
                file = file.Substring(0, file.IndexOf(".au"));
            else if (file.Contains(".aiff"))
                file = file.Substring(0, file.IndexOf(".aiff"));
            else if (file.Contains(".aif"))
                file = file.Substring(0, file.IndexOf(".aif"));
            else if (file.Contains(".wma"))
                file = file.Substring(0, file.IndexOf(".wma"));
            else if (file.Contains(".mp4"))
                file = file.Substring(0, file.IndexOf(".mp4"));
            else if (file.Contains(".mov"))
                file = file.Substring(0, file.IndexOf(".mov"));
            else if (file.Contains(".avi"))
                file = file.Substring(0, file.IndexOf(".avi"));
            else if (file.Contains(".amv"))
                file = file.Substring(0, file.IndexOf(".amv"));
            else if (file.Contains(".wmv"))
                file = file.Substring(0, file.IndexOf(".wmv"));
            return file;
        }
        private Boolean RemoveFromPlaylist()
        {
            if (RightBox.SelectedIndex >= 0)
            {
                int curr = RightBox.SelectedIndex;
                FileInfo selectedSong = new FileInfo(playlists[ComboBox1.SelectedIndex - 1][RightBox.SelectedIndex].FullName);
                playlists[ComboBox1.SelectedIndex - 1].RemoveAt(RightBox.SelectedIndex);
                RightBox.Items.Remove(RemoveTrackNumbers(RemoveExtension(selectedSong.Name)));
                RightBox.SelectedIndex = curr;
                if (RightBox.SelectedIndex < 0)
                {
                    curr--;
                    RightBox.SelectedIndex = curr;
                    if (RightBox.SelectedIndex < 0)
                    {
                        LeftBox.SelectedIndex = 0;
                    }
                }
                return true;
            }
            return false;
        }
        private String RemoveTrackNumbers(String file)
        {
            for (int i = 0; i < file.Length; i++)
            {
                if (file[i] >= 'A')
                {
                    file = file.Substring(i);
                    break;
                }
                if (file[i] == ' ' || file[i] == '_')
                {
                    file = file.Substring(i + 1);
                    break;
                }
            }
            return file;
        }
        private Boolean Repeat()
        {
            if (repeating)
                RepeatButton.Content = FindResource("RepeatGray");
            else
            {
                if (!color)
                    RepeatButton.Content = FindResource("RepeatSource");
                else
                    RepeatButton.Content = FindResource("RepeatBlue");
            }
            repeating = !repeating;
            return repeating;
        }
        private Boolean Rewind()
        {
            if (activated)
            {
                if (!rewindActivated)
                {
                    if (fastForwardActivated)
                        FastForward();
                    RewindButton.Content = FindResource("RewindGray");
                    rewind.Start();
                    rewindActivated = true;
                }
                else
                {
                    if (!color)
                        RewindButton.Content = FindResource("RewindSource");
                    else
                        RewindButton.Content = FindResource("RewindBlue");
                    rewind.Stop();
                    rewindActivated = false;
                }
                return true;
            }
            return false;
        }
        private void SaveState()
        {
            try
            {
                for (int i = 0; i < 100; i++)
                    File.Delete(Environment.CurrentDirectory + "\\savedata\\playlist" + (i + 1) + ".sav");
                File.Delete(Environment.CurrentDirectory + "\\savedata\\playlists.sav");
                File.Create(Environment.CurrentDirectory + "\\savedata\\playlists.sav").Close();
                StreamWriter writer = new StreamWriter(Environment.CurrentDirectory + "\\savedata\\playlists.sav");
                String files = "";
                for (int i = 0; i < playlists.Count; i++)
                {
                    files += Environment.CurrentDirectory + "\\savedata\\playlist" + (i + 1) + ".sav\r\n";
                    File.Create(Environment.CurrentDirectory + "\\savedata\\playlist" + (i + 1) + ".sav").Close();
                    StreamWriter playlistFile = new StreamWriter(Environment.CurrentDirectory + "\\savedata\\playlist" + (i + 1) + ".sav");
                    String mediaFiles = "";
                    for (int j = 0; j < playlists[i].Count; j++)
                    {
                        mediaFiles += playlists[i][j].FullName + "\r\n";
                    }
                    playlistFile.WriteLine(mediaFiles);
                    playlistFile.Close();
                }
                writer.WriteLine(files);
                writer.Close();
            }
            catch (Exception e) { Console.WriteLine("" + e); }
        }
        private Boolean SearchBoxChanged()
        {
            if (!SearchBox.Text.Equals(""))
            {
                LeftBox.Items.Clear();
                tempAudioFiles.Clear();
                int i = 0;
                for (; i < audioFiles.Count; i++)
                {//audioFiles[i].Name.IndexOf(SearchBox.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1
                    if (RemoveTrackNumbers(audioFiles[i].Name).IndexOf(SearchBox.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        String file = audioFiles[i].Name;
                        file = RemoveTrackNumbers(RemoveExtension(file));
                        LeftBox.Items.Add(file);
                        tempAudioFiles.Add(audioFiles[i]);
                    }
                }
            }
            else
            {
                LeftBox.Items.Clear();
                tempAudioFiles.Clear();
                for (int i = 0; i < audioFiles.Count; i++)
                {
                    String file = audioFiles[i].Name;
                    file = RemoveTrackNumbers(RemoveExtension(file));
                    LeftBox.Items.Add(file);
                    tempAudioFiles.Add(audioFiles[i]);
                }
                cdFound = false;
            }
            if (LeftBox.Items.Count >= 1)
            {
                LeftBox.SelectedItem = LeftBox.Items[0];
                return true;
            }
            else
                return false;
        }
        private Boolean SearchBoxKeyDown(KeyEventArgs e)
        {
            if ((e.Key == Key.Right || e.Key == Key.Enter || e.Key == Key.Tab) && SearchBox.SelectionStart == SearchBox.Text.Length && LeftBox.Items.Count >= 1)
            {
                ListBoxItem lbi2 = (ListBoxItem)(LeftBox.ItemContainerGenerator.ContainerFromItem(LeftBox.SelectedItem));
                lbi2.Focus();
                return true;
            }
            return false;
        }
        private Boolean SetTime()
        {
            if (!changedByMe)
            {
                TimeSlider.IsMoveToPointEnabled = false;
                if (!video)
                    mediaPlayer.Position = new TimeSpan(0, 0, 0, 0, (int)TimeSlider.Value);
                else
                    Player.Position = new TimeSpan(0, 0, 0, 0, (int)TimeSlider.Value);
                TimeSlider.IsMoveToPointEnabled = true;
                return true;
            }
            return false;
        }
        private Boolean SetVolume()
        {
            if ((int)VolumeSlider.Value >= 0.0000000001)
            {
                VolumeButtonOff.Visibility = Visibility.Hidden;
                VolumeButtonOn.Visibility = Visibility.Visible;
                mediaPlayer.Volume = VolumeSlider.Value / 10;
                Player.Volume = VolumeSlider.Value / 10;
                muted = false;
            }
            else
            {
                VolumeButtonOn.Visibility = Visibility.Hidden;
                VolumeButtonOff.Visibility = Visibility.Visible;
                mediaPlayer.Volume = VolumeSlider.Value / 10;
                Player.Volume = VolumeSlider.Value / 10;
                muted = true;
            }
            return muted;
        }
        private Boolean ShowVideo()
        {
            if (fullScreen)
                FullScreen();
            if (Player.Visibility == Visibility.Visible)
            {
                SongName.Visibility = Visibility.Hidden;
                Player.Visibility = Visibility.Hidden;
                MediaBacking.Visibility = Visibility.Hidden;
                MShowVideoWindow.Header = "Show _Video";
                return false;
            }
            else
            {
                SongName.Visibility = Visibility.Visible;
                Player.Visibility = Visibility.Visible;
                MediaBacking.Visibility = Visibility.Visible;
                MShowVideoWindow.Header = "Hide _Video";
                return true;
            }
        }
        private Boolean Shuffle()
        {
            if (!shuffle)
            {
                if (!color)
                    ShuffleButton.Content = FindResource("ShuffleSource");
                else
                    ShuffleButton.Content = FindResource("ShuffleBlue");
            }
            else
            {
                ShuffleButton.Content = FindResource("ShuffleGray");
            }
            shuffle = !shuffle;
            return shuffle;
        }
        private Boolean Skip()
        {
            if (activated)
            {
                flashing = true;
                flashButton = 'n';
                flash.IsEnabled = true;

                if (!video)
                {
                    if (shuffle)
                    {
                        Random rnd = new Random();
                        if (LeftBox.SelectedIndex >= 0)
                        {
                            int rand = rnd.Next(0, LeftBox.Items.Count);
                            while (rand == LeftBox.SelectedIndex && LeftBox.Items.Count > 1)
                                rand = rnd.Next(0, LeftBox.Items.Count);
                            LeftBox.SelectedIndex = rand;
                            LeftBox.ScrollIntoView(LeftBox.SelectedItem);
                            Open(tempAudioFiles[LeftBox.SelectedIndex].FullName);
                        }
                        else
                        {
                            int rand = rnd.Next(0, RightBox.Items.Count);
                            while (rand == RightBox.SelectedIndex && RightBox.Items.Count > 1)
                                rand = rnd.Next(0, RightBox.Items.Count);
                            RightBox.SelectedIndex = rand;
                            RightBox.ScrollIntoView(RightBox.SelectedItem);
                            Open(playlists[ComboBox1.SelectedIndex - 1][RightBox.SelectedIndex].FullName);
                        }
                        Player.Position = new TimeSpan(0, 0, 0);
                        mediaPlayer.Position = new TimeSpan(0, 0, 0);
                    }
                    else
                    {
                        if (LeftBox.SelectedIndex >= 0)
                        {
                            int i = 0;
                            for (; i < tempAudioFiles.Count; i++)
                                if (tempAudioFiles[i].FullName.Contains(name))
                                    break;
                            mediaPlayer.Stop();
                            if (tempAudioFiles.Count < i + 2)
                            {
                                Open(tempAudioFiles[0].FullName);
                                LeftBox.SelectedItem = LeftBox.Items[0];
                                LeftBox.ScrollIntoView(LeftBox.Items[0]);
                            }
                            else
                            {
                                Open(tempAudioFiles[i + 1].FullName);
                                LeftBox.SelectedItem = LeftBox.Items[i + 1];
                                LeftBox.ScrollIntoView(LeftBox.Items[i + 1]);
                            }
                        }
                        else if (RightBox.SelectedIndex >= 0)
                        {
                            int i = 0;
                            for (; i < playlists[ComboBox1.SelectedIndex - 1].Count; i++)
                                if (playlists[ComboBox1.SelectedIndex - 1][i].FullName.Contains(name))
                                    break;
                            mediaPlayer.Stop();
                            if (playlists[ComboBox1.SelectedIndex - 1].Count < i + 2)
                            {
                                Open(playlists[ComboBox1.SelectedIndex - 1][0].FullName);
                                RightBox.SelectedItem = RightBox.Items[0];
                                RightBox.ScrollIntoView(RightBox.Items[0]);
                            }
                            else
                            {
                                Open(playlists[ComboBox1.SelectedIndex - 1][i + 1].FullName);
                                RightBox.SelectedItem = RightBox.Items[i + 1];
                                RightBox.ScrollIntoView(RightBox.Items[i + 1]);
                            }
                        }
                    }
                    mediaPlayer.Position = new TimeSpan(0, 0, 0);
                }
                else
                {
                    if (shuffle)
                    {
                        Random rnd = new Random();
                        LeftBox.SelectedIndex = rnd.Next(0, LeftBox.Items.Count - 1);
                        LeftBox.ScrollIntoView(LeftBox.SelectedItem);
                        Open(tempAudioFiles[LeftBox.SelectedIndex].FullName);
                    }
                    else
                    {
                        if (LeftBox.SelectedIndex >= 0)
                        {
                            int i = 0;
                            for (; i < tempAudioFiles.Count; i++)
                                if (tempAudioFiles[i].FullName.Contains(name))
                                    break;
                            Player.Stop();
                            if (tempAudioFiles.Count < i + 2)
                            {
                                Open(tempAudioFiles[0].FullName);
                                LeftBox.SelectedItem = LeftBox.Items[0];
                                LeftBox.ScrollIntoView(LeftBox.Items[0]);
                            }
                            else
                            {
                                Open(tempAudioFiles[i + 1].FullName);
                                LeftBox.SelectedItem = LeftBox.Items[i + 1];
                                LeftBox.ScrollIntoView(LeftBox.Items[i + 1]);
                            }
                        }
                        else if (RightBox.SelectedIndex >= 0)
                        {
                            int i = 0;
                            for (; i < playlists[ComboBox1.SelectedIndex - 1].Count; i++)
                                if (playlists[ComboBox1.SelectedIndex - 1][i].FullName.Contains(name))
                                    break;
                            Player.Stop();
                            if (playlists[ComboBox1.SelectedIndex - 1].Count < i + 2)
                            {
                                Open(playlists[ComboBox1.SelectedIndex - 1][0].FullName);
                                RightBox.SelectedItem = RightBox.Items[0];
                                RightBox.ScrollIntoView(RightBox.Items[0]);
                            }
                            else
                            {
                                Open(playlists[ComboBox1.SelectedIndex - 1][i + 1].FullName);
                                RightBox.SelectedItem = RightBox.Items[i + 1];
                                RightBox.ScrollIntoView(RightBox.Items[i + 1]);
                            }
                        }
                    }
                    Player.Position = new TimeSpan(0, 0, 0);
                }
            }
            return true;
        }
        private Boolean Stop()
        {
            if (activated)
            {
                flashing = true;
                flashButton = 's';
                flash.IsEnabled = true;
                PauseButton.Opacity = 0;
                PlayButton.Opacity = 100;
                MPlay.Header = "_Play";
                playing = false;
                if (!video)
                    mediaPlayer.Stop();
                else
                    Player.Stop();
                return true;
            }
            return false;
        }
        private Boolean UpdateTime()
        {
            if (!video)
            {
                if (repeating)
                {
                    if (mediaPlayer.Position >= mediaPlayer.NaturalDuration.TimeSpan)
                        mediaPlayer.Position = new TimeSpan(0, 0, 0);
                }
                else if (shuffle)
                {
                    if (mediaPlayer.Position >= mediaPlayer.NaturalDuration.TimeSpan)
                    {
                        Random rnd = new Random();
                        if (LeftBox.SelectedIndex >= 0)
                        {
                            int rand = rnd.Next(0, LeftBox.Items.Count);
                            while (rand == LeftBox.SelectedIndex && LeftBox.Items.Count > 1)
                                rand = rnd.Next(0, LeftBox.Items.Count);
                            LeftBox.SelectedIndex = rand;
                            LeftBox.ScrollIntoView(LeftBox.SelectedItem);
                            Open(tempAudioFiles[LeftBox.SelectedIndex].FullName);
                        }
                        else
                        {
                            int rand = rnd.Next(0, RightBox.Items.Count);
                            while (rand == RightBox.SelectedIndex && RightBox.Items.Count > 1)
                                rand = rnd.Next(0, RightBox.Items.Count);
                            RightBox.SelectedIndex = rand;
                            RightBox.ScrollIntoView(RightBox.SelectedItem);
                            Open(playlists[ComboBox1.SelectedIndex - 1][RightBox.SelectedIndex].FullName);
                        }
                        Player.Position = new TimeSpan(0, 0, 0);
                        mediaPlayer.Position = new TimeSpan(0, 0, 0);
                    }
                }
                else
                {
                    try
                    {
                        if (mediaPlayer.Position >= mediaPlayer.NaturalDuration.TimeSpan)
                        {
                            if (LeftBox.SelectedIndex >= 0)
                            {
                                int i = 0;
                                for (; i < tempAudioFiles.Count; i++)
                                    if (tempAudioFiles[i].FullName.Contains(name))
                                        break;
                                mediaPlayer.Stop();
                                if (tempAudioFiles.Count < i + 2)
                                {
                                    Open(tempAudioFiles[0].FullName);
                                    LeftBox.SelectedItem = LeftBox.Items[0];
                                    LeftBox.ScrollIntoView(LeftBox.Items[0]);
                                }
                                else
                                {
                                    Open(tempAudioFiles[i + 1].FullName);
                                    LeftBox.SelectedItem = LeftBox.Items[i + 1];
                                    LeftBox.ScrollIntoView(LeftBox.Items[i + 1]);
                                }
                            }
                            else if (RightBox.SelectedIndex >= 0)
                            {
                                int i = 0;
                                for (; i < playlists[ComboBox1.SelectedIndex - 1].Count; i++)
                                    if (playlists[ComboBox1.SelectedIndex - 1][i].FullName.Contains(name))
                                        break;
                                mediaPlayer.Stop();
                                if (playlists[ComboBox1.SelectedIndex - 1].Count < i + 2)
                                {
                                    Open(playlists[ComboBox1.SelectedIndex - 1][0].FullName);
                                    RightBox.SelectedItem = RightBox.Items[0];
                                    RightBox.ScrollIntoView(RightBox.Items[0]);
                                }
                                else
                                {
                                    Open(playlists[ComboBox1.SelectedIndex - 1][i + 1].FullName);
                                    RightBox.SelectedItem = RightBox.Items[i + 1];
                                    RightBox.ScrollIntoView(RightBox.Items[i + 1]);
                                }
                            }
                            mediaPlayer.Position = new TimeSpan(0, 0, 0);
                        }
                    }
                    catch (Exception e) { Console.WriteLine(e); return false; }
                }
                if (mediaPlayer.Position.Seconds < 10)
                    CurrentTime.Content = mediaPlayer.Position.Minutes + ":0" + mediaPlayer.Position.Seconds;
                else
                    CurrentTime.Content = mediaPlayer.Position.Minutes + ":" + mediaPlayer.Position.Seconds;
                changedByMe = true;
                TimeSlider.Value = mediaPlayer.Position.Minutes * 60 * 1000 + mediaPlayer.Position.Seconds * 1000 + mediaPlayer.Position.Milliseconds;
                changedByMe = false;
            }
            else
            {
                if (repeating)
                {
                    if (Player.Position >= Player.NaturalDuration.TimeSpan)
                        Player.Position = new TimeSpan(0, 0, 0);
                }
                else if (shuffle)
                {
                    if (Player.Position >= Player.NaturalDuration.TimeSpan)
                    {
                        Random rnd = new Random();
                        if (LeftBox.SelectedIndex >= 0)
                        {
                            int rand = rnd.Next(0, LeftBox.Items.Count);
                            while (rand == LeftBox.SelectedIndex && LeftBox.Items.Count > 1)
                                rand = rnd.Next(0, LeftBox.Items.Count);
                            LeftBox.SelectedIndex = rand;
                            LeftBox.ScrollIntoView(LeftBox.SelectedItem);
                            Open(tempAudioFiles[LeftBox.SelectedIndex].FullName);
                        }
                        else
                        {
                            int rand = rnd.Next(0, RightBox.Items.Count);
                            while (rand == RightBox.SelectedIndex && RightBox.Items.Count > 1)
                                rand = rnd.Next(0, RightBox.Items.Count);
                            RightBox.SelectedIndex = rand;
                            RightBox.ScrollIntoView(RightBox.SelectedItem);
                            Open(playlists[ComboBox1.SelectedIndex - 1][RightBox.SelectedIndex].FullName);
                        }
                        Player.Position = new TimeSpan(0, 0, 0);
                        mediaPlayer.Position = new TimeSpan(0, 0, 0);
                    }
                }
                else
                {
                    try
                    {
                        if (Player.Position >= Player.NaturalDuration.TimeSpan)
                        {
                            if (LeftBox.SelectedIndex >= 0)
                            {
                                int i = 0;
                                for (; i < tempAudioFiles.Count; i++)
                                    if (tempAudioFiles[i].FullName.Contains(name))
                                        break;
                                Player.Stop();
                                if (tempAudioFiles.Count < i + 2)
                                {
                                    Open(tempAudioFiles[0].FullName);
                                    LeftBox.SelectedItem = LeftBox.Items[0];
                                    LeftBox.ScrollIntoView(LeftBox.Items[0]);
                                }
                                else
                                {
                                    Open(tempAudioFiles[i + 1].FullName);
                                    LeftBox.SelectedItem = LeftBox.Items[i + 1];
                                    LeftBox.ScrollIntoView(LeftBox.Items[i + 1]);
                                }
                            }
                            else if (RightBox.SelectedIndex >= 0)
                            {
                                int i = 0;
                                for (; i < playlists[ComboBox1.SelectedIndex - 1].Count; i++)
                                    if (playlists[ComboBox1.SelectedIndex - 1][i].FullName.Contains(name))
                                        break;
                                Player.Stop();
                                if (playlists[ComboBox1.SelectedIndex - 1].Count < i + 2)
                                {
                                    Open(playlists[ComboBox1.SelectedIndex - 1][0].FullName);
                                    RightBox.SelectedItem = RightBox.Items[0];
                                    RightBox.ScrollIntoView(RightBox.Items[0]);
                                }
                                else
                                {
                                    Open(playlists[ComboBox1.SelectedIndex - 1][i + 1].FullName);
                                    RightBox.SelectedItem = RightBox.Items[i + 1];
                                    RightBox.ScrollIntoView(RightBox.Items[i + 1]);
                                }
                            }
                            Player.Position = new TimeSpan(0, 0, 0);
                        }
                    }
                    catch (Exception e) { Console.WriteLine(e); return false; }
                }
                if (Player.Position.Seconds < 10)
                    CurrentTime.Content = Player.Position.Minutes + ":0" + Player.Position.Seconds;
                else
                    CurrentTime.Content = Player.Position.Minutes + ":" + Player.Position.Seconds;
                changedByMe = true;
                TimeSlider.Value = Player.Position.Minutes * 60 * 1000 + Player.Position.Seconds * 1000 + Player.Position.Milliseconds;
                changedByMe = false;
            }
            return true;
        }
        private Boolean VolumeOnOff()
        {
            if (muted)
            {
                VolumeButtonOff.Visibility = Visibility.Hidden;
                VolumeButtonOn.Visibility = Visibility.Visible;
                VolumeSlider.Value = volumeValue;
                return true;
            }
            else
            {
                VolumeButtonOn.Visibility = Visibility.Hidden;
                VolumeButtonOff.Visibility = Visibility.Visible;
                volumeValue = VolumeSlider.Value;
                VolumeSlider.Value = 0;
                return false;
            }
        }
        private void WalkDirectoryTree(System.IO.DirectoryInfo root, String drive)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            try
            {
                files = root.GetFiles("*.*");
            }
            catch (UnauthorizedAccessException e) { log.Add(e.Message); Console.WriteLine(e.Message); }
            catch (System.IO.DirectoryNotFoundException e) { Console.WriteLine(e.Message); }

            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                    if (fi.FullName.Contains(".mp3") || fi.FullName.Contains(".wav") || fi.FullName.Contains(".m4a") || fi.FullName.Contains(".au") || fi.FullName.Contains(".aiff") || fi.FullName.Contains(".aif") || fi.FullName.Contains(".wma") || fi.FullName.Contains(".mp4") || fi.FullName.Contains(".mov") || fi.FullName.Contains(".avi") || fi.FullName.Contains(".amv") || fi.FullName.Contains(".wmv"))
                    {
                        String file = fi.Name;
                        file = RemoveExtension(file);
                        file = RemoveTrackNumbers(file);
                        if (drive.Equals("D:\\"))
                        {
                            if (cdfiles.Count == 0)
                                cdfiles.Add(fi);
                            else
                            {
                                int i = 0;
                                for (; i < cdfiles.Count; i++)
                                {
                                    if (file.CompareTo(RemoveTrackNumbers(RemoveExtension(cdfiles[i].Name))) < 0)
                                    {
                                        cdfiles.Insert(i, fi);
                                        break;
                                    }
                                    else if (file.CompareTo(cdfiles[i].Name) == 0)
                                        break;
                                }
                                if (i >= cdfiles.Count - 1)
                                    cdfiles.Add(fi);
                            }
                        }//end of if D:\\
                        else//it's the C:\\...
                        {
                            try
                            {
                                ReadFileInfo(file);
                                if (audioFiles.Count == 0)
                                {
                                    audioFiles.Add(fi);
                                    if (fi.Extension.Equals(".mp3"))
                                    {
                                        sqliteCmd.CommandText = "INSERT INTO songs (primkey, filename, title, artist, album, year) VALUES ('" + 0 + "', '" + fi.FullName + "', '" + title + "', '" + artist + "', '" + album + "', '" + year + "');";
                                        sqliteCmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    int i = 0;
                                    for (; i < audioFiles.Count; i++)
                                    {
                                        if (file.CompareTo(RemoveTrackNumbers(RemoveExtension(audioFiles[i].Name))) < 0)
                                        {
                                            audioFiles.Insert(i, fi);
                                            if (fi.Extension.Equals(".mp3"))
                                            {
                                                sqliteCmd.CommandText = "INSERT INTO songs (primkey, filename, title, artist, album, year) VALUES ('" + i + "', '" + fi.FullName + "', '" + title + "', '" + artist + "', '" + album + "', '" + year + "');";
                                                sqliteCmd.ExecuteNonQuery();
                                            }
                                            break;
                                        }
                                        else if (file.CompareTo(audioFiles[i].Name) == 0)
                                            break;
                                    }
                                    if (i >= audioFiles.Count - 1)
                                    {
                                        audioFiles.Add(fi);
                                        if (fi.Extension.Equals(".mp3"))
                                        {
                                            sqliteCmd.CommandText = "INSERT INTO songs (primkey, filename, title, artist, album, year) VALUES ('" + i + "', '" + fi.FullName + "', '" + title + "', '" + artist + "', '" + album + "', '" + year + "');";
                                            sqliteCmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                            catch (Exception e) { Console.WriteLine(e); }
                        }
                    }
                }
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                    WalkDirectoryTree(dirInfo, drive);
            }
        }
        private void WindowSizeChanged()
        {
            fullScreen = false;
            Dock.Height = Main.ActualHeight - 20;
            Dock.Width = Main.ActualWidth - 14;
            Canvas1.Height = Dock.Height - 20;
            Canvas1.Width = Dock.Width;
            LeftBox.Height = Canvas1.Height - 245;
            LeftBox.Width = (int)((Dock.Width - 160) * (3.0 / 5.0));
            RightBox.Height = Canvas1.Height - 245;
            RightBox.Width = (int)((Dock.Width - 160) * (2.0 / 5.0));
            Player.Height = Canvas1.Height - 200;
            Player.Width = Canvas1.Width - 30;
            MediaBacking.Height = Canvas1.Height - 200;
            MediaBacking.Width = Canvas1.Width - 30;
            TimeSlider.Width = Canvas1.Width - 190;
            SongNameBlack.Width = LeftBox.Width - 305;
            Canvas.SetTop(AddButton, 60 + (LeftBox.Height / 2) - 35);
            Canvas.SetTop(RemoveButton, 60 + (LeftBox.Height / 2) + 10);
            Canvas.SetLeft(SearchBox, LeftBox.Width - 185);
            Canvas.SetLeft(Search, Canvas.GetLeft(SearchBox) - 85);
            Canvas.SetLeft(AddButton, LeftBox.Width + 30);
            Canvas.SetLeft(RemoveButton, LeftBox.Width + 30);
            Canvas.SetTop(Player, 15);
            Canvas.SetLeft(Player, 15);
            Canvas.SetTop(MediaBacking, 15);
            Canvas.SetLeft(MediaBacking, 15);
            Canvas.SetTop(TimeSlider, LeftBox.Height + 75);
            Canvas.SetTop(CurrentTime, LeftBox.Height + 64);
            Canvas.SetRight(CurrentTime, Canvas1.Width - 80);
            Canvas.SetTop(TotalTime, LeftBox.Height + 64);
            Canvas.SetLeft(TotalTime, Canvas1.Width - 80);
            Canvas.SetTop(PlayButton, LeftBox.Height + 108);
            Canvas.SetLeft(PlayButton, (Canvas1.Width / 2) - 48);
            Canvas.SetTop(PauseButton, LeftBox.Height + 108);
            Canvas.SetLeft(PauseButton, (Canvas1.Width / 2) - 48);
            Canvas.SetTop(FastForwardButton, LeftBox.Height + 118);
            Canvas.SetRight(FastForwardButton, (Canvas1.Width / 2) - 158);
            Canvas.SetTop(RewindButton, LeftBox.Height + 118);
            Canvas.SetLeft(RewindButton, (Canvas1.Width / 2) - 158);
            Canvas.SetTop(SkipButton, LeftBox.Height + 133);
            Canvas.SetRight(SkipButton, (Canvas1.Width / 2) - 263);
            Canvas.SetTop(PreviousButton, LeftBox.Height + 133);
            Canvas.SetLeft(PreviousButton, (Canvas1.Width / 2) - 263);
            Canvas.SetTop(StopButton, LeftBox.Height + 138);
            Canvas.SetTop(ShuffleButton, LeftBox.Height + 138);
            Canvas.SetTop(RepeatButton, LeftBox.Height + 138);
            Canvas.SetTop(VolumeButtonOn, LeftBox.Height + 143);
            Canvas.SetTop(VolumeButtonOff, LeftBox.Height + 143);
            Canvas.SetTop(VolumeSlider, LeftBox.Height + 148);
            fullScreen = false;
        }
        # endregion

        # region Button Events
        private void About(object sender, RoutedEventArgs e)
        {
            About();
        }
        private void AddToPlaylist(object sender, RoutedEventArgs e)
        {
            AddToPlaylist();
        }
        private void Close(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Color(object sender, RoutedEventArgs e)
        {
            Color();
        }
        private void ComboBox1SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox1SelectionChanged();
        }
        private void FastForward(object sender, RoutedEventArgs e)
        {
            FastForward();
        }
        private void FullScreen(object sender, RoutedEventArgs e)
        {
            FullScreen();
        }
        private void LeftBoxDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LeftBoxDoubleClick((ListBox)sender, e);
        }
        private void RightBoxDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LeftBoxDoubleClick((ListBox)sender, e);
        }
        private void LeftBoxGotFocus(object sender, RoutedEventArgs e)
        {
            RightBox.UnselectAll();
        }
        private void RightBoxGotFocus(object sender, RoutedEventArgs e)
        {
            LeftBox.UnselectAll();
        }
        private void LeftBoxKeyDown(object sender, KeyEventArgs e)
        {
            LeftBoxKeyDown((ListBox)sender, e);
        }
        private void RightBoxKeyDown(object sender, KeyEventArgs e)
        {
            LeftBoxKeyDown((ListBox)sender, e);
        }
        private void LeftBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LeftBox.ScrollIntoView(LeftBox.SelectedItem);
        }
        private void RightBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RightBox.ScrollIntoView(RightBox.SelectedItem);
        }
        void MainLoaded(object sender, RoutedEventArgs e)
        {
            MainLoaded();
        }
        private void MainMouseMove(object sender, MouseEventArgs e)
        {
            MainMouseMove();
        }
        private void New(object sender, RoutedEventArgs e)
        {
            New();
        }
        private void Open(object sender, RoutedEventArgs e)
        {
            Open("");
        }
        private void PlayerClick(object sender, MouseButtonEventArgs e)
        {
            ShowVideo();
        }
        private void PlayPause(object sender, RoutedEventArgs e)
        {
            PlayPause();
        }
        private void Previous(object sender, RoutedEventArgs e)
        {
            Previous();
        }
        private void Refresh(object sender, RoutedEventArgs e)
        {
            FileSearch();
        }
        private void RemoveFromPlaylist(object sender, RoutedEventArgs e)
        {
            RemoveFromPlaylist();
        }
        private void Repeat(object sender, RoutedEventArgs e)
        {
            Repeat();
        }
        private void Rewind(object sender, RoutedEventArgs e)
        {
            Rewind();
        }
        private void SearchBoxChanged(object sender, TextChangedEventArgs e)
        {
            SearchBoxChanged();
        }
        private void SearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            SearchBoxKeyDown(e);
        }
        private void ShowVideo(object sender, RoutedEventArgs e)
        {
            ShowVideo();
        }
        private void Shuffle(object sender, RoutedEventArgs e)
        {
            Shuffle();
        }
        private void Skip(object sender, RoutedEventArgs e)
        {
            Skip();
        }
        private void Stop(object sender, RoutedEventArgs e)
        {
            Stop();
        }
        private void VolumeOnOff(object sender, RoutedEventArgs e)
        {
            VolumeOnOff();
        }
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveState();
        }
        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            WindowSizeChanged();
        }
        # endregion

        # region Command Events
        private void AboutCMD(object sender, ExecutedRoutedEventArgs e)
        {
            About();
        }
        private void CloseCMD(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
        private void ColorCMD(object sender, ExecutedRoutedEventArgs e)
        {
            Color();
        }
        private void FFCMD(object sender, ExecutedRoutedEventArgs e)
        {
            FastForward();
        }
        private void FullScreenCMD(object sender, ExecutedRoutedEventArgs e)
        {
            FullScreen();
        }
        private void NewCMD(object sender, ExecutedRoutedEventArgs e)
        {
            New();
        }
        private void OpenCMD(object sender, ExecutedRoutedEventArgs e)
        {
            Open("");
        }
        private void PlayCMD(object sender, ExecutedRoutedEventArgs e)
        {
            PlayPause();
        }
        private void PreviousCMD(object sender, ExecutedRoutedEventArgs e)
        {
            Previous();
        }
        private void RefreshCMD(object sender, ExecutedRoutedEventArgs e)
        {
            FileSearch();
        }
        private void RepeatCMD(object sender, ExecutedRoutedEventArgs e)
        {
            Repeat();
        }
        private void RWCMD(object sender, ExecutedRoutedEventArgs e)
        {
            Rewind();
        }
        private void ShowVideoCMD(object sender, ExecutedRoutedEventArgs e)
        {
            ShowVideo();
        }
        private void SkipCMD(object sender, ExecutedRoutedEventArgs e)
        {
            Skip();
        }
        private void StopCMD(object sender, ExecutedRoutedEventArgs e)
        {
            Stop();
        }
        private void VolumeDownCMD(object sender, ExecutedRoutedEventArgs e)
        {
            VolumeSlider.Value -= 1;
        }
        private void VolumeOnOffCMD(object sender, ExecutedRoutedEventArgs e)
        {
            VolumeOnOff();
        }
        private void VolumeUpCMD(object sender, ExecutedRoutedEventArgs e)
        {
            VolumeSlider.Value += 1;
        }
        # endregion

        # region Dispatcher Handlers
        private void CheckCD_Tick(object sender, EventArgs e)
        {
            FileSearchCDDrive();
        }
        private void Flash_Tick(object sender, EventArgs e)
        {
            Flash();
        }
        private void SliderUpdate_Tick(object sender, EventArgs e)
        {
            UpdateTime();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            HideControls();
        }
        private void Rewind_Tick(object sender, EventArgs e)
        {
            if (!video)
            {
                if (mediaPlayer.Position < new TimeSpan(0, 0, 0, 0, 50))
                {
                    Rewind();
                    PlayPause();
                }
                else
                    mediaPlayer.Position = mediaPlayer.Position - new TimeSpan(0, 0, 0, 0, 75);
            }
            else
            {
                if (Player.Position < new TimeSpan(0, 0, 0, 0, 50))
                {
                    Rewind();
                    PlayPause();
                }
                else
                    Player.Position = Player.Position - new TimeSpan(0, 0, 0, 0, 75);
            }
        }
        # endregion

        # region Slider Events
        private void SetTime(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetTime();
        }
        private void SetVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetVolume();
        }
        # endregion

    }
}
