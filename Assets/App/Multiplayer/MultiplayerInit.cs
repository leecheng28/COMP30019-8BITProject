﻿/*
 * Mariam Shahid  < mariams@student.unimelb.edu.au >
 * Sam Beyer     < sbeyer@student.unimelb.edu.au >
 */

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GooglePlayGames.BasicApi.Multiplayer;

namespace xyz._8bITProject.cooperace.multiplayer
{
	public class MultiplayerInit : MonoBehaviour {

		#if UNITY_EDITOR
		static bool editor = true;
		#else
		static bool editor = false;
		#endif

		public static void Init(GameObject level){
			if(editor){
				InEditorInit (level);
			} else {
				OnAndroidInit (level);
			}
		}

		/// According to monodevelop's intelligent documentation generator, this method
		/// 'Ins the editor init'. Well, I think that just about says it all!
		/// 
		/// (Actually, this method initialises the components and objects inside a
		///  level, specifically in a way to play a multiplayer game inside the unity
		///  editor, rather than on a device)
		static void InEditorInit (GameObject level){

			// Get player1 and player2 game objects
			LocalPlayerController[] players = level.GetComponentsInChildren<LocalPlayerController>();
			GameObject player1 = players[0].gameObject;
			GameObject player2 = players[1].gameObject;

			UpdateManager updateManager = new UpdateManager();

			// Make sure one player is remote
			player2.GetComponent<RemotePhysicsController> ().enabled = true;

			// Tell update manager about the serialiser for player 2 so updates get recieved
			player2.GetComponent<PlayerSerializer> ().enabled = true;
			updateManager.Subscribe(player2.GetComponent<PlayerSerializer> (), UpdateManager.Channel.PLAYER);

			// Make sure one player is local
			player1.GetComponent<LocalPlayerController> ().enabled = true;

			// Tell player 1 to send updates to the update manager
			player1.GetComponent<PlayerSerializer> ().enabled = true;
			player1.GetComponent<PlayerSerializer> ().updateManager = updateManager;


			// camera should have a reference to the player to-be-followed

			CameraController camera = FindObjectOfType<CameraController> ();
			camera.target = player1.GetComponent<ArcadePhysicsController>();



			// push blocks should also respond to collisions rather than
			// remote updates

			foreach (PushBlockController pbc in
				level.GetComponentsInChildren<PushBlockController>()){

				pbc.enabled = true;
			}


			// keys, key blocks and pressure plates should respond to
			// collisions, not remote updates!

			foreach (KeyController key in
				level.GetComponentsInChildren<KeyController>()) {

				key.enabled = true;
			}

			foreach (PressurePlateController plate in
				level.GetComponentsInChildren<PressurePlateController>()) {

				plate.enabled = true;
			}

			foreach (KeyBlockController block in
				level.GetComponentsInChildren<KeyBlockController>()) {

				block.enabled = true;
			}


		}

		/// Set up a level and its child objects and their components to
		/// play a multi-player game, deciding who is the host and enabling
		/// all of the necessary components
		/// 
		static void OnAndroidInit(GameObject level){


			// decide which player is the host by participant ID

			string myID = MultiPlayerController.Instance.GetMyParticipantId ();
			List<Participant> participants = MultiPlayerController.Instance.GetAllPlayers ();

			// the host is the participant with the first ID in the list

			bool host = (participants [0].ParticipantId.Equals (myID));

			UILogger.Log("I am the " + (host ? "host" : "client"));

			// Get localPlayer and remotePlayer gameObjects sorted

			LocalPlayerController[] players = level.GetComponentsInChildren<LocalPlayerController>();
			GameObject localPlayer;
			GameObject remotePlayer;
		
			if (host) {
				localPlayer  = players [0].gameObject;
				remotePlayer = players [1].gameObject;

			} else {
				localPlayer  = players [1].gameObject;
				remotePlayer = players [0].gameObject;

			}



			// Enter update manager, stage left!

			UpdateManager updateManager = new UpdateManager();
			MultiPlayerController.Instance.updateManager = updateManager;



			// link chat with updateManager

			ChatController chat = FindObjectOfType<ChatController>();

			updateManager.chatController = chat;
			chat.updateManager = updateManager;



			// camera should track local player

			CameraController camera = FindObjectOfType<CameraController> ();

			camera.target = localPlayer.GetComponent<ArcadePhysicsController>();



			// set up objects and their components!

			InitializePlayers (localPlayer, remotePlayer, updateManager);


			if (host) {
				InitializeObstaclesHost (level, updateManager);

			} else {
				InitializeObstaclesClient (level, updateManager);

			}


			// assign deserialiser IDs to obstacles

			byte id = 0;

			foreach(BoolObstacleSerializer obstacle in
					level.GetComponentsInChildren<BoolObstacleSerializer> ()
					.OrderBy(gameObject => gameObject.name )){

				obstacle.SetID (id);
				id++;
			}

			foreach(PushBlockSerializer obstacle in
				level.GetComponentsInChildren<PushBlockSerializer> ()
				.OrderBy(gameObject => gameObject.name )){

				// TODO: implement ID's with push block serialisers

				// obstacle.SetID (id);
				id++;
			}

		}

