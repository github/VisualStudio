using System;
using System.Threading.Tasks;

namespace GitHub.App.Models
{
    public interface IHttpListenerContext
    {
        //
        // Summary:
        //     Gets the System.Net.HttpListenerRequest that represents a client's request for
        //     a resource.
        //
        // Returns:
        //     An System.Net.HttpListenerRequest object that represents the client request.
        public HttpListenerRequest Request { get; }

        //
        // Summary:
        //     Gets the System.Net.HttpListenerResponse object that will be sent to the client
        //     in response to the client's request.
        //
        // Returns:
        //     An System.Net.HttpListenerResponse object used to send a response back to the
        //     client.
        public HttpListenerResponse Response { get; }
        
        //
        // Summary:
        //     Gets an object used to obtain identity, authentication information, and security
        //     roles for the client whose request is represented by this System.Net.HttpListenerContext
        //     object.
        //
        // Returns:
        //     An System.Security.Principal.IPrincipal object that describes the client, or
        //     null if the System.Net.HttpListener that supplied this System.Net.HttpListenerContext
        //     does not require authentication.
        public IPrincipal User { get; }

        //
        // Summary:
        //     Accept a WebSocket connection as an asynchronous operation.
        //
        // Parameters:
        //   subProtocol:
        //     The supported WebSocket sub-protocol.
        //
        // Returns:
        //     Returns System.Threading.Tasks.Task`1.The task object representing the asynchronous
        //     operation. The System.Threading.Tasks.Task`1.Result property on the task object
        //     returns an System.Net.WebSockets.HttpListenerWebSocketContext object.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     subProtocol is an empty string-or- subProtocol contains illegal characters.
        //
        //   T:System.Net.WebSockets.WebSocketException:
        //     An error occurred when sending the response to complete the WebSocket handshake.
        public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol);
        //
        // Summary:
        //     Accept a WebSocket connection specifying the supported WebSocket sub-protocol
        //     and WebSocket keep-alive interval as an asynchronous operation.
        //
        // Parameters:
        //   subProtocol:
        //     The supported WebSocket sub-protocol.
        //
        //   keepAliveInterval:
        //     The WebSocket protocol keep-alive interval in milliseconds.
        //
        // Returns:
        //     Returns System.Threading.Tasks.Task`1.The task object representing the asynchronous
        //     operation. The System.Threading.Tasks.Task`1.Result property on the task object
        //     returns an System.Net.WebSockets.HttpListenerWebSocketContext object.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     subProtocol is an empty string-or- subProtocol contains illegal characters.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     keepAliveInterval is too small.
        //
        //   T:System.Net.WebSockets.WebSocketException:
        //     An error occurred when sending the response to complete the WebSocket handshake.
        public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, TimeSpan keepAliveInterval);
        //
        // Summary:
        //     Accept a WebSocket connection specifying the supported WebSocket sub-protocol,
        //     receive buffer size, and WebSocket keep-alive interval as an asynchronous operation.
        //
        // Parameters:
        //   subProtocol:
        //     The supported WebSocket sub-protocol.
        //
        //   receiveBufferSize:
        //     The receive buffer size in bytes.
        //
        //   keepAliveInterval:
        //     The WebSocket protocol keep-alive interval in milliseconds.
        //
        // Returns:
        //     Returns System.Threading.Tasks.Task`1.The task object representing the asynchronous
        //     operation. The System.Threading.Tasks.Task`1.Result property on the task object
        //     returns an System.Net.WebSockets.HttpListenerWebSocketContext object.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     subProtocol is an empty string-or- subProtocol contains illegal characters.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     keepAliveInterval is too small.-or- receiveBufferSize is less than 16 bytes-or-
        //     receiveBufferSize is greater than 64K bytes.
        //
        //   T:System.Net.WebSockets.WebSocketException:
        //     An error occurred when sending the response to complete the WebSocket handshake.
        public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, int receiveBufferSize, TimeSpan keepAliveInterval);
        //
        // Summary:
        //     Accept a WebSocket connection specifying the supported WebSocket sub-protocol,
        //     receive buffer size, WebSocket keep-alive interval, and the internal buffer as
        //     an asynchronous operation.
        //
        // Parameters:
        //   subProtocol:
        //     The supported WebSocket sub-protocol.
        //
        //   receiveBufferSize:
        //     The receive buffer size in bytes.
        //
        //   keepAliveInterval:
        //     The WebSocket protocol keep-alive interval in milliseconds.
        //
        //   internalBuffer:
        //     An internal buffer to use for this operation.
        //
        // Returns:
        //     Returns System.Threading.Tasks.Task`1.The task object representing the asynchronous
        //     operation. The System.Threading.Tasks.Task`1.Result property on the task object
        //     returns an System.Net.WebSockets.HttpListenerWebSocketContext object.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     subProtocol is an empty string-or- subProtocol contains illegal characters.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     keepAliveInterval is too small.-or- receiveBufferSize is less than 16 bytes-or-
        //     receiveBufferSize is greater than 64K bytes.
        //
        //   T:System.Net.WebSockets.WebSocketException:
        //     An error occurred when sending the response to complete the WebSocket handshake.
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync(string subProtocol, int receiveBufferSize, TimeSpan keepAliveInterval, ArraySegment<byte> internalBuffer);
    }
}
