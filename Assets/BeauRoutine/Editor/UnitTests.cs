/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    UnitTests.cs
 * Purpose: Any unit tests go here.
*/

#if UNITY_5_3 || UNITY_5_3_OR_NEWER

using NUnit.Framework;
using System;

namespace BeauRoutine.Editor
{
	static public class UnitTests
	{
		[Test(Description="Ensures curves contain the endpoints [0, 0] and [1, 1]")]
		static public void TestCurves()
		{
			Curve[] allCurves = (Curve[])Enum.GetValues(typeof(Curve));
			for (int i = 0; i < allCurves.Length; ++i)
			{
				Curve curve = allCurves[i];

				Assert.AreEqual(0, curve.Evaluate(0), "Zero is not equal for curve " + curve.ToString());
				Assert.AreEqual(1, curve.Evaluate(1), "One is not equal for curve " + curve.ToString());
			}
		}
	}
}

#endif