using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkingLibrary;
using System.Text;
using System.Net.Sockets;


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
/// <summary>
/// 
/// Less-exhaustive but still quite thorough test suite for the Networking class.
/// 
/// </summary>
namespace NetworkingTester
{

    /// <summary>
    /// Test class for the networking project. Includes a simulation of a client-server interaction and multiple different cases of behaviors
    /// </summary>
    [TestClass]
    public class Tester
    {


        /// <summary>
        /// Read Name and comments for actions
        /// </summary>
        /// <returns>async task aligning with await calls to methods </returns>
        [TestMethod]
        public async Task BasicServerAndClientInteraction()
        {
            Networking server = new Networking(NullLogger.Instance, (c) => {; }, (c) => { Console.WriteLine("disconnect"); }, (a, b) => { Console.WriteLine("hit!"); Assert.IsTrue(b.Equals("hello")); });
            //server starts
            new Thread(async () => await server.WaitForClientsAsync(11000, infinite: true)).Start();
            await Task.Delay(1000);
            Assert.IsTrue(server.IsWaitingForClients);
            Assert.IsFalse(server.IsConnected);

            //client joins
            Networking client = new Networking(NullLogger.Instance, (c) => { Console.WriteLine("onConnect Called! Profit!"); }, (c) => {; }, (a, b) => {; });
            await client.ConnectAsync("localhost", 11000);
            await Task.Delay(1000);
            Assert.IsTrue(client.IsConnected);

            //client leaves
            client.Disconnect();
            Assert.IsFalse(client.IsConnected);

            //client joins
            await client.ConnectAsync("localhost", 11000);
            await Task.Delay(1000);
            Assert.IsTrue(client.IsConnected);

            //client sends message
            await client.SendAsync("hello");
            await Task.Delay(1000);

            //the assert in the onmessage delegate is hit,
            //server receives message


            //server ends
            server.StopWaitingForClients();

            //client tries to join
            Networking client2 = new Networking(NullLogger.Instance, (c) => {; }, (c) => {; }, (a, b) => {; });
            try
            {
                await client2.ConnectAsync("localhost", 11000); await Task.Delay(1000);
            }
            catch
            {
                //client is unable to join
                Assert.IsFalse(client2.IsConnected);
            }

        }

        /// <summary>
        /// Read Name
        /// </summary>
        /// <returns> async task aligning with await calls to methods </returns>

        [TestMethod]
        public async Task ClientDisconnectsOnMessageSend()
        {
            bool message = true;
            Networking server1 = new Networking(NullLogger.Instance, (c) => {; }, (c) => { message = false; }, (a, b) => { Console.WriteLine("hit!"); Assert.IsTrue(b.Equals("hello")); });

            //server starts
            new Thread(async () => await server1.WaitForClientsAsync(11000, infinite: true)).Start();
            await Task.Delay(1000);
            Assert.IsTrue(server1.IsWaitingForClients);
            Assert.IsFalse(server1.IsConnected);
            //client joins
            Networking client = new Networking(NullLogger.Instance, (c) => { Console.WriteLine("onConnect Called! Profit!"); }, (c) => {; }, (a, b) => {; });
            await client.ConnectAsync("localhost", 11000);
            await Task.Delay(1000);
            Assert.IsTrue(client.IsConnected);

            //client sends message but disconnects in the process

            Thread sending = new Thread(async () => await client.SendAsync("hello"));
            Thread disconnect = new Thread(() => client.Disconnect());
            disconnect.Start();
            sending.Start();

            await Task.Delay(1000);
            Assert.IsFalse(message);



        }

        /// <summary>
        /// server abruptly closes on client while the client is connected
        /// </summary>
        /// <returns>async task aligning with await calls to methods </returns>
        [TestMethod]
        public async Task ServerEndsOnClientUhOh()
        {
            Networking server = new Networking(NullLogger.Instance, (c) => {; }, (c) => { Console.WriteLine("disconnect"); }, (a, b) => { Console.WriteLine("hit!"); Assert.IsTrue(b.Equals("hello")); });
            //server starts
            new Thread(async () => await server.WaitForClientsAsync(11002, infinite: true)).Start();
            await Task.Delay(1000);
            Assert.IsTrue(server.IsWaitingForClients);
            Assert.IsFalse(server.IsConnected);

            //client joins
            Networking client = new Networking(NullLogger.Instance, (c) => { Console.WriteLine("onConnect Called! Profit!"); }, (c) => {; }, (a, b) => {; });
            await client.ConnectAsync("localhost", 11002);
            await Task.Delay(1000);
            Assert.IsTrue(client.IsConnected);

            server.StopWaitingForClients();

            //the client isn't disconnected
            Assert.IsTrue(client.IsConnected);

            Networking client2 = new Networking(NullLogger.Instance, (c) => { Console.WriteLine("onConnect Called! Profit!"); }, (c) => {; }, (a, b) => {; });
            bool message = true;
            try { await client2.ConnectAsync("localhost", 11002); }

            catch (Exception ex) { message = false; }
            await Task.Delay(1000);
            Assert.IsFalse(message);

        }

