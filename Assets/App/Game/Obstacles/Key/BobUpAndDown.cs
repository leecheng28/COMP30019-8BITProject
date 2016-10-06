﻿/*
 * Key bobbing animation logic.
 *
 * Athir Saleem <isaleem@student.unimelb.edu.au>
 * 
 * Modified to use a sine curve
 * 
 * Matthew Farrugia-Roberts <farrugiam@student.unimelb.edu.au>
 *
 */

using UnityEngine;
using System.Collections;

namespace xyz._8bITProject.cooperace {
	
	public class BobUpAndDown : MonoBehaviour {

		/// How many seconds for a full cycle?
		public float period = 2;
		/// How far above/below the origin do we go?
		public float amplitude = 0.2f;

		/// Keep track of when we are in the cycle
		private float t = 0;

		/// the sprite to bob up and down
		SpriteRenderer sprite;
		/// where does this sprite's transform begin?
		Vector3 origin;

		void Start() {

			// link up components

			sprite = GetComponentInChildren<SpriteRenderer> ();

			// store the initial position

			origin = sprite.transform.position;
		}

		void Update() {

			// what's our new phase?

			t += Time.deltaTime;
			while (t > period) { t -= period; }

			// calculate the relevant offset using Sin

			float offset = amplitude * Mathf.Sin (2 * Mathf.PI * t / period);

			// apply that offset

			sprite.transform.position = origin + Vector3.up * offset;
		}

	}
}
