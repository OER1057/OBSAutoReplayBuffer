using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using McMaster.Extensions.CommandLineUtils;
using OBSWebsocketDotNet;

namespace OBSAutoReplayBuffer;
class Program
{
    [Argument(0, "Process Name", "Name of process to detect.")]
    [Required]
    static string[] ProcessNames { get; set; } = new string[0];
    [Option("-o|--obs", Description = "Specify OBS executable file path.")]
    static string OBSExe { get; set; } = @"C:\Program Files\obs-studio\bin\64bit\obs64.exe";
    [Option("-p|--port", Description = "Specify port of obs-websocket server.")]
    static int Port { get; set; } = 4455;
    [Option("-w|--password", Description = "Specify password of obs-websocket server if set.")]
    static string Password { get; set; } = "";
    static void Main(string[] args)
    {
        CommandLineApplication.Execute<Program>(args);
    }
    static OBSWebsocket ws = new OBSWebsocket();
    static bool Connected { get; set; } = false;
    private async Task OnExecute()
    {
        ws.Connected += (o, e) => { Connected = true; Console.WriteLine("Connected"); };
        ws.Disconnected += (o, e) => { Connected = false; Console.WriteLine("Disconnected"); };
        await LaunchOBS();
        await WatchProcess();
    }

    static async Task LaunchOBS()
    {
        if (Process.GetProcessesByName("obs64").Length == 0)
        {
            var startInfo = new ProcessStartInfo
            {
                Arguments = "--minimize-to-tray",
                UseShellExecute = false,
                FileName = OBSExe,
                WorkingDirectory = Path.GetDirectoryName(OBSExe),
            };
            Console.WriteLine("Launching OBS");
            Process.Start(startInfo);
            Console.WriteLine("Give OBS 5 sec to launch obs-websocket");
            await Task.Delay(5000); // すぐにつなぐと一旦つながるが切られる。リプレイバッファ開始してから切られるとめんどい
        }
        if (!Connected)
        {
            ws.ConnectAsync($"ws://localhost:{Port}", "HfdQPy8MsFNBUDlI");
            while (!Connected)
            {
                // 接続まで待機
                await Task.Delay(500);
            }
        }
    }

    static async Task OnProcessStart()
    {
        Console.WriteLine("Process Start");
        await LaunchOBS();
        ws.StartReplayBuffer();
    }
    static void OnProcessEnd()
    {
        Console.WriteLine("Process End");
        if (Connected)
        {
            ws.StopReplayBuffer();
        }
    }
    static bool IsRunning { get; set; } = false;
    static readonly int WatchInterval = 1000;
    static async Task WatchProcess()
    {
        Console.WriteLine("Start Monitoring");
        while (true)
        {
            bool wasRunning = IsRunning;
            bool isRunning = false;
            foreach (string processName in ProcessNames)
            {
                if (Process.GetProcessesByName(processName).Length > 0)
                {
                    isRunning = true;
                    break;
                }
            }
            if (!wasRunning && isRunning)
            {
                _ = Task.Run(OnProcessStart);
            }
            else if (wasRunning && !isRunning)
            {
                _ = Task.Run(OnProcessEnd);
            }
            IsRunning = isRunning;
            await Task.Delay(500);
        }
    }
}
