using Microsoft.AspNetCore.SignalR.Client;

namespace BusTracking.Mobile.Services
{
    /// <summary>
    /// SignalR client for real-time bus tracking.
    /// Registered as Singleton in MauiProgram so one connection is reused.
    /// </summary>
    public class TrackingHubService : ITrackingHubService
    {
        private HubConnection? _hub;

        public bool IsConnected =>
            _hub?.State == HubConnectionState.Connected;

        // Events the ViewModels subscribe to
        public event Action<decimal, decimal, decimal?, decimal?, string>? OnLocationReceived;
        public event Action<int>? OnTripEnded;
        public event Action<string?>? OnConnectionStateChanged;

        // ──────────────────────────────────────────────────────────────────
        // Connect / Disconnect
        // ──────────────────────────────────────────────────────────────────
        public async Task ConnectAsync(string jwtToken)
        {
            if (_hub?.State == HubConnectionState.Connected) return;

            _hub = new HubConnectionBuilder()
                .WithUrl($"{Constants.ApiBaseUrl}/hubs/tracking", options =>
                {
                    // JWT sent via query string — WebSocket cannot set headers
                    options.AccessTokenProvider = () => Task.FromResult<string?>(jwtToken);
                })
                .WithAutomaticReconnect([
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                ])
                .Build();

            // Wire up server → client events
            _hub.On<decimal, decimal, decimal?, decimal?, string>(
                "BusLocationUpdated",
                (lat, lng, speed, heading, time) =>
                    OnLocationReceived?.Invoke(lat, lng, speed, heading, time));

            _hub.On<int>("TripEnded",
                tripId => OnTripEnded?.Invoke(tripId));

            _hub.Reconnecting += ex => { OnConnectionStateChanged?.Invoke("Reconnecting…"); return Task.CompletedTask; };
            _hub.Reconnected += _ => { OnConnectionStateChanged?.Invoke(null); return Task.CompletedTask; };
            _hub.Closed += ex => { OnConnectionStateChanged?.Invoke("Disconnected"); return Task.CompletedTask; };

            await _hub.StartAsync();
            OnConnectionStateChanged?.Invoke(null);
        }

        public async Task DisconnectAsync()
        {
            if (_hub is not null)
            {
                await _hub.StopAsync();
                await _hub.DisposeAsync();
                _hub = null;
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Driver side — join + broadcast location
        // ──────────────────────────────────────────────────────────────────
        public Task JoinAsDriverAsync(int tripId) =>
            SafeInvokeAsync("JoinAsDriver", tripId);

        public Task SendLocationAsync(
            int tripId, int busId,
            decimal lat, decimal lng,
            decimal? speed, decimal? heading) =>
            SafeInvokeAsync("SendLocation", tripId, busId, lat, lng, speed, heading);

        public Task NotifyTripEndedAsync(int tripId) =>
            SafeInvokeAsync("TripEnded", tripId);

        // ──────────────────────────────────────────────────────────────────
        // Watcher side — subscribe to a trip group
        // ──────────────────────────────────────────────────────────────────
        public Task WatchTripAsync(int tripId) =>
            SafeInvokeAsync("WatchTrip", tripId);

        public Task StopWatchingAsync(int tripId) =>
            SafeInvokeAsync("LeaveTrip", tripId);

        // ──────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────
        private async Task SafeInvokeAsync(string method, params object?[] args)
        {
            if (_hub is null || _hub.State != HubConnectionState.Connected) return;
            try { await _hub.InvokeCoreAsync(method, args); }
            catch { /* network errors are non-fatal */ }
        }
    }
}
