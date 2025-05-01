using LoggerLibrary;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using NetworkingLibrary;

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
/// This project contains the GUi for the chat server which allows a user to 
/// start a server and start waiting for clients for a client-server 
/// text communication application. 
/// </summary>
/// 
namespace ChatServer;

/// <summary>
/// The Chat Server GUI 
/// </summary>
public partial class MainPage : ContentPage
{
    /// <summary>
    /// Networking object for the server
    /// </summary>
    private Networking server;

    /// <summary>
    /// Indicates if the button should connect or disconnect the server
    /// </summary>
    private bool connect = true;

    /// <summary>
    /// A list of all clients connected to the server
    /// </summary>
    private List<Networking> clients = new List<Networking>();

    /// <summary>
    /// Logger object used by the GUI and the server object
    /// </summary>
    private readonly ILogger logger;

    /// <summary>
    /// Initializes the Chat Server GUI 
    /// </summary>
    /// <param name="logger">Logger object used by the GUi and server object</param>
    public MainPage(ILogger<MainPage> logger)
    {
        this.logger = logger;
        CustomFileLoggerProvider loggerProvider = new CustomFileLoggerProvider();

        InitializeComponent();
        Loaded += onStart;
        this.logger.LogInformation($"Chat Server Constructed");
    }

    /// <summary>
    /// Sets the serverEntry text to the machine name by default
    /// </summary>
    /// <param name="sender">Chat Server GUI</param>
    /// <param name="e">On initial load for chat server</param>
    private void onStart(object? sender, EventArgs e)
    {
        IPAddress[] address = Dns.GetHostAddresses(Dns.GetHostName());
        IPAddress ipv4Address = address.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        serverIPEntry.Text = ipv4Address.ToString();
        serverEntry.Text = Environment.MachineName;
    }

    /// <summary>
    /// A helper method that updates the client list and shares them with the clients if
    /// a new client joins or an existing client disconnects
    /// </summary>
    /// <param name="channel">client that joined or disconnected</param>
    /// <returns>Task allows updateClientList to run async</returns>
    async private Task updateClientList(Networking channel)
    {
        // Command, indicates that the client should use this message to update the user list
        String clientList = "Command,";

        lock (userDisplay)
        {
            Dispatcher.Dispatch(() =>
            {
                userDisplay.Text = "";
                foreach (var client in clients)
                {
                    userDisplay.Text += $"{client.ID}\n";
                }
            });
        }

        foreach (var client in clients)
        {
            clientList += client.ID + ",";
        }

        new Thread(async () =>
        {
            foreach (var client in clients)
            {
                await client.SendAsync(clientList);
            }
        }).Start();
    }

    /// <summary>
    /// Either starts are ends the server depending on connect
    /// </summary>
    /// <param name="sender">serverState button</param>
    /// <param name="e">Onclick</param>
    async private void startServer(object sender, EventArgs e)
    {
        String timeStamp = DateTime.Now.ToString();

        if (connect)
        {
            try
            {
                connect = false;
                this.server = new Networking(NullLogger.Instance, onConnect, onDisconnect, onMessage);
                server.ID = serverEntry.Text;
                lock (chatDisplay)
                {
                    chatDisplay.Text = $"({timeStamp}) Server {serverEntry.Text} started on port 11000\n" + chatDisplay.Text;
                }
                serverState.Text = "End Server";
                serverEntry.IsReadOnly = true;
                serverIPEntry.IsReadOnly = true;
                await server.WaitForClientsAsync(11000, infinite: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting server: " + ex.Message);
            }
        }
        else
        {
            server.StopWaitingForClients();
            server.StopWaitingForMessages();
            server.Disconnect();

            /// Disconnect all the clients manually from the server side 
            closeAllClients();
            server.Disconnect();
            server.StopWaitingForClients();

            lock (chatDisplay)
            {
                chatDisplay.Text = $"({timeStamp}) Server {serverEntry.Text} Shutdown Correctly\n" + chatDisplay.Text; ;
            }

            lock (userDisplay)
            {
                userDisplay.Text = "";
            }

            serverState.Text = "Start Server";
            serverEntry.IsReadOnly = false;
            serverIPEntry.IsReadOnly = false;
            connect = true;
        }
    }


    /// <summary>
    /// A helper method that manuall closes all connected clients from the server 
    /// side when the server is closed.
    /// </summary>
    private async void closeAllClients()
    {
        if (clients.Count > 0)
        {
            await clients[0].SendAsync("Command,");
        }
        foreach (var client in clients)
        {
            client.StopWaitingForMessages();
            client.Disconnect();
        }
        clients.Clear();
    }

    /// <summary>
    /// Remove the channel from the clients list and send the updated client list with a different client
    /// </summary>
    /// <param name="channel">channel disconected from the server</param>
    private async void onDisconnect(Networking channel)
    {
        if (channel.Equals(server))
        {
            return;
        }

        String timeStamp = DateTime.Now.ToString();

        Dispatcher.Dispatch(() =>
        {
            lock (chatDisplay)
            {
                chatDisplay.Text = $"({timeStamp}) {channel.ID} disconnected from the server\n" + chatDisplay.Text; ;
            }
        });

        clients.Remove(channel);

        if (clients.Count > 0)
        {
            await updateClientList(clients[0]);
        }
        else
        {
            Dispatcher.Dispatch(() =>
            {
                lock (userDisplay)
                {
                    userDisplay.Text = "";
                }
            });
        }
    }

    /// <summary>
    /// Displays that the client connected and adds it to the client list
    /// </summary>
    /// <param name="channel">Channel being connected to the server</param>
    private async void onConnect(Networking channel)
    {
        await Task.Delay(50);
        String timeStamp = DateTime.Now.ToString();
        lock (chatDisplay)
        {
            Dispatcher.Dispatch(() =>
            {
                chatDisplay.Text = $"({timeStamp}) {channel.ID} connected to the server\n" + chatDisplay.Text;
            });
        }
        clients.Add(channel);
        await updateClientList(channel);
    }

    /// <summary>
    /// If the message starts with Command Name then the client ID is updated otherwise 
    /// send the message to the other clients as a message
    /// </summary>
    /// <param name="channel">Client sending the message</param>
    /// <param name="message">Message sent by the client</param>
    private async void onMessage(Networking channel, String message)
    {
        String timeStamp = DateTime.Now.ToString();

        Dispatcher.Dispatch(async () =>
        {
            if (message.StartsWith("Command Name"))
            {
                channel.ID = message.Substring(13);
                await updateClientList(channel);
                return;
            }
            else
            {
                foreach (var client in clients)
                {
                    await client.SendAsync($"{channel.ID}: {message}");
                }
                lock (chatDisplay)
                {
                    chatDisplay.Text = $"({timeStamp}) {channel.ID}: {message}\n" + chatDisplay.Text;
                }
            }
        });
    }
}