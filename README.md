# Networking & Logging
Author:		 Jessie Taubert\
Partner:	 Catherine Bao\
Start Date:	 March 22, 2024\
Course:		 CS 3500, University of Utah, School of Computing\
GitHub ID:	 jessietaubert && CatherineBao\
Repo:		 https://github.com/uofu-cs3500-spring24/assignment-seven-logging-and-networking-take7/tree/main 
Commit Date: April 1, 2024 09:30 PM\
Solution:	 Networking_and_Logging\
Copyright:	 CS 3500 and Jessie Taubert and Catherine Bao - This work may not be copied for use in Academic Coursework


# Overview of the Chat functionality
There are a few main components of this solution.\
The chat client:\
	The chat client allows the user to connect to server, set thier username, and send messages. They can also see the messages
	sent by other clients along with a list of all connected clients to the same server.
	
The chat server:\
	The chat server allows the user to start and end a server, set the name of the server, and the address of the server. The 
	address of the server defaults to the IP address of the host computer. The server also displays all the messages sent by
	the clients along with the a list of all connected clients to that server.

The logger:\
	There is a seperate logging file for both the client and server (two total files). The logging files are used by both the
	respective GUI and their networking object. This logger reports any action commited by the networking object and GUI along 
	with any errors that may arise.

Finally, the Networking project ties everything together and handles the actual connection logic.


Future extensions might include:\
	Making our GUI look more unique and potentially adding a more interactive visual element such a profile picture. We may also consider certain messages 
	clients send having special effects as an easter egg. 


# Time Expenditures

	Estimated: 12 hours		Actual: Jessie: 20hr Catherine: 20hr

We spent about an hour setting up the repository and the files. We spent about 6 hours on the skeleton implementation of the GUI and Networking class, as well as the logger. We spent about 10 hours testing, debugging, and commenting our code.
We spent 3 hours learning tools and techniques such as parallel programming and multi-threading. We both spent the same amount of time on the project.

Our time estimates are getting more accurrate; however, it is becoming more difficult to complete the assignments, and while we are better at identifying the difficulty, it is becoming harder to know how much
additional time we will be spending learning these new concepts that we have to implement rather than before only worrying about time for implementation. It indicates to us that our abilities to complete the assignments are fairly on par with what is expected of us.

# Comments to Evaluators
One of the tests sometimes doesn't work when ran all together. I believe this is fixed by having it connect on a different port, but this may persist? If so, it runs correctly when ran individually.

Additionally, no specifications were made for the boolean infinite parameter on WaitForClientsAsync if it was false. Because of this, we made
an assumption that if this was false, nothing would happen, ie: the server would **not** start waiting for clients, and would accept no connections.

Additionally, we made an assumption that since, in HandleIncomingDataAsync, the parameter bool infinite is passed in such that it is set equal to true (bool infinite = true ), we assumed this
to mean that we did not have to implement functionality for if this was false as it will always be true.

# Assignment specific writeups
**Testing**\
GUI Specific Testing:\
Our GUI was tested with manual testing. The system can connect with our Server and Client GUI and also the .exe files provided by the Professor for both client and the server. 
Additionally, the server can support mutliple clients (testing for locks). The GUI was initially tested with the Professor's DLL and later with our networking class, allowing 
us to debug in these separated means and identify where issues existed.

Networking Testing:\
Our networking class is tested using an MStest file. The file hits the following path:
1. Server starts
2. Client joins
3. Client leaves
4. Client joins 
5. Client sends message
6. Server ends
7. Client tries to join ended server
See the individual file for the other tests and scenarios.

We ensured thorough testing of the GUI and networking interactions. We tested joining, leaving, server closing on the client, sending messages with many participants, sending messages at the same time, ensured the participants list updated correctly upon new connections and disconnections, trying to join invalid servers, closed servers, etc. We also
added functionality for the closing of the application to disconnect the client or shut down the server respectively. We also tested sending "\n" in our messages to ensure we properly handled that case.

**Partnership**\
Jessie primarily focused on the networking class and Catherine primarily focused on the GUI, but both partners collaborated on all components of the project. Pair programming was utilized for the majority of this project, but branches were also utilized to separate the concerns and work on different functionality to not stop the
program from functioning upon the introduction of errors on branches. We ran into a slight issue when Catherine imported the Professor's DLL, which caused Jessie's Networking to no longer compile, but she didn't realize that was the issue and thought the Networking code broke. We understand how important branch is now.

Branches:\
Jessie worked on the Networking branch while Catherine worked on the GUIServerAttempt branch.
Merging caused no issues because of this division of concerns, so there was no overlap. Catherine branched back on commit 275a285 (pull request #4), and Jessie branched back on commit 7ae2bf4 (pull request #3). As mentioned, Catherine also worked on the Networking branch and Jessie on the GUI.

# Peers/References
Peers: 
1. Smyran Math
2. Mitch Briles 

References: 
1. C# Program to Find the IP Address of the Machine - https://www.geeksforgeeks.org/c-sharp-program-to-find-the-ip-address-of-the-machine/
2. How To Make a Chat Application in C# - https://www.youtube.com/watch?v=5Bp7ue91Z0k&t=516s
3. We also referenced the Professor's slides, lecture, instructions, and ForStudents Github Repository
4. Note a reference to Microsoft documentation for understanding multi-threading applications in C#, and TCPClient/Listener methods