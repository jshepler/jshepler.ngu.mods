using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace jshepler.ngu.mods.WebService.Twitch
{
    internal class WebSocketClient
    {
        private static int _socketCounter = 0;

        private CancellationTokenSource _socketTokenSource;
        private ClientWebSocket _socket;

        private void Log(string s) => Plugin.LogInfo($"[{DateTime.Now:H:mm:ss.fff}] tWSC-{SocketId}: {s}");

        internal EventHandler OnDisconnect;
        internal EventHandler<TwitchEventArgs> OnMessage;

        internal int SocketId;
        internal WebSocketState State => _socket?.State ?? WebSocketState.None;

        internal bool SocketException = false;

        internal async Task StartAsync(string url)
        {
            SocketId = Interlocked.Increment(ref _socketCounter);
            Log("StartAsync()");

            _socket = new();
            _socketTokenSource = new();

            await _socket.ConnectAsync(new Uri(url), CancellationToken.None);
            _ = Task.Run(() => SocketLoop().ConfigureAwait(false));
        }

        internal async Task StopAsync()
        {
            Log("StopAsync()");

            if (_socket == null || _socket.State != WebSocketState.Open)
                return;

            var timeoutToken = new CancellationTokenSource(3000).Token;

            try
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeoutToken);
                while (_socket != null && _socket.State != WebSocketState.Closed && !timeoutToken.IsCancellationRequested) ;
            }

            catch (OperationCanceledException)
            {
                Log("_socket.CloseAsync() timed out");
            }

            catch (Exception ex)
            {
                Log($"StopAsync() exception:\n{ex}");
            }

            finally
            {
                Log("StopAsync() ended");
            }
        }

        private async Task SocketLoop()
        {
            Log("SocketLoop()");

            var buffer = WebSocket.CreateClientBuffer(8192, 8192);
            var sb = new StringBuilder(8192);
            var bytesRead = 0;

            try
            {
                while (_socket.State != WebSocketState.Closed && !_socketTokenSource.IsCancellationRequested)
                {
                    var result = await _socket.ReceiveAsync(buffer, _socketTokenSource.Token);

                    if (_socketTokenSource.IsCancellationRequested)
                        break;

                    if (_socket.State == WebSocketState.CloseReceived && result.MessageType == WebSocketMessageType.Close)
                    {
                        //await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", CancellationToken.None);
                        break;
                    }

                    if (_socket.State != WebSocketState.Open || result.MessageType != WebSocketMessageType.Text)
                        continue;

                    sb.Append(Encoding.UTF8.GetString(buffer.Array, 0, result.Count));
                    bytesRead += result.Count;

                    if (!result.EndOfMessage)
                        continue;

                    var message = sb.ToString();
                    var e = TwitchEventArgs.From(message);

                    if (e.Message.metaData.messageType != TwitchMessage.MessageTypes.KeepAlive)
                        Log($"received {e.Message.metaData.messageType} ({bytesRead} bytes)");

                    OnMessage?.Invoke(this, e);

                    sb.Clear();
                    bytesRead = 0;
                }
            }

            catch (OperationCanceledException)
            {
                Log("socket token was cancelled");
            }

            catch (Exception ex)
            {
                Log(ex.Message);
                SocketException = true;
            }

            finally
            {
                _socket.Dispose();
                _socket = null;

                OnDisconnect?.Invoke(this, EventArgs.Empty);
                Log("SocketLoop() ended");
            }
        }
    }
}
