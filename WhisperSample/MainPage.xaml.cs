using WhisperSample.Services;
using WhisperSample.Services.Audio;
using WhisperSample.Services.Whisper;

namespace WhisperSample;

public partial class MainPage : ContentPage
{
    private readonly RealtimeTranscriber _transcriber;
    private CancellationTokenSource? _cts;
    private Task? _runningTask;

    public MainPage(
        RealtimeTranscriber transcriber,
        WhisperModelService modelService,
        WhisperTranscriptionService whisper,
        Func<IAudioStreamSource> streamSourceFactory)
    {
        InitializeComponent();
        _transcriber = transcriber;

        App.ReportFatal = (msg) =>
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    TranscriptLabel.Text += (string.IsNullOrWhiteSpace(TranscriptLabel.Text) ? "" : "\n") + msg;
                });
            }
            catch { }
        };

    }

    private async void OnStartClicked(object? sender, EventArgs e)
    {
        if (_runningTask != null)
            return;

        StartBtn.IsEnabled = false;
        StopBtn.IsEnabled = true;

        _cts = new CancellationTokenSource();

        _runningTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var text in _transcriber.RunAsync(TimeSpan.FromSeconds(5), _cts.Token))
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        TranscriptLabel.Text += (string.IsNullOrWhiteSpace(TranscriptLabel.Text) ? "" : "\n") + text;
                    });
                }
            }
            catch (OperationCanceledException)
            {
                // stop
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    TranscriptLabel.Text += (string.IsNullOrWhiteSpace(TranscriptLabel.Text) ? "" : "\n") + "[Error] " + ex.Message;
                });
            }
        });
    }

    private async void OnStopClicked(object? sender, EventArgs e)
    {
        StopBtn.IsEnabled = false;
        _cts?.Cancel();

        try
        {
            if (_runningTask != null)
                await _runningTask;
        }
        catch
        {
            // ignore
        }
        finally
        {
            _runningTask = null;
            _cts?.Dispose();
            _cts = null;
            StartBtn.IsEnabled = true;
        }
    }
}
