﻿using System;
using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;

namespace xyz._8bITProject.cooperace.multiplayer.tests
{
	[TestFixture]
	public class PlayerSerializerTest
	{
		private readonly float POSX1 = 34.351f;
		private readonly float POSY1 = 91.984f;
		private readonly float POSX2 = 74285.13f;
		private readonly float POSY2 = -143570108.57f;

		[Test]
		public void SerializeDeserialize () {
			PlayerSerializer serializer = new PlayerSerializer ();

			// Serialize some information
			PlayerInformation info = PlayerInformation1 ();
			List<byte> data = serializer.Serialize (info);

			// Make sure the serializer isn't just spitting out the same thing
			Assert.That(!info.Equals(data));

			// Deserialize the result
			PlayerInformation resultingInfo = serializer.Deserialize (data);

			// Check the information is the same as the original
			Assert.That(resultingInfo.Equals(info));
		}

		[Test]
		public void DeserializeSerialize () {
			PlayerSerializer serializer = new PlayerSerializer ();

			PlayerInformation info;
			List<byte> data = data2 ();
			List<byte> resultingData;

			// Deserialize a List
			info = serializer.Deserialize(data);

			// Make sure we didn't just get the same thing back
			Assert.That(!info.Equals(data));

			// Serialize the info
			resultingData = serializer.Serialize(info);

			// Check to see the new data is the same as the original
			for (int i=0; i<data.Count; i++) {
				Assert.That(resultingData [i] == data [i]);
			}
		}

		[Test]
		public void Serialize () {
			PlayerSerializer serializer = new PlayerSerializer ();

			PlayerInformation info = PlayerInformation2 ();
			List<byte> data = serializer.Serialize (info);
			List<byte> expectedData = data2 ();

			for (int i=0; i<data.Count; i++) {
				Assert.That(data [i] == expectedData [i]);
			}
		}

		[Test]
		public void Deserialize () {
			PlayerSerializer serializer = new PlayerSerializer ();

			List<byte> data = data1 ();
			PlayerInformation info = serializer.Deserialize (data);
			PlayerInformation expectedInfo = PlayerInformation1 ();
			Assert.That (info.Equals (expectedInfo));
		}

		[Test]
		public void DeserializeEmptyList () {
			PlayerSerializer serializer = new PlayerSerializer ();

			// try deserialize an empty list
			try {
				serializer.Deserialize (new List<byte> ());
			}
			catch (System.ArgumentOutOfRangeException e) {
				// Good! We can't deserialize that!
				Assert.Pass ();
			}
			// Oh no! Deserialize shouldn't be able to deseralize an empty list...
		}

		private PlayerInformation PlayerInformation1 () {
			return new PlayerInformation (POSX1, POSY1);
		}

		private PlayerInformation PlayerInformation2 () {
			return new PlayerInformation (POSX2, POSY2);
		}

		private List<byte> data1 () {
			List<byte> data =  new List<byte> ();
			// add each individual byte into the list
			data.Add(FloatGetByte(POSX1, 0));
			data.Add(FloatGetByte(POSX1, 1));
			data.Add(FloatGetByte(POSX1, 2));
			data.Add(FloatGetByte(POSX1, 3));
			data.Add(FloatGetByte(POSY1, 0));
			data.Add(FloatGetByte(POSY1, 1));
			data.Add(FloatGetByte(POSY1, 2));
			data.Add(FloatGetByte(POSY1, 3));
			return data;
		}

		private List<byte> data2 () {
			List<byte> data = new List<byte> ();
			// add each individual byte into the list
			data.Add(FloatGetByte(POSX2, 0));
			data.Add(FloatGetByte(POSX2, 1));
			data.Add(FloatGetByte(POSX2, 2));
			data.Add(FloatGetByte(POSX2, 3));
			data.Add(FloatGetByte(POSY2, 0));
			data.Add(FloatGetByte(POSY2, 1));
			data.Add(FloatGetByte(POSY2, 2));
			data.Add(FloatGetByte(POSY2, 3));
			return data;
		}

		// returns the nth byte in the float
		private byte FloatGetByte (float x, int n) {
			byte[] bytes = BitConverter.GetBytes (x);
			return bytes [n];
		}
	}
}
