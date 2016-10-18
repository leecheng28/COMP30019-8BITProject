﻿/*
 * Logic for the leaderboards menu.
 *
 * Athir Saleem <isaleem@student.unimelb.edu.au>
 *
 */

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace xyz._8bITProject.cooperace.leaderboard {
	public class LeaderboardsMenu : MonoBehaviour {

		// ui elements
		public Text levelNameText;
		public GameObject scoresList;
		public GameObject messageText;
		LeaderboardItem[] items;

		// api
		Leaderboards lb = new Leaderboards();

		// values to change ui to on update
		Score[] scoresToDisplay;
		string messageToDisplay;

		// the currently displayed level
		int currentLevelIndex_ = 0;
		int currentLevelIndex {
			get {
				return currentLevelIndex_;
			}
			set {
				// wraps around the list of maps
				if (value >= Maps.maps.Length) {
					value = 0;
				}
				if (value < 0) {
					value = Maps.maps.Length - 1;
				}
				currentLevelIndex_ = value;

				levelNameText.text = currentLevelName;
			}
		}
		string currentLevelName {
			get {
				return Maps.maps[currentLevelIndex];
			}
		}

		void Start() {
			items = scoresList.GetComponentsInChildren<LeaderboardItem>();

			// initially display the first map
			currentLevelIndex = 0;
			LoadLevelStats();
        }

		void Update() {
			// update scores if necessary
			if (scoresToDisplay != null) {
				for (int i = 0; i < items.Length; i++) {
					LeaderboardItem item = items[i];
					item.player1 = scoresToDisplay[i].player1;
					item.player2 = scoresToDisplay[i].player2;
					item.score = scoresToDisplay[i].time.ToString();
				}
				scoresToDisplay = null;

				// make sure score list is currently displayed
				scoresList.SetActive(true);
				messageText.SetActive(false);
			}

			// update message if necessary
			if (messageToDisplay != null) {
				messageText.GetComponent<Text>().text = messageToDisplay;
				messageToDisplay = null;

				// make sure message is currently displayed
				scoresList.SetActive(false);
				messageText.SetActive(true);
			}
		}

		// fetches the actual leaderboard scores
		void LoadLevelStats() {
			DisplayMessage("Loading scores for " + currentLevelName);
			lb.RequestScoresAsync(currentLevelName,
				new Action<ScoresResponse, ServerException>(OnServerResponse));
		}
		// callback for when server responds
		void OnServerResponse(ScoresResponse scores, ServerException error) {
			// make sure this response isn't out of date
			if (scores.level == currentLevelName) {
				if (error != null) {
					DisplayMessage("Unable to contact server, please check your connection.");
				} else if (scores.leaders != null) {
					DisplayNames(scores.leaders);
				}
			}
		}

		// methods to mark the ui to change
		// necessary as unity doesn't allow game objects to change from threads
		void DisplayMessage(string message) {
			messageToDisplay = message;
		}
		void DisplayNames(Score[] scores) {
			scoresToDisplay = scores;
		}

		// public methods to switch the currently displayed score
		public void SwitchToNextLevel() {
			currentLevelIndex += 1;
			LoadLevelStats();
		}
		public void SwitchToPrevLevel() {
			currentLevelIndex -= 1;
			LoadLevelStats();
		}

	}
}