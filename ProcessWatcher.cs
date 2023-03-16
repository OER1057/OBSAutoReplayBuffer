using System.Diagnostics;
namespace OBSAutoReplayBuffer;
/// <summary>
/// Watch running processes and make events.
/// </summary>
public class ProcessWatcher : IDisposable
{
    readonly string[] _processNames;
    readonly CancellationTokenSource _watcherCTS = new CancellationTokenSource();
    int? _pollRate;
    Task? _watcherTask;
    /// <summary>
    /// Initialize a new instance of the ProcessWatcher class.
    /// </summary>
    /// <param name="processNames">Names of processes to watch</param>
    public ProcessWatcher(string[] processNames)
    {
        _processNames = processNames;
    }
    /// <summary>
    /// Triggered when the number of processes become 1 or more from zero.
    /// </summary>
    public event EventHandler? OnFirstProcessStart;
    /// <summary>
    /// Triggered when the number of processes become zero from 1 or more.
    /// </summary>
    public event EventHandler? OnAllProcessesEnd;
    /// <summary>
    /// Start watching processes.
    /// </summary>
    /// <param name="pollRate">Intervals between each check of processes.</param>
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
    /// <summary>
    /// Releases the unmanaged resources and disposes of the managed resources used by the ProcessWatcher.
    /// </summary>
    public void Dispose()
    {
        _watcherCTS.Cancel();
        _watcherTask?.Wait();
        _watcherCTS.Dispose();
    }
}
