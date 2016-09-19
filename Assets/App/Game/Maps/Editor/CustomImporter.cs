﻿/*
 * Tiled map importer logic.
 *
 * Athir Saleem <isaleem@student.unimelb.edu.au>
 *
 */

using UnityEngine;
using Tiled2Unity;
using System.Collections.Generic;

namespace xyz._8bITProject.cooperace {
	[CustomTiledImporter]
	class CustomImporter : ICustomTiledImporter {

		// offset to adjust tiled object position for unity objects
		// specifically top-right to center
		Vector3 offset = new Vector3(0.5f, -0.5f, 0);

		// prefabs for initializing game objects
		GameObject playerPrefab = (GameObject)Resources.Load("Player");
		GameObject keyPrefab = (GameObject)Resources.Load("Key");
		GameObject keyBlockPrefab = (GameObject)Resources.Load("KeyBlock");
		GameObject pressurePlatePrefab = (GameObject)Resources.Load("PressurePlate");
		GameObject pressurePlateBlockPrefab = (GameObject)Resources.Load("PressurePlateBlock");
		GameObject pushBlockPrefab = (GameObject)Resources.Load("PushBlock");
		GameObject finishLinePrefab = (GameObject)Resources.Load("FinishLine");
		GameObject exitPrefab = (GameObject)Resources.Load("Exit");

		// Replaces the given marker game object (generated by tiled2unity)
		// with a new instance of the given prefab at the same location.
		// (Note: actually deleting game objects at this point is not possible
		//  due to how tiled2unity works, so the marker will be deleted later)
		GameObject replaceMarker(GameObject marker, GameObject prefab) {
			// create a new instance of the prefab and add it to the tiled map
			// object
			GameObject newObject = GameObject.Instantiate(prefab);
			newObject.transform.parent = marker.transform.parent.transform;

			newObject.transform.position = offset + marker.transform.position;

			// mark the object created by tiled2unity for deletion
			marker.name = "__to_be_destroyed__";

			return newObject;
		}

		// Automatically called by tiled2unity during import for every object
		// in the tiled map with properties.
		// Handles creating the appropriate prefab instance for each tiled
		// object.
		public void HandleCustomProperties(GameObject marker,
				IDictionary<string, string> props) {
			// the tiled map should have objects with specific keys
			// which are then replaced by instances of the prefab
			if (props.ContainsKey("Player")) {
				replaceMarker(marker, playerPrefab);
			} else if (props.ContainsKey("Key")) {
				replaceMarker(marker, keyPrefab);
			} else if (props.ContainsKey("KeyBlock")) {
				replaceMarker(marker, keyBlockPrefab);
			} else if (props.ContainsKey("PressurePlate")) {
				GameObject plate = replaceMarker(marker, pressurePlatePrefab);
				plate.GetComponent<PressurePlate>().SetAddress(props["PressurePlate"]);
			} else if (props.ContainsKey("PressurePlateBlock")) {
				GameObject block = replaceMarker(marker, pressurePlateBlockPrefab);
				block.GetComponent<PressurePlateBlock>().SetAddress(props["PressurePlateBlock"]);
			} else if (props.ContainsKey("PushBlock")) {
				replaceMarker(marker, pushBlockPrefab);
			} else if (props.ContainsKey("FinishLine")) {
				replaceMarker(marker, finishLinePrefab);
			}
		}

		// Automatically called by tiled2unity after the map is completly
		// imported.
		public void CustomizePrefab(GameObject mapPrefab) {
			// delete all objects marked for deletion
			foreach (Transform transform in mapPrefab.GetComponentsInChildren<Transform>()) {
				if (transform.name == "__to_be_destroyed__") {
					GameObject.DestroyImmediate(transform.gameObject);	
				}
			}

			// initialize exit colliders all around the map

			BuildExits (mapPrefab);

			// setup links between pressure plates and blocks
			// first group all plates and blocks by address
			// (what I wouldn't do for haskell right now...)

			// pressure plate groups
			PressurePlate[] allPlates =
				mapPrefab.GetComponentsInChildren<PressurePlate>();
			Dictionary<string, List<PressurePlate>> groupedPlates =
				groupByAddress<PressurePlate>(allPlates);

			// pressure plate block groups
			PressurePlateBlock[] allBlocks =
				mapPrefab.GetComponentsInChildren<PressurePlateBlock>();
			Dictionary<string, List<PressurePlateBlock>> groupedBlocks =
				groupByAddress<PressurePlateBlock>(allBlocks);

			// give the appropriate list to each plate and block
			foreach (PressurePlate plate in allPlates) {
				plate.linkedBlocks = groupedBlocks[plate.GetAddress()];
			}
			foreach (PressurePlateBlock block in allBlocks) {
				block.linkedPlates = groupedPlates[block.GetAddress()];
			}
		}

		// Helper method to group a list of game objects by its address field.
		// Returns a dictionary mapping the address to lists of objects with
		// that address.
		Dictionary<string, List<T>> groupByAddress<T>(T[] allObjects) where T : IAddressLinkedObject {
			Dictionary<string, List<T>> groups = new Dictionary<string, List<T>>();

			foreach (T obj in allObjects) {
				List<T> list;
				// See if there is already a list with the appropriate address.
				if (groups.ContainsKey(obj.GetAddress())) {
					// Yes, so use that list.
					list = groups[obj.GetAddress()];
				} else {
					// Nope, so make a new one and add to the dictionary.
					list = new List<T>(allObjects.Length);
					groups.Add(obj.GetAddress(), list);
				}

				// Add the game object to the appropriate list.
				list.Add(obj);
			}
			
			return groups;
		}


		// Helper method to create exits colliders around a level
		void BuildExits(GameObject mapPrefab){

			TiledMap map = mapPrefab.GetComponent<TiledMap>();

			float width = map.NumTilesWide;
			float height = map.NumTilesHigh;
			float padding = 1;		// space between level boundary and portal start
			float thickness = 2;	// width of exit portals

			// create a parent object for all four exit portals
			GameObject exits = new GameObject ("Exits");
			exits.transform.parent = mapPrefab.transform;

			// create the actual exits around the level
			NewExit(exits, "left",
				0 - padding - thickness/2, -height/2, thickness, height + 2 * padding + 2 * thickness);

			NewExit(exits, "right",
				width + padding + thickness/2, -height/2, thickness, height + 2 * padding + 2 * thickness);

			NewExit(exits, "above",
				width/2, 0 + padding + thickness/2, width + 2 * padding + 2 * thickness, thickness);

			NewExit(exits, "below",
				width/2, -height - padding - thickness/2, width + 2 * padding + 2 * thickness, thickness);

		}

		// Helper method to create a single exit portal at a given position
		void NewExit(GameObject parent, string name, float posX, float posY, float scaleX, float scaleY) {

			GameObject exit = GameObject.Instantiate (exitPrefab);

			exit.name = name;
			exit.transform.parent = parent.transform;
			exit.transform.position = new Vector3(posX, posY, 0);
			exit.transform.localScale = new Vector3(scaleX, scaleY, 0);
		}

	}
}
