using NAudio.Wave;

namespace Fazbot.AudioPlayer;

public record AudioTask(WaveOutEvent WaveOutEvent, Task Task, string Name, CancellationTokenSource CancellationTokenSource)
{
    public override string ToString() => Name;
}