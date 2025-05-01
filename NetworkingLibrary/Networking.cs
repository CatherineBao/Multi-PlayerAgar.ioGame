using Communications;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
/// <summary>
/// Author:    Catherine Bao
/// Partner:   Jessie Taubert
/// Date:      April 1, 2024
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and Catherine Bao and Jessie Taubert - This work 
///            may not be copied for use in Academic Coursework.
///
/// I, Catherine Bao and Jessie Taubert, certify that I wrote this 
/// code from scratch and did not copy it in part or whole from 
/// another source.  All references used in the completion of the 
/// assignments are cited in my README file.
///
/// File Contents
/// This project contains the implementations of the Networking class as specified in the API of the INetworking
/// interface. It allows for client and server connections and handles the messaging between them, along with disconnecting.
/// The actual interaction with the user happens in the GUI portion of the solution, but the logic is contained in this project.
/// </summary>
namespace NetworkingLibrary
{
    /// <summary>
    /// This class implements the INetworking interface as specified, handling server and client connections
    /// and disconnections, along with sending and receiving messages. Handles errors and exceptions as denoted in the interface API.
    /// </summary>
    public class Networking : INetworking
    {
        /// <summary>
        /// the logger being used
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// the port being connected on
        /// </summary>
        private int? port = null;

        /// <summary>
        /// the id, which may be a custom name or the remote address port
        /// </summary>
        private string id;

        /// <summary>
        /// connection delegate
        /// </summary>
        private readonly ReportConnectionEstablished onConnect;

        /// <summary>
        /// disconnection delegate
        /// </summary>
        private readonly ReportDisconnect onDisconnect;

        /// <summary>
        /// message delegate
        /// </summary>
        private readonly ReportMessageArrived onMessage;

        /// <summary>
        /// the client connected to the server
        /// </summary>
        private TcpClient client;

        /// <summary>
        /// cancellation token management
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// The central connection point.
        /// </summary>
        private TcpListener network_listener;

        /// <summary>
        /// A constructor for a networking object that takes in the three delegates and a logger
        /// </summary>
        /// <param name="logger"> the logger to be used</param>
        /// <param name="onConnect"> a delegate for a function that handles connections</param>
        /// <param name="onDisconnect"> a delegate for a function that handles disconnections</param>
        /// <param name="onMessage">a delegate for a function that handles messages</param>
        public Networking(ILogger logger,
            ReportConnectionEstablished onConnect,
            ReportDisconnect onDisconnect,
            ReportMessageArrived onMessage)
        {
            this.logger = logger;
            this.onConnect = onConnect;
            this.onDisconnect = onDisconnect;
            this.onMessage = onMessage;
            IsWaitingForClients = false;
            cancellationTokenSource = new CancellationTokenSource();
        }

        /// <inheritdoc/>
        public string ID
        {
            get
            {
                if (id == null)
                    return ((INetworking)this).RemoteAddressPort;
                else
                    return id;
            }
            set => id = value;
        }

        /// <inheritdoc/>
        public bool IsConnected => client != null && client.Connected;

        /// <inheritdoc/>
        public bool IsWaitingForClients { get; private set; }

        /// <inheritdoc/>
        public string RemoteAddressPort
        {
            get
            {
                if (IsConnected)
                    return client.Client.RemoteEndPoint.ToString();
                else if (IsWaitingForClients)
                    return $"Waiting For Connections on Port: {port}";
                else
                    return "Disconnected";
            }
        }

        /// <inheritdoc/>
        public string LocalAddressPort
        {
            get
            {
                if (IsConnected)
                    return client.Client.LocalEndPoint.ToString();
                else if (IsWaitingForClients)
                    return $"Waiting For Connections on Port: {port}";
                else if (port == null)
                    return "Disconnected";
                else
                    return $"{port} - Disconnected";
            }
        }

        /// <inheritdoc/>
        public async Task ConnectAsync(string host, int port)
        {

            try
            {
                if (IsConnected)
                    return;
                this.client = new TcpClient();

                await client.ConnectAsync(host, port);
                logger.LogInformation($"{RemoteAddressPort} connected to {host} on port: {port}");
                onConnect(this);
            }
            catch
            {
                string errorMessage = $"Unable to connect to {host} on {port}";
                logger.LogError(errorMessage);
                throw;
            }
        }

