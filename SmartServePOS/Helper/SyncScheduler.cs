using SmartServePOS.Services;

namespace SmartServePOS.Helper
{
	public class SyncScheduler : IDisposable
	{
		private readonly PeriodicTimer _timer;
		private readonly CancellationTokenSource _cts = new();

		private readonly IOrderSyncService _syncService;

		public SyncScheduler(IOrderSyncService syncService)
		{
			_syncService = syncService;
			_timer = new PeriodicTimer(TimeSpan.FromMinutes(10));
			//_timer = new PeriodicTimer(TimeSpan.FromSeconds(40));
		}

		public async Task StartAsync()
		{
			while (await _timer.WaitForNextTickAsync(_cts.Token))
			{
				await RunOnceSafeAsync();
			}
		}

		public async Task RunOnceSafeAsync()
		{
			try
			{
				if (!NetworkHelper.IsInternetAvailable())
					return;

				await _syncService.SyncPendingOrdersAsync();
			}
			catch
			{
				// NEVER crash scheduler
				// log only
			}
		}

		public void Dispose()
		{
			_cts.Cancel();
			_timer.Dispose();
		}
	}

}