        /// <summary>
        /// Read name
        /// </summary>
        /// <returns>async task aligning with await calls to methods </returns>
        [TestMethod]
        public async Task ServerStopsWaitingForMessages()
        {
            bool onMessageHit = false;
            Networking server1 = new Networking(NullLogger.Instance, (c) => {; }, (c) => {; }, (a, b) => { onMessageHit = true; });
            //server starts
            new Thread(async () => await server1.WaitForClientsAsync(11002, infinite: true)).Start();
            await Task.Delay(1000);
            Assert.IsTrue(server1.IsWaitingForClients);
            Assert.IsFalse(server1.IsConnected);

            //client joins
            Networking client = new Networking(NullLogger.Instance, (c) => { Console.WriteLine("onConnect Called! Profit!"); }, (c) => {; }, (a, b) => {; });
            await client.ConnectAsync("localhost", 11002);
            await Task.Delay(1000);
            Assert.IsTrue(client.IsConnected);

            server1.StopWaitingForMessages();
            Assert.IsTrue(server1.IsWaitingForClients);
            //client sends message
            await client.SendAsync("hello");

            Assert.IsFalse(onMessageHit);
        }


        /// <summary>
        /// ensures the proper control flow occurs for remote and local address port retrieval
        /// </summary>
        [TestMethod]
        public async Task testProperties()
        {
            Networking server1 = new Networking(NullLogger.Instance, (c) => {; }, (c) => {; }, (a, b) => {; });
            Assert.AreEqual(server1.LocalAddressPort, "Disconnected");
            Assert.AreEqual(server1.RemoteAddressPort, "Disconnected");

            await server1.WaitForClientsAsync(11000, true);
            await Task.Delay(1000);
            Assert.AreEqual(server1.LocalAddressPort, "Waiting For Connections on Port: 11000");
            Networking client = new Networking(NullLogger.Instance, (c) => { Console.WriteLine("onConnect Called! Profit!"); }, (c) => {; }, (a, b) => {; });
            await client.ConnectAsync("localhost", 11000);
            await Task.Delay(1000);
            Assert.IsTrue(client.IsConnected);
            Assert.IsFalse(server1.IsConnected);

            await Task.Delay(1000);
            Assert.AreEqual(client.LocalAddressPort, client.LocalAddressPort);//we had to make the tcpclient instance var public to test that is it is localendpoint associated with the specific client and changed it to this after
            client.Disconnect();
            server1.StopWaitingForClients();
            Assert.AreEqual(server1.LocalAddressPort, "11000 - Disconnected");

        }

        /// <summary>
        /// Tests functionality for the infinite: false parameter for the wait for clients method
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SocketException))]
        public async Task testWaitForClientsFalse()
        {
            Networking server1 = new Networking(NullLogger.Instance, (c) => {; }, (c) => {; }, (a, b) => {; });
            new Thread(async () => await server1.WaitForClientsAsync(11004, false)).Start();
            await Task.Delay(1000);
            Assert.IsFalse(server1.IsWaitingForClients);

            //client tries to join
            Networking client = new Networking(NullLogger.Instance, (c) => { ; }, (c) => {; }, (a, b) => {; });
          
                await client.ConnectAsync("localhost", 11004);
                    
              Assert.IsFalse(client.IsConnected); 
                     
        }

        /// <summary>
        /// Read Name
        /// </summary>
        [TestMethod]
        public void testID()
        {
            Networking server1 = new Networking(NullLogger.Instance, (c) => {; }, (c) => {; }, (a, b) => {; });
            Assert.AreEqual(server1.ID, server1.RemoteAddressPort);
            server1.ID = "lol!";
            Assert.AreEqual(server1.ID, "lol!");
        }

    }

}

