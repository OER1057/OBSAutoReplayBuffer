using System.Diagnostics;
namespace OBSAutoReplayBuffer;
public class ProcessWatcher : IDisposable
{
    readonly string[] _processNames;
    readonly CancellationTokenSource _watcherCTS = new CancellationTokenSource();
    int? _pollRate;
    Task? _watcherTask;
    public ProcessWatcher(string[] processNames)
    {
        _processNames = processNames;
    }
    public event EventHandler? OnFirstProcessStart;
    public event EventHandler? OnAllProcessesEnd;
    public void Start(int pollRate = 1000)
    {
        if (_watcherTask == null)
        {
            _pollRate = pollRate;
            _watcherTask = Watch(_watcherCTS.Token);
        }
    }
    async Task Watch(CancellationToken ct)
    {
        var currentProcesses = new List<Process>();
        var lastProcesses = new List<Process>();
        while (!ct.IsCancellationRequested)
        {
            currentProcesses = new List<Process>();
            foreach (var processName in _processNames)
            {
                currentProcesses.AddRange(Process.GetProcessesByName(processName));
            }
            if (lastProcesses.Count == 0 && currentProcesses.Count > 0)
            {
                OnFirstProcessStart?.Invoke(this, EventArgs.Empty);
            }
            else if (lastProcesses.Count > 0 && currentProcesses.Count == 0)
            {
                OnAllProcessesEnd?.Invoke(this, EventArgs.Empty);
            }
            lastProcesses = currentProcesses;
            await Task.Delay(_pollRate ?? 0);
        }
    }
    public void Dispose()
    {
        _watcherCTS.Cancel();
        _watcherTask?.Wait();
        _watcherCTS.Dispose();
    }
}
