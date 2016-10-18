﻿/*
 * Level select menu logic.
 *
 * Athir Saleem <isaleem@student.unimelb.edu.au>
 *
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace xyz._8bITProject.cooperace.ui {
	public class LevelSelectMenuController : MonoBehaviour {
		#if UNITY_EDITOR
		static bool editor = true;
		#else
		static bool editor = false;
		#endif

		// ui elements
		public Text levelNameText;
		public GameObject messageText;

		// the currently displayed level
		public static int currentLevelIndex_ = 0;
		public int currentLevelIndex {
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

				UpdateLevelDetails();
			}
		}

		void OnEnable() {
			// make sure something is loaded when visible
			UpdateLevelDetails();
			HideMessage();
		}

		// display the level details
		void UpdateLevelDetails() {
			levelNameText.text = Maps.maps[currentLevelIndex];
		}

		// methods to manipulate the message
		void DisplayMessage(string msg) {
			messageText.GetComponent<Text>().text = msg;
			messageText.SetActive(true);
		}
		void HideMessage() {
			messageText.SetActive(false);
		}

		// public methods to switch the currently displayed level
		public void SwitchToNextLevel() {
			currentLevelIndex += 1;
		}
		public void SwitchToPrevLevel() {
			currentLevelIndex -= 1;
		}

		// public method to handle play button behaviour
		public void PlayButtonHandler() {
			if (editor) UIHelper.GoTo("Game Scene");
			else UIHelper.GoTo("MatchMakingScreen");

		}

		// public method to handle back button behaviour
		public void BackButtonHandler() {
			UIStateMachine.instance.GoTo(UIState.MainMenu);
		}

	}
}