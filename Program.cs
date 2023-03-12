using System.Diagnostics;
using OBSWebsocketDotNet;

namespace OBSAutoReplayBuffer;
class Program
{
    static string[] ProcessNames { get; set; } = new string[0];
    static string OBSExe { get; set; } = @"C:\Program Files\obs-studio\bin\64bit\obs64.exe";
    static int Port { get; set; } = 0;
    static string Password { get; set; } = "";
    static bool Connected { get; set; } = false;
    static bool IsRunning { get; set; } = false;
    static OBSWebsocket ws = new OBSWebsocket();
    static async Task OnProcessStart()
    {
        await LaunchOBS();
        ws.StartReplayBuffer();
    }
    static void OnProcessEnd()
    {
        if (Connected)
        {
            ws.StopReplayBuffer();
        }
    }
    static async Task Main(string[] args)
    {
        ProcessNames = new[] { "Notepad" };
        Port = 4455;
        Password = "";

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
                // Arguments = "--minimize-to-tray",
                UseShellExecute = false,
                FileName = OBSExe,
                WorkingDirectory = Path.GetDirectoryName(OBSExe),
            };
            Process.Start(startInfo);
            await Task.Delay(5000); // すぐにつなぐと切られる
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
    static async Task WatchProcess()
    {
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
