using System;
using System.Threading;
using System.Threading.Tasks;
using DankWaifu.Sys;

namespace TextNow.Creator
{
    internal abstract class Mode
    {
        protected Mode(int index, DataGridItem ui)
        {
            Index = index;
            UI = ui;
        }

        protected int Index { get; }
        protected DataGridItem UI { get; }

        public abstract Task BaseAsync();

        protected void UpdateThreadStatus(string s, int delay)
        {
            UI.Status = s;
            Thread.Sleep(delay);
        }

        protected async Task UpdateThreadStatusAsync(string s, int delay)
        {
            await UpdateThreadStatusAsync(s, delay, CancellationToken.None)
                .ConfigureAwait(false);
        }
        
        protected async Task UpdateThreadStatusAsync(string s, int delay, CancellationToken c)
        {
            UI.Status = s;
            await Task.Delay(delay, c)
                .ConfigureAwait(false);
        }

        protected async Task WaitingForInputFile(string file)
        {
            await UpdateThreadStatusAsync($"Waiting for {file} file to be loaded", 2000)
                .ConfigureAwait(false);
        }

        protected virtual async Task OnExceptionAsync(Exception e)
        {
            await OnBackgroundExceptionAsync(e)
                .ConfigureAwait(false);
            await UpdateThreadStatusAsync($"{e.GetType().Name}: {e.Message}", 5000)
                .ConfigureAwait(false);
        }

        protected async Task OnBackgroundExceptionAsync(Exception e)
        {
            if (Settings.Get<bool>(Constants.LogAllExceptions))
            {
                await ErrorLogger.WriteAsync(e, false)
                    .ConfigureAwait(false);
                return;
            }

            switch (e)
            {
                case TimeoutException _:
                case OperationCanceledException _:
                case InvalidOperationException _:
                    break;

                default:
                    await ErrorLogger.WriteAsync(e)
                        .ConfigureAwait(false);
                    break;
            }
        }

        protected async Task DelayAsync(int seconds, string beforeDoing)
        {
            await DelayAsync(seconds, beforeDoing, CancellationToken.None)
                .ConfigureAwait(false);
        }

        protected async Task DelayAsync(int seconds, string beforeDoing, CancellationToken c)
        {
            for (var i = seconds; i > 0; i--)
            {
                var msg = i <= 1 ? $"Delaying before {beforeDoing}: {i} second remains" : $"Delaying before {beforeDoing}: {i} seconds remain";
                await UpdateThreadStatusAsync(msg, 1000, c)
                    .ConfigureAwait(false);
            }
        }
    }
}