        /// <inheritdoc/>
        public void Disconnect()
        {
            client?.Close();
        }

        /// <inheritdoc/>
        public async Task HandleIncomingDataAsync(bool infinite = true)
        {
            if (IsConnected)
            {
                try
                {
                    StringBuilder dataBacklog = new StringBuilder();
                    byte[] buffer = new byte[4096];
                    NetworkStream stream = client.GetStream();

                    if (stream == null)
                    {
                        onDisconnect(this);
                        return;
                    }

                    while (true)
                    {
                        int total = await stream.ReadAsync(buffer);

                        if (total == 0)
                        {
                            onDisconnect(this);
                            logger.LogDebug($"Connection quit unexpectedly. End of Stream Reached. Connection must be closed");
                            return;
                        }

                        string current_data = Encoding.UTF8.GetString(buffer, 0, total);

                        dataBacklog.Append(current_data);

                        logger.LogInformation($"Received {total} new bytes for a total of {dataBacklog.Length}.");

                        this.CheckForMessage(dataBacklog);
                    }
                }
                catch (Exception ex)
                {
                    onDisconnect(this);
                    logger.LogDebug(ex.Message);
                }
            }
        }



        /// <summary>
        ///   Given a string (actually a string builder object)
        ///   check to see if it contains one or more messages as defined by
        ///   our protocol (the newline character '\n').
        /// </summary>
        /// <param name="data"> all characters encountered so far</param>
        private void CheckForMessage(StringBuilder data)
        {
            string allData = data.ToString();
            int terminator_position = allData.IndexOf("\n");

            while (terminator_position >= 0)
            {
                string message = allData.Substring(0, terminator_position + 1);
                data.Remove(0, terminator_position + 1);

                logger.LogInformation($"  Message found:\n" +
                    $"  ---------------------------------------------------------------------------------\n" +
                    $"  {message}");
                message = message.Replace("\n", "");
                onMessage(this, message);

                allData = data.ToString();
                terminator_position = allData.IndexOf("\n") - 1;
            }
        }


        /// <inheritdoc/>
        public async Task SendAsync(string text)
        {
            text = text.Replace("\n", "\\n");
            text += "\n";
            if (IsConnected)
            {
                try
                {
                    byte[] messageBytes = Encoding.UTF8.GetBytes(text);
                    NetworkStream stream = client.GetStream();
                    await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
                    logger.LogInformation($"    Message Sent from:   {client.Client.LocalEndPoint} to {client.Client.RemoteEndPoint}");
                }
                catch (Exception ex)
                {
                    onDisconnect(this);
                    logger.LogTrace($"    Client disconnected in attempt to send message: {client.Client.RemoteEndPoint} - {ex.Message}");
                }

            }
            else
            {
                logger.LogDebug($"attempted to send a message  {text} to a disconnected client.");
            }
        }

        /// <inheritdoc/>
        public void StopWaitingForClients()
        {
            cancellationTokenSource?.Cancel();
            IsWaitingForClients = false;
            logger.LogInformation("Server has stoped waiting for clients");

        }

        /// <inheritdoc/>
        public void StopWaitingForMessages()
        {
            cancellationTokenSource?.Cancel();
            logger.LogInformation("Server has stoped waiting for messages");
        }

        /// <inheritdoc/>
        public async Task WaitForClientsAsync(int port, bool infinite)
        {
            if (!infinite)
                return;
            try
            {
                IsWaitingForClients = true;
                this.port = port;
                network_listener = new TcpListener(IPAddress.Any, port);
                network_listener.Start();

                logger.LogInformation("Server has started waiting for clients");

                while (true)
                {
                    TcpClient connection = await network_listener.AcceptTcpClientAsync(cancellationTokenSource.Token);
                    Networking newClient = new Networking(logger, onConnect, onDisconnect, onMessage);
                    newClient.client = connection;
                    logger.LogInformation("Client connected.");
                    await Task.Run(() =>
                        new Thread(async () =>
                        {
                            await newClient.HandleIncomingDataAsync();
                        }).Start());

                    onConnect(newClient);
                                     
                }
            }
            catch
            {
                network_listener.Stop();
                IsWaitingForClients = false;
                onDisconnect(this);
                logger.LogDebug("Server failed to start waiting for clients.");
            }

        }
    }
}