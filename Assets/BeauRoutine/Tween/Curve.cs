/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Curve.cs
 * Purpose: Set of premade easing functions.
*/

namespace BeauRoutine
{
    /// <summary>
    /// Premade easing functions.
    /// </summary>
    public enum Curve : byte
    {
        Linear = 0,
        Smooth,

        QuadIn,
        QuadOut,
        QuadInOut,

        CubeIn,
        CubeOut,
        CubeInOut,

        QuartIn,
        QuartOut,
        QuartInOut,

        QuintIn,
        QuintOut,
        QuintInOut,

        BackIn,
        BackOut,
        BackInOut,

        SineIn,
        SineOut,
        SineInOut,

        CircleIn,
        CircleOut,
        CircleInOut,

        BounceIn,
        BounceOut,
        BounceInOut,

        ElasticIn,
        ElasticOut,
        ElasticInOut
    }
}
