﻿using System;
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

namespace TMMediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

    # region Global Variables
        Boolean playing = false;
        Boolean muted = false;
        Boolean repeating = false;
        Boolean activated = false;
        Boolean FFactivated = false;
        Boolean RWactivated = false;
        Boolean flashing = false;
        Boolean changedbyme = false;
        Boolean video = false;
        Boolean color = false;
        Boolean fullScreen = false;
        char flashbutton = '0';
        static double volumevalue = 10.0;
        String name = null;
        private MediaPlayer mediaPlayer = new MediaPlayer();
        static System.Collections.Specialized.StringCollection log = new System.Collections.Specialized.StringCollection();
        List<FileInfo> audiofiles = new List<FileInfo>();
        List<FileInfo> tempaudiofiles = new List<FileInfo>();
        DispatcherTimer flash = new DispatcherTimer();
        DispatcherTimer sliderUpdate = new DispatcherTimer();
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer rewind = new DispatcherTimer();
    # endregion

    # region Main
        public MainWindow()
        {
            InitializeComponent();

            TotalTime.Content = "0:00";
            TimeSlider.Maximum = 180;
            TimeSlider.IsMoveToPointEnabled = true;
            VolumeSlider.IsMoveToPointEnabled = true;
            VolumeSlider.Value = (int)(volumevalue);
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
            Player.LoadedBehavior = MediaState.Manual;
            Player.UnloadedBehavior = MediaState.Manual;
            FileSearch();
            for (int i = 0; i < audiofiles.Count; i++)
            {
                Listbox1.Items.Add(RemoveTrackNumbers(RemoveExtension(audiofiles[i].Name)));
                tempaudiofiles.Add(audiofiles[i]);
            }
            Listbox1.SelectedIndex = 0;
            SearchBox.Focus();
            VolumeSlider.Value = 5;
            SongName.Visibility = Visibility.Hidden;
            //MessageBox.Show("" + Listbox1.Items.Count);
            //Open("Media\\22.mp4");
        }
    # endregion

    # region Base Functions
        private void About()
        {
            MessageBox.Show("Shortcuts:\r\n\r\n    Ctrl+N  -  New Window\r\n    Ctrl+O  -  Open File\r\n    Alt+F4  -  Close Window\r\n    Ctrl+P  -  Toggle Play/Pause\r\n    Ctrl+S  -  Stop Song\r\n    Ctrl+F  -  Fast Forward\r\n    Ctrl+R  -  Rewind\r\n    Ctrl+Right  -  Skip\r\n    Ctrl+Left  -  Previous\r\n    F10  -  Toggle Mute\r\n    Ctrl+Q  -  Toggle Repeat\r\n    Ctrl+H  -  Display Help\r\n    Alt+Enter  -  Toggle Full Screen\r\n    Ctrl+W  -  Toggle Video Display\r\n    Ctrl+C  -  Change Display Color");
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
                Listbox1.Foreground = new SolidColorBrush(Colors.DeepSkyBlue);
                SearchBox.Foreground = new SolidColorBrush(Colors.DeepSkyBlue);
                PlayButton.Content = FindResource("PlayBlue");
                PauseButton.Content = FindResource("PauseBlue");
                FastForwardButton.Content = FindResource("FFBlue");
                RewindButton.Content = FindResource("RewindBlue");
                SkipButton.Content = FindResource("SkipBlue");
                PreviousButton.Content = FindResource("PreviousBlue");
                StopButton.Content = FindResource("StopBlue");
                RepeatButton.Content = FindResource("RepeatBlue");
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
                Listbox1.Foreground = new SolidColorBrush(Colors.OrangeRed);
                SearchBox.Foreground = new SolidColorBrush(Colors.OrangeRed);
                PlayButton.Content = FindResource("PlaySource");
                PauseButton.Content = FindResource("PauseSource");
                FastForwardButton.Content = FindResource("FFSource");
                RewindButton.Content = FindResource("RewindSource");
                SkipButton.Content = FindResource("SkipSource");
                PreviousButton.Content = FindResource("PreviousSource");
                StopButton.Content = FindResource("StopSource");
                RepeatButton.Content = FindResource("RepeatSource");
                VolumeButtonOn.Content = FindResource("VolumeOnSource");
                VolumeButtonOff.Content = FindResource("VolumeOffSource");
                color = false;
            }
            return color;
        }
        private Boolean FastForward()
        {
            if (activated)
            {
                if (!FFactivated)
                {
                    if (RWactivated)
                        Rewind();
                    FastForwardButton.Content = FindResource("FFGray");
                    if (!video)
                        mediaPlayer.SpeedRatio = 2;
                    else
                        Player.SpeedRatio = 2;
                    FFactivated = true;
                }
                else
                {
                    FastForwardButton.Content = FindResource("FFSource");
                    if (!video)
                        mediaPlayer.SpeedRatio = 1;
                    else
                        Player.SpeedRatio = 1;
                    FFactivated = false;
                }
                return true;
            }
            return false;
        }
        private void FileSearch()
        {
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
                path = path.Substring(0, path.IndexOf("bin")-1);
                System.IO.DirectoryInfo rootDir = new DirectoryInfo(path);
                WalkDirectoryTree(rootDir);
                rootDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Music");
                WalkDirectoryTree(rootDir);
                rootDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Videos");
                WalkDirectoryTree(rootDir);
            }

            // Write out all the files that could not be processed.
            //Console.WriteLine("Files with restricted access:");
            //foreach (string s in log)
            //{
            //    Console.WriteLine(s);
            //}
            // Keep the console window open in debug mode.
            //Console.WriteLine("Press any key");
            //Console.ReadKey();
        }
        private Boolean Flash()
        {
            if (flashing)
            {
                if (flashbutton == 'p')
                    PlayButton.Content = FindResource("PlayGray");
                else if (flashbutton == 'x')
                    PauseButton.Content = FindResource("PauseGray");
                else if (flashbutton == 'r')
                    RewindButton.Content = FindResource("RewindGray");
                else if (flashbutton == 'f')
                    FastForwardButton.Content = FindResource("FFGray");
                else if (flashbutton == 'b')
                    PreviousButton.Content = FindResource("PreviousGray");
                else if (flashbutton == 'n')
                    SkipButton.Content = FindResource("SkipGray");
                else if (flashbutton == 's')
                    StopButton.Content = FindResource("StopGray");
                flashing = false;
                return true;
            }
            else
            {
                if (flashbutton == 'p')
                    PlayButton.Content = FindResource("PlaySource");
                else if (flashbutton == 'x')
                    PauseButton.Content = FindResource("PauseSource");
                else if (flashbutton == 'r')
                    RewindButton.Content = FindResource("RewindSource");
                else if (flashbutton == 'f')
                    FastForwardButton.Content = FindResource("FFSource");
                else if (flashbutton == 'b')
                    PreviousButton.Content = FindResource("PreviousSource");
                else if (flashbutton == 'n')
                    SkipButton.Content = FindResource("SkipSource");
                else if (flashbutton == 's')
                    StopButton.Content = FindResource("StopSource");
                flash.IsEnabled = false;
                return false;
            }
        }
        private void FullScreen()
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
                if(Player.Visibility.Equals(Visibility.Visible))
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
            VolumeButtonOn.Visibility = Visibility.Hidden;
            VolumeButtonOff.Visibility = Visibility.Hidden;
            VolumeSlider.Visibility = Visibility.Hidden;
            TimeSlider.Visibility = Visibility.Hidden;
            CurrentTime.Visibility = Visibility.Hidden;
            TotalTime.Visibility = Visibility.Hidden;
            SongName.Visibility = Visibility.Hidden;
        }
        private void Listbox1DoubleClick(MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(Listbox1, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                Open(tempaudiofiles[Listbox1.SelectedIndex].FullName);
            }
        }
        private void Listbox1KeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var item = ItemsControl.ContainerFromElement(Listbox1, e.OriginalSource as DependencyObject) as ListBoxItem;
                if (item != null)
                {
                    Open(tempaudiofiles[Listbox1.SelectedIndex].FullName);
                }
            }
            else if (e.Key == Key.Left)
            {
                if (Listbox1.SelectedIndex == 0)
                {
                    e.Handled = true;
                    Keyboard.Focus(SearchBox);
                    SearchBox.SelectionStart = 0;
                    SearchBox.SelectAll();
                }
                else
                {
                    Listbox1.SelectedIndex = Listbox1.SelectedIndex - 1;
                    Listbox1.ScrollIntoView(Listbox1.SelectedItem);
                }
            }
            else if (e.Key == Key.Right)
            {
                if (Listbox1.SelectedIndex == Listbox1.Items.Count - 1)
                {
                    Listbox1.SelectedIndex = 0;
                    Listbox1.ScrollIntoView(Listbox1.SelectedItem);
                }
                else
                {
                    Listbox1.SelectedIndex = Listbox1.SelectedIndex + 1;
                    Listbox1.ScrollIntoView(Listbox1.SelectedItem);
                }
            }
            else if (e.Key == Key.Down || e.Key == Key.Up)
            {
                e.Handled = true;
            }
        }
        private void MainMouseMove()
        {
            PlayButton.Visibility = Visibility.Visible;
            PauseButton.Visibility = Visibility.Visible;
            FastForwardButton.Visibility = Visibility.Visible;
            RewindButton.Visibility = Visibility.Visible;
            SkipButton.Visibility = Visibility.Visible;
            PreviousButton.Visibility = Visibility.Visible;
            StopButton.Visibility = Visibility.Visible;
            RepeatButton.Visibility = Visibility.Visible;
            VolumeButtonOn.Visibility = Visibility.Visible;
            VolumeButtonOff.Visibility = Visibility.Visible;
            VolumeSlider.Visibility = Visibility.Visible;
            TimeSlider.Visibility = Visibility.Visible;
            CurrentTime.Visibility = Visibility.Visible;
            TotalTime.Visibility = Visibility.Visible;
            if (!fullScreen)
            {
                timer.Stop();
            }
            else
            {
                if(Player.Visibility.Equals(Visibility.Visible))
                    SongName.Visibility = Visibility.Visible;
                if (timer != null)
                {
                    timer.Tick -= Timer_Tick;
                }
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Tick += new EventHandler(Timer_Tick);
                timer.Start();
            }
        }
        private void New()
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
        private Boolean Open()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media files (*.mp3, *.mp4, ...)|*.mp3;*.wav;*.m4a;*.au;*.aiff;*.aif;*.wma;*.mp4;*.mov;*.avi;*.amv;*.wmv|Audio files (*.mp3, *.wav, ...)|*.mp3;*.wav;*.m4a;*.au;*.aiff;*.aif;*.wma|Video files (*.mp4, *.mov, ...)|*.mp4;*.mov;*.avi;*.amv;*.wmv|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                if (playing)
                    Player.Stop();
                if (RWactivated)
                    Rewind();
                if (FFactivated)
                    FastForward();
                String str = openFileDialog.FileName;
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
                if (!video)
                {
                    Player.Close();
                    Player.Visibility = Visibility.Hidden;
                    MediaBacking.Visibility = Visibility.Hidden;
                    MShowVideoWindow.Header = "Show _Video";
                    mediaPlayer.Open(opened);
                }
                else
                {
                    mediaPlayer.Close();
                    Player.Visibility = Visibility.Visible;
                    MediaBacking.Visibility = Visibility.Visible;
                    MShowVideoWindow.Header = "Hide _Video";
                    Player.Source = opened;
                    Player.Play();
                    Player.Pause();
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
                for (int j = 0; j < tempaudiofiles.Count; j++)
                    if (tempaudiofiles[j].FullName.Contains(str))
                        str = tempaudiofiles[j].Name;
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
        private Boolean Open(String file)
        {
            if (!file.Equals(""))
            {/*
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
                if (playing)
                    Player.Stop();
                if (RWactivated)
                    Rewind();
                if (FFactivated)
                    FastForward();
                String str = file;
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
                for (int j = 0; j < tempaudiofiles.Count; j++)
                    if (tempaudiofiles[j].FullName.Contains(str))
                        str = tempaudiofiles[j].Name;
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
                flashbutton = 'b';
                flash.IsEnabled = true;
            }
            if(!video)
            {
                if (mediaPlayer.Position > new TimeSpan(0, 0, 1))
                {
                    mediaPlayer.Position = new TimeSpan(0, 0, 0);
                }
                else
                {
                    int i = 0;
                    for (; i < tempaudiofiles.Count; i++)
                        if (tempaudiofiles[i].FullName.Contains(name))
                            break;
                    mediaPlayer.Stop();
                    if (i == 0)
                    {
                        Open(tempaudiofiles[tempaudiofiles.Count - 1].FullName);
                        Listbox1.SelectedItem = Listbox1.Items[tempaudiofiles.Count - 1];
                        Listbox1.ScrollIntoView(Listbox1.Items[tempaudiofiles.Count - 1]);
                    }
                    else
                    {
                        Open(tempaudiofiles[i - 1].FullName);
                        Listbox1.SelectedItem = Listbox1.Items[i - 1];
                        Listbox1.ScrollIntoView(Listbox1.Items[i - 1]);
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
                    int i = 0;
                    for (; i < tempaudiofiles.Count; i++)
                        if (tempaudiofiles[i].FullName.Contains(name))
                            break;
                    Player.Stop();
                    if (i == 0)
                    {
                        Open(tempaudiofiles[tempaudiofiles.Count - 1].FullName);
                        Listbox1.SelectedItem = Listbox1.Items[tempaudiofiles.Count - 1];
                        Listbox1.ScrollIntoView(Listbox1.Items[tempaudiofiles.Count - 1]);
                    }
                    else
                    {
                        Open(tempaudiofiles[i - 1].FullName);
                        Listbox1.SelectedItem = Listbox1.Items[i - 1];
                        Listbox1.ScrollIntoView(Listbox1.Items[i - 1]);
                    }
                    Player.Position = new TimeSpan(0, 0, 0);
                }
            }
            return true;
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
        private String RemoveTrackNumbers(String file)
        {
            for(int i = 0; i < file.Length; i++)
            {
                if(file[i] >= 'A')
                {
                    file = file.Substring(i);
                    break;
                }
                if(file[i] == ' ' || file[i] == '_')
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
                RepeatButton.Content = FindResource("RepeatSource");
            repeating = !repeating;
            return repeating;
        }
        private Boolean Rewind()
        {
            if (activated)
            {
                if (!RWactivated)
                {
                    if (FFactivated)
                        FastForward();
                    RewindButton.Content = FindResource("RewindGray");
                    rewind.Start();
                    RWactivated = true;
                }
                else
                {
                    RewindButton.Content = FindResource("RewindSource");
                    rewind.Stop();
                    RWactivated = false;
                }
                return true;
            }
            return false;
        }
        private void SearchBoxChanged()
        {
            if (!SearchBox.Text.Equals(""))
            {
                Listbox1.Items.Clear();
                tempaudiofiles.Clear();
                int i = 0;
                for (; i < audiofiles.Count; i++)
                {//audiofiles[i].Name.IndexOf(SearchBox.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1
                    if (RemoveTrackNumbers(audiofiles[i].Name).IndexOf(SearchBox.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        String file = audiofiles[i].Name;
                        file = RemoveTrackNumbers(RemoveExtension(file));
                        Listbox1.Items.Add(file);
                        tempaudiofiles.Add(audiofiles[i]);
                    }
                }
            }
            else
            {
                Listbox1.Items.Clear();
                tempaudiofiles.Clear();
                for (int i = 0; i < audiofiles.Count; i++)
                {
                    String file = audiofiles[i].Name;
                    file = RemoveTrackNumbers(RemoveExtension(file));
                    Listbox1.Items.Add(file);
                    tempaudiofiles.Add(audiofiles[i]);
                }
            }
            if (Listbox1.Items.Count >= 1)
                Listbox1.SelectedItem = Listbox1.Items[0];
        }
        private void SearchBoxKeyDown(KeyEventArgs e)
        {
            if ((e.Key == Key.Right || e.Key == Key.Enter || e.Key == Key.Tab) && SearchBox.SelectionStart == SearchBox.Text.Length && Listbox1.Items.Count >= 1)
            {
                ListBoxItem lbi2 = (ListBoxItem)(Listbox1.ItemContainerGenerator.ContainerFromItem(Listbox1.SelectedItem));
                lbi2.Focus();
            }
        }
        private Boolean SetTime()
        {
            if (!changedbyme)
            {
                TimeSlider.IsMoveToPointEnabled = false;
                if(!video)
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
                if (!video)
                    mediaPlayer.Volume = VolumeSlider.Value / 10;
                else
                    Player.Volume = VolumeSlider.Value / 10;
                muted = false;
            }
            else
            {
                VolumeButtonOn.Visibility = Visibility.Hidden;
                VolumeButtonOff.Visibility = Visibility.Visible;
                if (!video)
                    mediaPlayer.Volume = VolumeSlider.Value / 10;
                else
                    Player.Volume = VolumeSlider.Value / 10;
                muted = true;
            }
            return muted;
        }
        private Boolean ShowVideo()
        {
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
        private Boolean Skip()
        {
            if (activated)
            {
                flashing = true;
                flashbutton = 'n';
                flash.IsEnabled = true;
            }
            if(!video)
            {
                int i = 0;
                for (; i < tempaudiofiles.Count; i++)
                    if (tempaudiofiles[i].FullName.Contains(name))
                        break;
                mediaPlayer.Stop();
                if (tempaudiofiles.Count < i + 2)
                {
                    Open(tempaudiofiles[0].FullName);
                    Listbox1.SelectedItem = Listbox1.Items[0];
                    Listbox1.ScrollIntoView(Listbox1.Items[0]);
                }
                else
                {
                    Open(tempaudiofiles[i + 1].FullName);
                    Listbox1.SelectedItem = Listbox1.Items[i + 1];
                    Listbox1.ScrollIntoView(Listbox1.Items[i + 1]);
                }
                mediaPlayer.Position = new TimeSpan(0, 0, 0);
            }
            else
            {
                int i = 0;
                for (; i < tempaudiofiles.Count; i++)
                    if (tempaudiofiles[i].FullName.Contains(name))
                        break;
                Player.Stop();
                if (tempaudiofiles.Count < i + 2)
                {
                    Open(tempaudiofiles[0].FullName);
                    Listbox1.SelectedItem = Listbox1.Items[0];
                    Listbox1.ScrollIntoView(Listbox1.Items[0]);
                }
                else
                {
                    Open(tempaudiofiles[i + 1].FullName);
                    Listbox1.SelectedItem = Listbox1.Items[i + 1];
                    Listbox1.ScrollIntoView(Listbox1.Items[i + 1]);
                }
                Player.Position = new TimeSpan(0, 0, 0);
            }
            return true;
        }
        private Boolean Stop()
        {
            if (activated)
            {
                flashing = true;
                flashbutton = 's';
                flash.IsEnabled = true;
                PauseButton.Opacity = 0;
                PlayButton.Opacity = 100;
                MPlay.Header = "_Play";
                playing = false;
                if(!video)
                    mediaPlayer.Stop();
                else
                    Player.Stop();
                return true;
            }
            return false;
        }
        private void UpdateTime()
        {
            if (!video)
            {
                if (repeating)
                {
                    if (mediaPlayer.Position == mediaPlayer.NaturalDuration.TimeSpan)
                        mediaPlayer.Position = new TimeSpan(0, 0, 0);
                }
                else
                {
                    try
                    {
                        if (mediaPlayer.Position == mediaPlayer.NaturalDuration.TimeSpan)
                        {
                            int i = 0;
                            for (; i < tempaudiofiles.Count; i++)
                                if (tempaudiofiles[i].FullName.Contains(name))
                                    break;
                            mediaPlayer.Stop();
                            if (tempaudiofiles.Count < i + 2)
                            {
                                Open(tempaudiofiles[0].FullName);
                                Listbox1.SelectedItem = Listbox1.Items[0];
                                Listbox1.ScrollIntoView(Listbox1.Items[0]);
                            }
                            else
                            {
                                Open(tempaudiofiles[i + 1].FullName);
                                Listbox1.SelectedItem = Listbox1.Items[i + 1];
                                Listbox1.ScrollIntoView(Listbox1.Items[i + 1]);
                            }
                            mediaPlayer.Position = new TimeSpan(0, 0, 0);
                        }
                    }
                    catch (Exception e) { UpdateTime(); }
                }
                if (mediaPlayer.Position.Seconds < 10)
                    CurrentTime.Content = mediaPlayer.Position.Minutes + ":0" + mediaPlayer.Position.Seconds;
                else
                    CurrentTime.Content = mediaPlayer.Position.Minutes + ":" + mediaPlayer.Position.Seconds;
                changedbyme = true;
                TimeSlider.Value = mediaPlayer.Position.Minutes * 60 * 1000 + mediaPlayer.Position.Seconds * 1000 + mediaPlayer.Position.Milliseconds;
                changedbyme = false;
            }
            else
            {
                if (repeating)
                {
                    if (Player.Position == Player.NaturalDuration.TimeSpan)
                        Player.Position = new TimeSpan(0, 0, 0);
                }
                else
                {
                    if (Player.Position == Player.NaturalDuration.TimeSpan)
                    {
                        int i = 0;
                        for (; i < tempaudiofiles.Count; i++)
                            if (tempaudiofiles[i].FullName.Contains(name))
                                break;
                        Player.Stop();
                        if (tempaudiofiles.Count < i + 2)
                        {
                            Open(tempaudiofiles[0].FullName);
                            Listbox1.SelectedItem = Listbox1.Items[0];
                            Listbox1.ScrollIntoView(Listbox1.Items[0]);
                        }
                        else
                        {
                            Open(tempaudiofiles[i + 1].FullName);
                            Listbox1.SelectedItem = Listbox1.Items[i + 1];
                            Listbox1.ScrollIntoView(Listbox1.Items[i + 1]);
                        }
                        Player.Position = new TimeSpan(0, 0, 0);
                    }
                }
                if (Player.Position.Seconds < 10)
                    CurrentTime.Content = Player.Position.Minutes + ":0" + Player.Position.Seconds;
                else
                    CurrentTime.Content = Player.Position.Minutes + ":" + Player.Position.Seconds;
                changedbyme = true;
                TimeSlider.Value = Player.Position.Minutes * 60 * 1000 + Player.Position.Seconds * 1000 + Player.Position.Milliseconds;
                changedbyme = false;
            }
        }
        private Boolean VolumeOnOff()
        {
            if (muted)
            {
                VolumeButtonOff.Visibility = Visibility.Hidden;
                VolumeButtonOn.Visibility = Visibility.Visible;
                VolumeSlider.Value = volumevalue;
                return true;
            }
            else
            {
                VolumeButtonOn.Visibility = Visibility.Hidden;
                VolumeButtonOff.Visibility = Visibility.Visible;
                volumevalue = VolumeSlider.Value;
                VolumeSlider.Value = 0;
                return false;
            }
        }
        private void WalkDirectoryTree(System.IO.DirectoryInfo root)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse.
                // You may decide to do something different here. For example, you
                // can try to elevate your privileges and access the file again.
                log.Add(e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files) //search for files within the directories
                {
                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().
                    // *.mp3;*.wav;*.m4a;*.au;*.aiff;*.aif;*.wma;*.mp4;*.mov;*.avi;*.amv;*.wmv
                    if (fi.FullName.Contains(".mp3") || fi.FullName.Contains(".wav") || fi.FullName.Contains(".m4a") || fi.FullName.Contains(".au") || fi.FullName.Contains(".aiff") || fi.FullName.Contains(".aif") || fi.FullName.Contains(".wma") || fi.FullName.Contains(".mp4") || fi.FullName.Contains(".mov") || fi.FullName.Contains(".avi") || fi.FullName.Contains(".amv") || fi.FullName.Contains(".wmv"))
                    {
                        String file = fi.Name;
                        file = RemoveExtension(file);
                        file = RemoveTrackNumbers(file);
                        //Listbox1.Items.Add(file); //<--Uncomment this to write individual files in directories to Console
                        if (audiofiles.Count == 0)
                            audiofiles.Add(fi);
                        else
                        {
                            int i = 0;
                            for (; i < audiofiles.Count; i++)
                            {
                                if (file.CompareTo(RemoveTrackNumbers(RemoveExtension(audiofiles[i].Name))) < 0)
                                {
                                    audiofiles.Insert(i, fi);
                                    break;
                                }
                                //else if (file.CompareTo(audiofiles[i].Name) == 0)
                                //    break;
                            }
                            if(i >= audiofiles.Count - 1)
                                audiofiles.Add(fi);
                        }
                    }

                    /*
                    String s = fi.FullName;
                    */
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories(); //<------HERE!!!!!

                /**/
                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    //Listbox1.Items.Add(dirInfo.Name);
                    WalkDirectoryTree(dirInfo); //<--Uncomment this to recurse through the next level of subdirectories
                }
                /**/
            }
        }
        private void WindowSizeChanged()
        {
            fullScreen = false;
            Dock.Height = Main.ActualHeight - 20;
            Dock.Width = Main.ActualWidth - 14;
            Canvas1.Height = Dock.Height - 20;
            Canvas1.Width = Dock.Width;
            Listbox1.Height = Canvas1.Height - 245;
            Listbox1.Width = Canvas1.Width - 30;
            Player.Height = Canvas1.Height - 200;
            Player.Width = Canvas1.Width - 30;
            MediaBacking.Height = Canvas1.Height - 200;
            MediaBacking.Width = Canvas1.Width - 30;
            TimeSlider.Width = Canvas1.Width - 190;
            Canvas.SetTop(Player, 15);
            Canvas.SetLeft(Player, 15);
            Canvas.SetTop(MediaBacking, 15);
            Canvas.SetLeft(MediaBacking, 15);
            Canvas.SetTop(TimeSlider, Listbox1.Height + 75);
            Canvas.SetTop(CurrentTime, Listbox1.Height + 64);
            Canvas.SetRight(CurrentTime, Canvas1.Width - 80);
            Canvas.SetTop(TotalTime, Listbox1.Height + 64);
            Canvas.SetLeft(TotalTime, Canvas1.Width - 80);
            Canvas.SetTop(PlayButton, Listbox1.Height + 108);
            Canvas.SetLeft(PlayButton, (Canvas1.Width / 2) - 48);
            Canvas.SetTop(PauseButton, Listbox1.Height + 108);
            Canvas.SetLeft(PauseButton, (Canvas1.Width / 2) - 48);
            Canvas.SetTop(FastForwardButton, Listbox1.Height + 118);
            Canvas.SetRight(FastForwardButton, (Canvas1.Width / 2) - 158);
            Canvas.SetTop(RewindButton, Listbox1.Height + 118);
            Canvas.SetLeft(RewindButton, (Canvas1.Width / 2) - 158);
            Canvas.SetTop(SkipButton, Listbox1.Height + 133);
            Canvas.SetRight(SkipButton, (Canvas1.Width / 2) - 263);
            Canvas.SetTop(PreviousButton, Listbox1.Height + 133);
            Canvas.SetLeft(PreviousButton, (Canvas1.Width / 2) - 263);
            Canvas.SetTop(StopButton, Listbox1.Height + 138);
            Canvas.SetTop(RepeatButton, Listbox1.Height + 138);
            Canvas.SetTop(VolumeButtonOn, Listbox1.Height + 143);
            Canvas.SetTop(VolumeButtonOff, Listbox1.Height + 143);
            Canvas.SetTop(VolumeSlider, Listbox1.Height + 148);
            fullScreen = false;
        }
    # endregion

    # region Button Events
        private void About(object sender, RoutedEventArgs e)
        {
            About();
        }
        private void Close(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Color(object sender, RoutedEventArgs e)
        {
            Color();
        }
        private void FastForward(object sender, RoutedEventArgs e)
        {
            FastForward();
        }
        private void FullScreen(object sender, RoutedEventArgs e)
        {
            FullScreen();
        }
        private void Listbox1DoubleClick(object sender, MouseButtonEventArgs e)
        {
            Listbox1DoubleClick(e);
        }
        private void Listbox1KeyDown(object sender, KeyEventArgs e)
        {
            Listbox1KeyDown(e);
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
            Open();
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
            Open();
        }
        private void PlayCMD(object sender, ExecutedRoutedEventArgs e)
        {
            PlayPause();
        }
        private void PreviousCMD(object sender, ExecutedRoutedEventArgs e)
        {
            Previous();
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
