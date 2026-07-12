using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Windows;

namespace SmartServePOS.Services;

public class SingleInstanceService
{
    private const string PipeName = "SmartServePOS_Pipe";
    private readonly CancellationTokenSource _cts = new();

    public void StartListening()
    {
        Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.In);

                    await server.WaitForConnectionAsync(_cts.Token);

                    using var reader = new StreamReader(server);

                    var message = await reader.ReadLineAsync();

                    if (message == "ACTIVATE")
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var window = Application.Current.MainWindow;

                            if (window == null)
                                return;

                            if (window.WindowState == WindowState.Minimized)
                                window.WindowState = WindowState.Normal;

                            window.Activate();
                            window.Topmost = true;
                            window.Topmost = false;
                            window.Focus();
                        });
                    }
                }
                catch
                {
                }
            }
        });
    }

    public void Stop()
    {
        _cts.Cancel();
    }

    public static void NotifyExistingInstance()
    {
        try
        {
            using var client = new NamedPipeClientStream(
                ".",
                PipeName,
                PipeDirection.Out);

            client.Connect(500);

            using var writer = new StreamWriter(client)
            {
                AutoFlush = true
            };

            writer.WriteLine("ACTIVATE");
        }
        catch
        {
        }
    }
}