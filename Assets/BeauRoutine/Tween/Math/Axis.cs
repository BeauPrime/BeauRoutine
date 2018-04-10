/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
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

        XY		= X | Y,
        XZ		= X | Z,
        YZ		= Y | Z,

        XYZ		= X | Y | Z
    }
}
