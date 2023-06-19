using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DankWaifu.Celly;
using DankWaifu.Collections;
using DankWaifu.Net;
using DankWaifu.Sys;
using DankWindowsWaifu.WPF;
using MessageBox = System.Windows.MessageBox;

namespace TextNow.Creator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly SettingsDataGrid _settingsDataGrid;

        public MainWindow()
        {
            InitializeComponent();
            WorkerMonitorSource = new ObservableCollection<DataGridItem>();
            WorkerMonitor.DataContext = this;
            Settings.Load();
            _settingsDataGrid = SettingsDataGrid();

            Title = $"{Assembly.GetExecutingAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version}";

            WorkerMonitor.LoadingRow += (o, args) =>
            {
                args.Row.Header = (args.Row.GetIndex() + 1).ToString();
            };

            var test = CellyHelpers.RandomCellyCarrierInfo();
            var test2 = CellyHelpers.RandomIccid(test);
        }

        public ObservableCollection<DataGridItem> WorkerMonitorSource { get; }

        private SettingsDataGrid SettingsDataGrid()
        {
            var settings = new ObservableCollection<SettingObj>
            {
                new SettingPrimitive<int>("Max workers", Constants.MaxWorkers, 1),
                new SettingPrimitive<int>("Max creates", Constants.MaxCreates, 1),
                new SettingConcurrentQueue(Constants.Proxies),
                new SettingConcurrentQueue("Reserve phone number proxies", Constants.ReservePhoneNumberProxies),
                new SettingConcurrentQueue("Validate captcha proxies", Constants.ValidateCaptchaProxies),
                new SettingConcurrentQueue("First names", Constants.FirstNames),
                new SettingConcurrentQueue("Last names", Constants.LastNames),
                new SettingConcurrentQueue("Area codes", Constants.AreaCodes),
                new SettingPrimitive<int>("2captcha timeout (seconds)", Constants.TwoCaptchaTimeout, 120),
                new SettingPrimitive<int>("Max reserve phone number attempts", Constants.MaxReservePhoneNumberAttempts, 10),
                new SettingPrimitive<string>("2captcha API key", Constants.TwoCaptchaAPIKey, string.Empty),
                new SettingPrimitive<bool>("Log all exceptions?", Constants.LogAllExceptions, false)
            };

            var settingsPage = (TabItem)TbMain.Items[1];
            var gridContent = (Grid)settingsPage.Content;
            var ret = new SettingsDataGrid(this, gridContent, settings);
            ret.CreateUi();
            return ret;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _settingsDataGrid.SavePrimitives();
            Process.GetCurrentProcess().Kill();
        }

        private List<WebProxy> Proxies(string settingKey)
        {
            var ret = new List<WebProxy>();
            var proxyStrs = _settingsDataGrid.GetConcurrentQueue(settingKey);
            if (proxyStrs.Count == 0)
                return ret;

            while (proxyStrs.Count > 0)
            {
                var str = proxyStrs.GetNext(false);
                if (NetHelpers.TryParseProxy(str, out var proxy))
                    ret.Add(proxy);
            }

            return ret;
        }

        private Collections CreateCollections()
        {
            var proxies = Proxies(Constants.Proxies);
            if (proxies.Count == 0)
                throw new InvalidOperationException("proxies list is empty");

            var reservePhoneNumberProxies = Proxies(Constants.ReservePhoneNumberProxies);
            if (reservePhoneNumberProxies.Count == 0)
                throw new InvalidOperationException("reserve phone number proxies is empty");

            var validateCaptchaProxies = Proxies(Constants.ValidateCaptchaProxies);
            
            var collections = new Collections(
                new ConcurrentQueue<WebProxy>(proxies),
                new ConcurrentQueue<WebProxy>(reservePhoneNumberProxies),
                new ConcurrentQueue<WebProxy>(validateCaptchaProxies), 
                _settingsDataGrid
            );

            if (collections.FirstNames.Count == 0)
                throw new InvalidOperationException("words1 is empty");

            if (collections.LastNames.Count == 0)
                throw new InvalidOperationException("words2 is empty");

            collections.FirstNames.Shuffle();
            collections.LastNames.Shuffle();
            collections.Proxies.Shuffle();
            collections.ValidateCaptchaProxies.Shuffle();
            collections.AreaCodes.Shuffle();
            collections.ReservePhoneNumberProxies.Shuffle();
            
            return collections;
        }

        private async Task ProxyReplenisherWorker(CancellationToken c, Collections collections)
        {
            try
            {
                while (!c.IsCancellationRequested)
                {
                    try
                    {
                        var proxies = Proxies(Constants.Proxies);
                        if (proxies.Count == 0)
                            continue;

                        foreach (var proxy in proxies)
                            collections.Proxies.Enqueue(proxy);
                    }
                    finally
                    {
                        await Task.Delay(60000, c)
                            .ConfigureAwait(false);
                    }
                }
            }
            catch (TaskCanceledException) {/**/}
        }

        private void InitThreadMonitor(int maxWorkers)
        {
            WorkerMonitorSource.Clear();

            for (var i = 0; i < maxWorkers; i++)
            {
                var item = new DataGridItem
                {
                    Account = string.Empty,
                    Status = string.Empty
                };
                WorkerMonitorSource.Add(item);
            }
        }

        private async void CmdLaunch_Click(object sender, RoutedEventArgs e)
        {
            _settingsDataGrid.SavePrimitives();

            try
            {
                var collections = await Task.Run(() => CreateCollections());
                
                var maxWorkers = Settings.Get<int>(Constants.MaxWorkers);
                InitThreadMonitor(maxWorkers);

                CmdLaunch.IsEnabled = false;

                using (var c = new CancellationTokenSource())
                {
                    var stats = new Stats();
                    var statsWorker = StatsAsync(c.Token, stats);
                    var proxyReplWorker = ProxyReplenisherWorker(c.Token, collections);

                    var tasks = new List<Task>();
                    using (var writeLock = new SemaphoreSlim(1, 1))
                    {
                        for (var i = 0; i < maxWorkers; i++)
                        {
                            var cls = new TextNowCreatorWorker(
                                i,
                                WorkerMonitorSource[i],
                                collections,
                                stats,
                                writeLock
                            );
                            tasks.Add(cls.BaseAsync());
                        }

                        await Task.WhenAll(tasks);
                    }

                    c.Cancel();
                    await Task.WhenAll(statsWorker, proxyReplWorker);

                    CmdLaunch.IsEnabled = true;
                    MessageBox.Show(@"Work complete");
                }
                
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(this, ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private async Task StatsAsync(CancellationToken c, Stats stats)
        {
            try
            {
                var start = DateTime.Now;
                while (!c.IsCancellationRequested)
                {
                    var runTime = DateTime.Now.Subtract(start);
                    await Dispatcher.InvokeAsync(() =>
                    {
                        Title =
                            $"{Assembly.GetExecutingAssembly().GetName().Name} {Assembly.GetExecutingAssembly().GetName().Version} " +
                            $"[{string.Format("{3:D2}:{0:D2}:{1:D2}:{2:D2}", runTime.Hours, runTime.Minutes, runTime.Seconds, runTime.Days)}]";

                        LblAttempts.Content = $"Attempts: [{stats.Attempts:N0}]";
                        LblCreated.Content = $"Created: [{stats.Created:N0}]";
                    });

                    await Task.Delay(950, c)
                        .ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) {/**/}
        }
    }
}