		static void InitializePlayers(GameObject localPlayer, GameObject remotePlayer, UpdateManager updateManager){

			// enable the controller components

			localPlayer.GetComponent<LocalPlayerController> ().enabled = true;
			remotePlayer.GetComponent<RemotePhysicsController> ().enabled = true;


			// enable the serialisers! and link them to update manager

			PlayerSerializer localSerializer = localPlayer.GetComponent<PlayerSerializer> ();

			localSerializer.enabled = true;
			localSerializer.updateManager = updateManager;


			PlayerSerializer remoteSerializer = remotePlayer.GetComponent<PlayerSerializer> ();

			remoteSerializer.enabled = true;
			updateManager.Subscribe(remoteSerializer, UpdateManager.Channel.PLAYER);

		}

		static void InitializeObstaclesHost(GameObject level, UpdateManager updateManager){

			// on the host side, keys, key blocks and pressure playes should
			// all respond to collisions, not remote updates!

			foreach (KeyController key in level.GetComponentsInChildren<KeyController>()) {

				key.enabled = true;
			}


			foreach (PressurePlateController plate in level.GetComponentsInChildren<PressurePlateController>()) {

				plate.enabled = true;
			}


			foreach (KeyBlockController block in level.GetComponentsInChildren<KeyBlockController>()) {

				block.enabled = true;
			}


			// all of these obstacles need to be set up to notify the 
			// update manager when something changes!

			foreach (BoolObstacleSerializer obstacle in level.GetComponentsInChildren<BoolObstacleSerializer>()) {

				obstacle.enabled = true;
				
				obstacle.updateManager = updateManager;
			}



			// as for push blocks, we'll need to enable their
			// normal controllers to, so that they can be pushed around
			// and we'll also set them up to 

			foreach (PushBlockController pbc in
				level.GetComponentsInChildren<PushBlockController>()){

				pbc.enabled = true;

				PushBlockSerializer pbs = pbc.GetComponent<PushBlockSerializer> ();
				pbs.enabled = true;
				pbs.updateManager = updateManager;

			}

		}

		static void InitializeObstaclesClient(GameObject level, UpdateManager updateManager){

			// on the client side, there's no need to control keys and stuff via
			// collisions! they should just respond to network updates through their
			// serializer

			foreach (BoolObstacleSerializer obstacle in level.GetComponentsInChildren<BoolObstacleSerializer>()) {

				obstacle.enabled = true;

				updateManager.Subscribe(obstacle, UpdateManager.Channel.OBSTACLE);
			}


			// push blocks are also meant to subscribe to notifications from the
			// network, and should be remote controllable

			foreach (PushBlockController pbc in level.GetComponentsInChildren<PushBlockController>()){

				RemotePhysicsController rpc = pbc.GetComponent<RemotePhysicsController> ();
				rpc.enabled = true;

				PushBlockSerializer pbs = pbc.GetComponent<PushBlockSerializer> ();
				pbs.enabled = true;

				updateManager.Subscribe(pbs, UpdateManager.Channel.PUSHBLOCK);

			}
		}
	}
}
