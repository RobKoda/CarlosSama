using DSharpPlus.VoiceNext;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Fazbot.AudioPlayer;

public class CustomAudioPlayer : IDisposable
{
    private readonly WasapiLoopbackCapture _capture;
    
    public CustomAudioPlayer()
    {
        var deviceEnumerator = new MMDeviceEnumerator();
        var devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
        var capturedDevice = devices.Single(device => device.FriendlyName.Contains("Music"));
        _capture = new WasapiLoopbackCapture(capturedDevice);
        _capture.StartRecording();
    }

    public void StreamToVoiceChannel(VoiceTransmitSink stream)
    {
        _capture.DataAvailable += (s, e) => OnStreamToChoiceChannel(s, e, stream);
    }

    public void UnStreamToVoiceChannel(VoiceTransmitSink stream)
    {
        _capture.DataAvailable -= (s, e) => OnStreamToChoiceChannel(s, e, stream);
    }

    private async Task OnStreamToChoiceChannel(object? sender, WaveInEventArgs e, VoiceTransmitSink stream)
    {
        byte[] pcmBuffer = e.Buffer;
        int bytesRecorded = e.BytesRecorded;

        pcmBuffer = ToPCM16(pcmBuffer, bytesRecorded, _capture.WaveFormat);
        
        await stream.WriteAsync(pcmBuffer, 0, pcmBuffer.Length);
    }
    
    public async Task PlayAudioAsync(string fileName, bool looping, WaveOutEvent waveOutEvent, CancellationToken cancellationToken)
    {
        Task.Run(() => CreateThreadPlayAudioAsync(fileName, looping, waveOutEvent, cancellationToken), cancellationToken);
    }
    
    private async Task CreateThreadPlayAudioAsync(string fileName, bool looping, WaveOutEvent waveOutEvent, CancellationToken cancellationToken)
    {
        await using var audioFile = new Mp3FileReader(fileName);
        waveOutEvent.DeviceNumber = GetOutputDeviceIndex();

        if (looping)
        {
            var loop = new LoopStream(audioFile);
            waveOutEvent.Init(loop);
        }
        else
        {
            waveOutEvent.Init(audioFile);    
        }
        
        waveOutEvent.Play();
        while (waveOutEvent.PlaybackState == PlaybackState.Playing)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
    
    static int GetOutputDeviceIndex()
    {
        var waveInDevices = WaveOut.DeviceCount;
        for (var waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
        {
            var deviceInfo = WaveOut.GetCapabilities(waveInDevice);
            if (deviceInfo.ProductName.Contains("Music")) return waveInDevice;
        }

        return 0;
    }

    public void Dispose()
    {
        _capture.StopRecording();
        _capture.Dispose();
    }
    
    /// <summary>
    /// Converts an IEEE Floating Point audio buffer into a 16bit PCM compatible buffer.
    /// </summary>
    /// <param name="inputBuffer">The buffer in IEEE Floating Point format.</param>
    /// <param name="length">The number of bytes in the buffer.</param>
    /// <param name="format">The WaveFormat of the buffer.</param>
    /// <returns>A byte array that represents the given buffer converted into PCM format.</returns>
    public byte[] ToPCM16(byte[] inputBuffer, int length, WaveFormat format)
    {
        if (length == 0)
            return new byte[0]; // No bytes recorded, return empty array.

        // Create a WaveStream from the input buffer.
        using var memStream = new MemoryStream(inputBuffer, 0, length);
        using var inputStream = new RawSourceWaveStream(memStream, format);

        // Convert the input stream to a WaveProvider in 16bit PCM format with sample rate of 48000 Hz.
        var convertedPCM = new SampleToWaveProvider16(
            new WdlResamplingSampleProvider(
                new WaveToSampleProvider(inputStream),
                48000)
        );

        byte[] convertedBuffer = new byte[length];

        using var stream = new MemoryStream();
        int read;
            
        // Read the converted WaveProvider into a buffer and turn it into a Stream.
        while ((read = convertedPCM.Read(convertedBuffer, 0, length)) > 0)
            stream.Write(convertedBuffer, 0, read);

        // Return the converted Stream as a byte array.
        return stream.ToArray();
    }
}