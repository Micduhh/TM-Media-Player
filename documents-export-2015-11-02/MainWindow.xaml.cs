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

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Boolean playing;
        Boolean muted;
        Boolean repeating;
        static double volumevalue;

        public MainWindow()
        {
            playing = false;
            muted = false;
            repeating = false;
            volumevalue = 10.0;
            InitializeComponent();
            TotalTime.Content = "3:00";
            TimeSlider.Maximum = 180;
            TimeSlider.IsMoveToPointEnabled = true;
            VolumeSlider.Value = (int)(volumevalue);
            PauseButton.Opacity = 0;
            VolumeButtonOff.Visibility = Visibility.Hidden;
        }

        private void New(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void PlayPause(object sender, RoutedEventArgs e)
        {
            if (!playing)
            {
                //PlayButton.Visibility = Visibility.Hidden;
                //PauseButton.Visibility = Visibility.Visible;
                PlayButton.Opacity = 0;
                PauseButton.Opacity = 100;
                PPlay.Header = "_Pause";
                MessageBox.Show("Playing...");
            }
            else
            {
                //PauseButton.Visibility = Visibility.Hidden;
                //PlayButton.Visibility = Visibility.Visible;
                PauseButton.Opacity = 0;
                PlayButton.Opacity = 100;
                PPlay.Header = "_Play";
                MessageBox.Show("Paused...");
            }
            playing = !playing;
        }

        private void FastForward(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Fast Foward...");
        }

        private void Rewind(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Rewind...");
        }

        private void NewCMD(object sender, ExecutedRoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void CloseCMD(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void PlayCMD(object sender, ExecutedRoutedEventArgs e)
        {
            if (!playing)
            {
                PlayButton.Opacity = 0;
                PauseButton.Opacity = 100;
                PPlay.Header = "_Pause";
                MessageBox.Show("Playing...");
            }
            else
            {
                PauseButton.Opacity = 0;
                PlayButton.Opacity = 100;
                PPlay.Header = "_Play";
                MessageBox.Show("Paused...");
            }
            playing = !playing;
        }

        private void FFCMD(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Fast Foward...");
        }

        private void RWCMD(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Rewind...");
        }

        private void Volume(object sender, RoutedEventArgs e)
        {
            if(muted)
            {
                VolumeButtonOff.Visibility = Visibility.Hidden;
                VolumeButtonOn.Visibility = Visibility.Visible;
                VolumeSlider.Value = volumevalue;
            }
            else
            {
                VolumeButtonOn.Visibility = Visibility.Hidden;
                VolumeButtonOff.Visibility = Visibility.Visible;
                volumevalue = VolumeSlider.Value;
                VolumeSlider.Value = 0;
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if((int)VolumeSlider.Value == 0)
            {
                VolumeButtonOn.Visibility = Visibility.Hidden;
                VolumeButtonOff.Visibility = Visibility.Visible;
                muted = true;
            }
            else
            {
                VolumeButtonOff.Visibility = Visibility.Hidden;
                VolumeButtonOn.Visibility = Visibility.Visible;
                muted = false;
            }
        }

        private void Previous(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Previous...");
        }

        private void Skip(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Skip...");
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Stopped...");
        }

        private void Repeat(object sender, RoutedEventArgs e)
        {
            if(repeating)
                MessageBox.Show("Not repeating...");
            else
                MessageBox.Show("Repeating...");
            repeating = !repeating;
        }

        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeSlider.IsMoveToPointEnabled = false;
            int minutes = (int)(TimeSlider.Value/60);
            int seconds = (int)(TimeSlider.Value);
            while(seconds >= 0)
            {
                seconds = seconds - 60;
            }
            seconds = 60 + seconds;
            String secs = seconds.ToString();
            if (secs.Length < 2)
                secs = "0" + secs;
            CurrentTime.Content = minutes + ":" + secs;
            TimeSlider.IsMoveToPointEnabled = true;
        }
    }
}
