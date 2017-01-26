/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    LayoutProperty.cs
 * Purpose: Enum determining which properties can be 
 *          tweened on a LayoutElement.
*/

namespace BeauRoutine
{
    /// <summary>
    /// Which property should be tweened on a LayoutElement.
    /// </summary>
    public enum LayoutProperty : byte
    {
        MinWidth,
        MinHeight,
        PreferredWidth,
        PreferredHeight,
        FlexWidth,
        FlexHeight
    }
}
