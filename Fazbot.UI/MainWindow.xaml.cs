using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Fazbot.App.Services;
using Fazbot.AudioPlayer;
using Microsoft.Extensions.Configuration;
using NAudio.Wave;

namespace Fazbot.UI;

public partial class MainWindow
{
    private readonly AdminCommandsService _adminCommands;
    private readonly CustomAudioPlayer _audioPlayer;
    private readonly DiscordBotService _discordBotService;

    private readonly ObservableCollection<AudioTask> _audioTasks = [];

    private readonly string _audiosLocation;

    public MainWindow(AdminCommandsService adminCommandsService, CustomAudioPlayer audioPlayer,
        DiscordBotService discordBotService, IConfiguration configuration)
    {
        _audiosLocation = configuration["AudiosLocation"]!;
        
        _adminCommands = adminCommandsService;
        _audioPlayer = audioPlayer;
        _discordBotService = discordBotService;
        InitializeComponent();

        AudioTasksListBox.ItemsSource = _audioTasks;

        GenerateButtons();

        Task.Run(RefreshAudioTasks);
    }

    private void GenerateButtons()
    {
        var files = Directory.GetFiles(_audiosLocation, "*.mp3");
        foreach (var file in files)
        {
            var button = new Button
            {
                Content = $"♪♫ {Path.GetFileNameWithoutExtension(file)}"
            };
            button.Click += (_, _) => PlayAudio(file, false);
            button.MouseRightButtonUp += (_, _) => PlayAudio(file, true);
            ButtonsStack.Children.Add(button);
        }
    }
    
    private async void JoinVoiceChannelButton_OnClickAsync(object sender, RoutedEventArgs e) =>
        await _adminCommands.JoinVoiceChannelAsync(_discordBotService);
    private async void LeaveVoiceChannelButton_OnClickAsync(object sender, RoutedEventArgs e) => await _adminCommands.LeaveVoiceChannelAsync(_discordBotService);

    private void PlayAudio(string fileName, bool looping)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var waveOutEvent = new WaveOutEvent();
        _audioTasks.Add(new AudioTask(waveOutEvent,
            _audioPlayer.PlayAudioAsync(fileName, looping, waveOutEvent, cancellationTokenSource.Token), 
            Path.GetFileNameWithoutExtension(fileName),
            cancellationTokenSource));
    }

    private void AudioTasksListBox_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (AudioTasksListBox.SelectedItem == null) return;

        var audioTask = (AudioTasksListBox.SelectedItem as AudioTask)!;

        var popup = new VolumeAdjustment
        {
            Volume = audioTask.WaveOutEvent.Volume
        };
        popup.ShowDialog();

        audioTask.WaveOutEvent.Volume = popup.Volume;
    }

    private async void AudioTasksListBox_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (AudioTasksListBox.SelectedItem == null) return;

        var audioTask = (AudioTasksListBox.SelectedItem as AudioTask)!;
        await audioTask.CancellationTokenSource.CancelAsync();
        _audioTasks.Remove(audioTask);
    }

    private void RefreshAudioTasks()
    {
        while (true)
        {
            for (var index = _audioTasks.Count - 1; index >= 0; index--)
            {
                var audioTask = _audioTasks[index];
                if (audioTask.WaveOutEvent.PlaybackState != PlaybackState.Stopped) continue;
                
                var indexToRemove = index;
                Application.Current.Dispatcher.Invoke((Action) delegate
                {
                    _audioTasks.RemoveAt(indexToRemove);
                });
            }

            Thread.Sleep(1000);
        }
    }
}