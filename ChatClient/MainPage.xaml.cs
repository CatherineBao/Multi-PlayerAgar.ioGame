using LoggerLibrary;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetworkingLibrary;
using Microsoft.Maui.Controls;

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
/// This project contains the GUi for the chat client which allows a user to 
/// connect to the server, receive messages, and disconnect from the server.
/// </summary>
/// 
namespace ChatClient
{
    /// <summary>
    /// The Chat Client GUI
    /// </summary>
    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// Indicates if the button should connect or disconnect the server
        /// </summary>
        private bool connect = true;

        /// <summary>
        /// Networking object for the client
        /// </summary>
        private Networking client;

        /// <summary>
        /// Logger object used by the GUI and the client object
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes the Chat Client GUI 
        /// </summary>
        /// <param name="logger">Logger object used by the GUi and client object</param>
        public MainPage(ILogger<MainPage> logger)
        {
            this.logger = logger;
            CustomFileLoggerProvider loggerProvider = new CustomFileLoggerProvider();
            InitializeComponent();
            Loaded += OnStart;
        }

        /// <summary>
        /// Disables the send button when the GUI first starts
        /// </summary>
        /// <param name="sender">Chat Client GUI</param>
        /// <param name="e">On initial load for chat client</param>
        private void OnStart(object? sender, EventArgs e)
        {
            send.IsEnabled = false;
            textEnter.IsEnabled = false;
        }

        /// <summary>
        /// Connects or disconnects the client depending on the state of the join button
        /// </summary>
        /// <param name="sender">join button</param>
        /// <param name="e">Onclick</param>
        private async void joinServer(object sender, EventArgs e)
        {
            String timeStamp = DateTime.Now.ToString();

            Dispatcher.Dispatch(() =>
            {
                if (serverEntry.Text.Equals("") || usernameEntry.Text.Equals(""))
                {
                    lock (chatDisplay)
                    {
                        chatDisplay.Text = $"({timeStamp}) Username and Server must be established\n" + chatDisplay.Text;
                    }
                    return;
                }
            });


            if (connect)
            {
                try
                {
                    connect = false;
                    this.client = new Networking(NullLogger.Instance, onConnect, onDisconnect, onMessage);
                    client.ID = usernameEntry.Text;
                    await client.ConnectAsync(serverEntry.Text, 11000);
                }
                catch (Exception ex)
                {
                    lock (chatDisplay)
                    {
                        chatDisplay.Text = $"({timeStamp}) Error connecting to the {serverEntry.Text}: {ex.Message}\n" + chatDisplay.Text; ;
                    }
                    logger.LogInformation($"Error connecting to the {serverEntry.Text}: {ex.Message}\n");
                    connect = true;
                }
            }
            else
            {
                if (client.IsConnected)
                {
                    try
                    {
                        client.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        lock (chatDisplay)
                        {
                            chatDisplay.Text = $"({timeStamp}) Error disconnecting from the {serverEntry.Text}: {ex.Message}\n" + chatDisplay.Text; ;
                        }
                        logger.LogInformation($"Error disconnecting from the {serverEntry.Text}: {ex.Message}\n");
                    }
                }
            }
        }

        /// <summary>
        /// Updates the client GUI when the client successfully disconnects from the server
        /// </summary>
        /// <param name="channel">the current networking object disconnecting</param>
        private void onDisconnect(Networking channel)
        {
            String timeStamp = DateTime.Now.ToString();
            Dispatcher.Dispatch(() =>
            {
                lock (chatDisplay)
                {
                    chatDisplay.Text = $"({timeStamp}) You disconnected from the server :<\n" + chatDisplay.Text;
                }
                join.Text = "Join Server";
                serverEntry.IsReadOnly = false;
                usernameEntry.IsReadOnly = false;
                lock (userDisplay)
                {
                    userDisplay.Text = "";
                }
                send.IsEnabled = false;
                textEnter.IsEnabled = false;
            });
            connect = true;
        }

        /// <summary>
        /// When the client connects it sends its ID to the server
        /// </summary>
        /// <param name="channel"> the current networking object connecting</param>
        private async void onConnect(Networking channel)
        {
            String timeStamp = DateTime.Now.ToString();
            join.Text = "Disconnect";
            serverEntry.IsReadOnly = true;
            usernameEntry.IsReadOnly = true;
            lock (chatDisplay)
            {
                chatDisplay.Text = $"({timeStamp}) You connected to the server :>\n" + chatDisplay.Text;
            }
            send.IsEnabled = true;
            textEnter.IsEnabled = true;
            await client.SendAsync($"Command Name {usernameEntry.Text}");
            new Thread(async () =>
            {
                await client.HandleIncomingDataAsync(true);
            }).Start();
        }

        /// <summary>
        /// When a message is received it will either display it or execute a command
        /// </summary>
        /// <param name="channel">Channel sending the message</param>
        /// <param name="message">Message being received</param>
        private void onMessage(Networking channel, string message)
        {
            String timeStamp = DateTime.Now.ToString();
            lock (chatDisplay)
            {
                if (message.StartsWith("Command,"))
                {
                    message = message.Substring(8);
                    Dispatcher.Dispatch(() =>
                    {
                        lock (userDisplay)
                        {
                            userDisplay.Text = "";
                            string[] clients = message.Split(",");
                            foreach (string client in clients)
                            {
                                userDisplay.Text += client + "\n";
                            }
                        }
                    });
                }
                else
                {
                    Dispatcher.Dispatch(() =>
                    {
                        lock (chatDisplay)
                        {
                            chatDisplay.Text = $"({timeStamp}) {message}\n" + chatDisplay.Text;
                        }
                    });
                }
            }
        }

        /// <summary>
        /// When the send button is sent the message is sent by the client 
        /// </summary>
        /// <param name="sender">send</param>
        /// <param name="e">onClick</param>
        private async void sendMessage(object sender, EventArgs e)
        {
            await client.SendAsync($"{textEnter.Text}");
            textEnter.Text = "";
        }
    }
}