﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Timer = System.Timers.Timer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DaFenPlayer.Video;
using DaFenPlayer.Video.Decode;
using System.Windows;
using System.Diagnostics;
using System.Reflection;

namespace DaFenPlayer
{
    internal partial class MainWindowViewModel : ObservableObject, INotifyPropertyChanged
    {
        VideoCore videoCore;
        public EventHandler onVideoDrawFrontend;
        public bool isDrawing = false;

        [ObservableProperty]
        private VideoReceiveArgs receiveArgs = null;

        [ObservableProperty]
        private int streamProtocol = 0;

        [ObservableProperty]
        private string streamUrl = "localhost:554/senpai";

        [ObservableProperty]
        private bool isButtonOpenEnabled = true;

        [ObservableProperty]
        private bool isButtonStopEnabled = true;

        [ObservableProperty]
        private string videoResolution = "";

        [ObservableProperty]
        private string videoFramerate = "";

        [ObservableProperty]
        private string videoFormat = "";

        [ObservableProperty]
        private string logMessage = "";

        Timer infoTimer = new Timer();

        public MainWindowViewModel()
        {
            infoTimer.Interval = 1000;
            infoTimer.Elapsed += updateVideoInfoTimer;
        }

        private void updateVideoInfoTimer(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (ReceiveArgs != null)
            {
                VideoResolution = ReceiveArgs.width + "x" + ReceiveArgs.height;
                VideoFramerate  = ReceiveArgs.framerate.ToString("0.00") + " Fps";
                VideoFormat     = ReceiveArgs.format;
            }
        }

        [RelayCommand]
        public void ButtonOpen()
        {
            IsButtonOpenEnabled = false;

            string protocol = (StreamProtocol == 0) ? "rtsp" :
                              (StreamProtocol == 1) ? "rtmp" : "hls";

            if (videoCore == null)
            {
                videoCore = new VideoCore(protocol + "://" + StreamUrl);
                videoCore.OnVideoReceived   += videoCore_OnVideoReceived;
                videoCore.OnLogReceived     += VideoCore_OnLogReceived;
            }

            videoCore.Start();
            infoTimer.Start();
        }

        [RelayCommand]
        public void ButtonStop()
        {
            IsButtonOpenEnabled = true;
            //isDrawing = false;

            if (videoCore != null)
            {
                videoCore.Stop();

                // 2024.6.5 Blackcat: Add Blank Frame To Clear The Video
                ReceiveArgs = null;
                onVideoDrawFrontend?.Invoke(this, null);

                infoTimer.Stop();
            }
        }

        [RelayCommand]
        public void MenuAbout()
        {
            string ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            MessageBox.Show($"Version: {ver}\nAuthor: XOT(minexo79)\nMail: minexo79@gmail.com", "About", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        public void OpenRepo()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/minexo79/ffmpegplayer",
                UseShellExecute = true
            });
        }

        private void videoCore_OnVideoReceived(object? sender, VideoReceiveArgs e)
        {
            ReceiveArgs = e;

            onVideoDrawFrontend?.Invoke(this, null);
        }

        private void VideoCore_OnLogReceived(object? sender, LogArgs e)
        {
            LogMessage = e.logMessage ?? "";
        }

    }
}
