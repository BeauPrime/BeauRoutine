/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    11 May 2018
 * 
 * File:    SplineLerpSpace.cs
 * Purpose: Enum describing how interpolation percentages
            should be applied to a spline.
*/

namespace BeauRoutine.Splines
{
    /// <summary>
    /// How a spline should be interpolated.
    /// </summary>
    public enum SplineLerp : byte
    {
        // Each segment takes an amount of time
        // proportional to the number of vertices
        Vertex,

        // Each segment takes an amount of time
        // proportional to the direct distance between vertices
        Direct,

        // Each segments takes an amount of time
        // proportional to the precise distance between vertices
        Precise
    }
}
