using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

namespace BombermanOnlineProject.Client.Network
{
	/// <summary>
	/// FASE 8.4 - Network Layer: SignalR Client Wrapper
	/// Responsibilities:
	/// - Manage lifecycle of the HubConnection
	/// - Provide typed methods for server communication
	/// - Abstract SignalR implementation details from Presenters
	/// </summary>
	public class SignalRClient
	{
		private HubConnection _connection;
		private readonly string _url;

		public bool IsConnected => _connection?.State == HubConnectionState.Connected;

		public SignalRClient(string url = "http://localhost:5054/gamehub")
		{
			_url = url;
			_connection = new HubConnectionBuilder()
				.WithUrl(_url)
				.WithAutomaticReconnect()
				.Build();
		}

		#region Connection Management

		public async Task ConnectAsync()
		{
			if (IsConnected) return;

			try
			{
				await _connection.StartAsync();
			}
			catch (Exception ex)
			{
				throw new Exception($"SignalR Connection Error: {ex.Message}");
			}
		}

		public async Task DisconnectAsync()
		{
			if (_connection != null)
			{
				await _connection.StopAsync();
				await _connection.DisposeAsync();
			}
		}

		#endregion

		#region Event Subscriptions (Server to Client)

		/// <summary>
		/// Registers a handler for a server-side event.
		/// </summary>
		public void On<T>(string methodName, Action<T> handler)
		{
			_connection.On(methodName, handler);
		}

		/// <summary>
		/// Specific handler for GameState updates (JsonElement handling)
		/// </summary>
		public void OnGameStateUpdate(Action<JsonElement> handler)
		{
			_connection.On("GameStateUpdate", handler);
		}

		#endregion

		#region Hub Actions (Client to Server)

		public async Task<string> CreateGameAsync(string playerName)
		{
			return await _connection.InvokeAsync<string>("CreateGame", playerName);
		}

		public async Task<bool> JoinGameAsync(string sessionId, string playerName)
		{
			return await _connection.InvokeAsync<bool>("JoinGame", sessionId, playerName);
		}

		public async Task StartGameAsync()
		{
			await _connection.InvokeAsync("StartGame");
		}

		public async Task MovePlayerAsync(string direction)
		{
			await _connection.SendAsync("MovePlayer", direction);
		}

		public async Task PlaceBombAsync()
		{
			await _connection.SendAsync("PlaceBomb");
		}

		public async Task LeaveGameAsync()
		{
			await _connection.SendAsync("LeaveGame");
		}

		#endregion
	}
}