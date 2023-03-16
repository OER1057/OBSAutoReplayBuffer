using System.CommandLine.DragonFruit;
using System.Diagnostics;
using OBSWebsocketDotNet;

namespace OBSAutoReplayBuffer;
class Program
{
    static OBSWebsocket _ws = new OBSWebsocket();
    static bool _connected = false;
    static FileInfo _obsExe = new FileInfo(@"C:\Program Files\obs-studio\bin\64bit\obs64.exe");
    static int _port = 4455;
    static string? _password;
    static string[] _processList = new string[0];
    /// <summary>
    /// Start/Stop OBS replay buffer sync with start/end of specific processes.
    /// </summary>
    /// <param name="args">Name(s) of process to detect. [required]</param>
    /// <param name="obs">Specify OBS executable file path. [default: C:\Program Files\obs-studio\bin\64bit\obs64.exe]</param>
    /// <param name="port">Specify port of obs-websocket server. [default: 4455]</param>
    /// <param name="password">Specify password of obs-websocket server if set. [default: not set]</param>
    static async Task Main(FileInfo? obs, int? port, string? password, string[] args)
    {
        Console.Title = "OBSAutoReplayBuffer";
        Console.WriteLine("OBSAutoReplayBuffer by OER1057");
        if (Process.GetProcessesByName("OBSAutoReplayBuffer").Length >= 2)
        {
            Console.WriteLine("OBSAutoReplayBuffer is already running.");
            Environment.Exit(1);
        }

        _obsExe = obs ?? _obsExe;
        if (!_obsExe.Exists)
        {
            Console.Error.WriteLine($"{_obsExe.FullName} does not exist.");
            Environment.Exit(1);
        }
        _port = port ?? _port;
        _password = password;
        _processList = args;
        if (_processList.Length == 0)
        {
            Console.Error.WriteLine("Missing required argument(s).");
            Environment.Exit(1);
        }

        _ws.Connected += (_, _) => { _connected = true; Console.WriteLine("Connected."); };
        _ws.Disconnected += (_, _) => { _connected = false; Console.WriteLine("Disconnected."); };
        await LaunchAndConnectOBS();

        var watcher = new ProcessWatcher(_processList);
        watcher.OnAllProcessesEnd += (_, _) => OnProcessEnd();
        watcher.OnFirstProcessStart += async (_, _) => await OnProcessStart();
        watcher.Start();

        while (true) { await Task.Delay(int.MaxValue); }
    }

    static async Task LaunchAndConnectOBS()
    {
        if (Process.GetProcessesByName("obs64").Length == 0)
        {
            var startInfo = new ProcessStartInfo
            {
                Arguments = "--minimize-to-tray",
                UseShellExecute = false,
                FileName = _obsExe.FullName,
                WorkingDirectory = _obsExe.DirectoryName,
            };
            Console.WriteLine("Launching OBS . . .");
            Process.Start(startInfo);
            Console.WriteLine("Give OBS 5 sec to launch obs-websocket . . .");
            await Task.Delay(5000); // すぐにつなぐと一旦つながるが切られる。リプレイバッファ開始してから切られるとめんどい
        }
        if (!_connected)
        {
            _ws.ConnectAsync($"ws://localhost:{_port}", _password);
            while (!_connected)
            {
                // 接続まで待機
                await Task.Delay(500);
            }
        }
    }

    static async Task OnProcessStart()
    {
        Console.WriteLine("Process start. Start replay buffer.");
        await LaunchAndConnectOBS();
        _ws.StartReplayBuffer();
    }
    static void OnProcessEnd()
    {
        Console.WriteLine("Process end. Stop replay buffer.");
        if (_connected)
        {
            _ws.StopReplayBuffer();
        }
    }
}
