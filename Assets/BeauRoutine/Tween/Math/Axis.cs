/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Axis.cs
 * Purpose: Bitflag enum for a 3D axis or combination thereof.
*/

using System;

namespace BeauRoutine
{
    /// <summary>
    /// The axis on which transformation tweens
    /// will be applied.
    /// </summary>
    [Flags]
    public enum Axis : byte
    {
        None    = 0x00,

        X		= 0x01,
        Y		= 0x02,
        Z		= 0x04,
        W       = 0x08,

        XY		= X | Y,
        XZ		= X | Z,
        XW      = X | W,
        YZ		= Y | Z,
        YW      = Y | W,
        ZW      = Z | W,

        XYZ		= X | Y | Z,
        XYW     = X | Y | W,
        XZW     = X | Z | W,
        YZW     = Y | Z | W,

        XYZW    = X | Y | Z | W
    }
}
