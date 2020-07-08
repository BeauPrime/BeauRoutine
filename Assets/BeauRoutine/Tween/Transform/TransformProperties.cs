/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    TransformProperties.cs
 * Purpose: Bitflag enum for the different properties of a Transform.
 *          Used to selectively update properties in a TransformState.
*/

using System;

namespace BeauRoutine
{
    /// <summary>
    /// Properties of a transform.
    /// </summary>
    [Flags]
    public enum TransformProperties : ushort
    {
        None        = 0x000,

        PositionX   = 0x001,
        PositionY   = 0x002,
        PositionZ   = 0x004,

        ScaleX      = 0x008,
        ScaleY      = 0x010,
        ScaleZ      = 0x020,

        RotationX   = 0x040,
        RotationY   = 0x080,
        RotationZ   = 0x100,

        Position    = PositionX | PositionY | PositionZ,
        Scale       = ScaleX | ScaleY | ScaleZ,
        Rotation    = RotationX | RotationY | RotationZ,

        All         = Position | Scale | Rotation
    }
}
