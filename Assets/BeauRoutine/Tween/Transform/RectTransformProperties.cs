/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    RectTransformProperties.cs
 * Purpose: Bitflag enum for the different properties of a RectTransform.
 *          Used to selectively update properties in a RectTransformState.
*/

using System;

namespace BeauRoutine
{
    /// <summary>
    /// Properties of a RectTransform.
    /// </summary>
    [Flags]
    public enum RectTransformProperties : uint
    {
        None        = 0x000,

        AnchoredPositionX   = 0x001,
        AnchoredPositionY   = 0x002,
        AnchoredPositionZ   = 0x004,

        AnchorMinX   = 0x008,
        AnchorMinY   = 0x010,
        AnchorMaxX   = 0x020,
        AnchorMaxY   = 0x040,

        SizeDeltaX  = 0x080,
        SizeDeltaY  = 0x100,

        PivotX      = 0x200,
        PivotY      = 0x400,

        ScaleX      = 0x800,
        ScaleY      = 0x1000,
        ScaleZ      = 0x2000,

        RotationX   = 0x4000,
        RotationY   = 0x8000,
        RotationZ   = 0x10000,

        AnchoredPosition    = AnchoredPositionX | AnchoredPositionY | AnchoredPositionZ,
        Anchors    = AnchorMinX | AnchorMinY | AnchorMaxX | AnchorMaxY,
        SizeDelta = SizeDeltaX | SizeDeltaY,
        Pivot = PivotX | PivotY,

        Scale       = ScaleX | ScaleY | ScaleZ,
        Rotation    = RotationX | RotationY | RotationZ,

        All         = AnchoredPosition | Anchors | SizeDelta | Pivot | Scale | Rotation
    }
}
