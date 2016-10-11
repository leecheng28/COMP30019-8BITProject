﻿/*
 * This is the class all ingoing and outgoing updates pass through
 * It attatches and detatches a header to the update telling the game about what will be in the update
 * It is also responsible for deciding whether 
 * 
 * Mariam Shahid  < mariams@student.unimelb.edu.au >
 * Sam Beyer     < sbeyer@student.unimelb.edu.au >
 * 
*/
using System;
using UnityEngine;
using System.Collections.Generic;

namespace xyz._8bITProject.cooperace.multiplayer
{
	public class UpdateManager : IUpdateManager, IObservable<List<byte>>
	{
        #if UNITY_EDITOR
        static bool editor = true;
        static bool uiLogger = false;
        #else
		static bool editor = false;
        static bool uiLogger = true;
        #endif
        
        // The protcol being used to attatch the header
        public static readonly byte PROTOCOL_VERSION = 0;
		// Obstacle update identifier
		public static readonly byte OBSTACLE = BitConverter.GetBytes ('o')[0];
		// Player update identifier
		public static readonly byte PLAYER = BitConverter.GetBytes ('p')[0];
		// Chat update identifier
		public static readonly byte CHAT = BitConverter.GetBytes ('t')[0];

		// List of subscribers
		private List<IListener<List<byte>>> subscribers = new List<IListener<List<byte>>> ();

		// The ChatController to which chat message updates should be sent
		public ChatController chatController;

		// Takes an update and the sender and then distributes the update to subscribers
		public void HandleUpdate (List<byte> data, string senderID)
		{
			Debug.Log ("Update recieved from " + senderID);

			// Strip the header off the update
			List<byte> header = HeaderManager.StripHeader(data);

            try {
                try {
					
                    if (header[0] == PROTOCOL_VERSION) {
						
                        if (header[1] == PLAYER) {
                            Debug.Log("Notifying everyone");
                            if (uiLogger) UILogger.Log("recieved player udpate");

							// Notify everyone of player updates (this should change to just be players)
                            NotifyAll(data);
                        }
				
                        else if (header[1] == CHAT && chatController != null) {
                            Debug.Log("Notifying ChatController");
                            if (uiLogger) UILogger.Log("recieved chat udpate");

							// Give chat controller the message
                            chatController.GiveMessage(data);

                        } // Handle other types of updates in this if/else tree
                    } // Handle other protocols in this if/else tree

                } catch (Exception e) {
                    Debug.Log("Invalid update identifier");
                    throw e;
                }
            } catch (Exception e) {
                Debug.Log("Invalid Protocol");
                throw e;
            }
		}

		// Sends an update for an obstacle
		public void SendObstacleUpdate (List<byte> data)
		{
            HeaderManager.ApplyHeader(data, OBSTACLE);
			MultiPlayerController.Instance.SendMyReliable (data);
			Debug.Log ("Sending obstacle update");
		}

		// Sends an update for a player
		public void SendPlayerUpdate (List<byte> data)
		{
            HeaderManager.ApplyHeader(data, PLAYER);
            if (editor) {
                HandleUpdate(data, "memes");
            } else {
                MultiPlayerController.Instance.SendMyUnreliable(data);
            }
            
			Debug.Log ("Sending player update");
            if (uiLogger) UILogger.Log("sending player udpate");
        }

		// Sends an update for a chat message
		public void SendTextChat (List<byte> data)
		{
            HeaderManager.ApplyHeader(data, CHAT);
			MultiPlayerController.Instance.SendMyReliable (data);
			Debug.Log ("Sending chat message");
            if (uiLogger) UILogger.Log("Sending chat message");
        }

        // An object is added to the list of subscribers
		public void Subscribe (IListener<List<byte>> o)
		{
			subscribers.Add (o);
		}

		// Notifies all subscribers of an update
		private void NotifyAll (List<byte> data) {
			foreach (IListener<List<byte>> sub in subscribers) {
				sub.Notify (data);
			}
		}
	}
}

