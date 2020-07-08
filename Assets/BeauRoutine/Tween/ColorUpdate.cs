/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    ColorUpdate.cs
 * Purpose: Enum for how color tweens should function.
 *          Useful if combining an alpha tween with a color tween.
*/

namespace BeauRoutine
{
    /// <summary>
    /// How a color should be tweened.
    /// </summary>
    public enum ColorUpdate : byte
    {
        /// <summary>
        /// Alpha will be tweened.
        /// </summary>
        FullColor,

        /// <summary>
        /// The alpha of the initial value will be preserved.
        /// In the case of renderers, the alpha of the renderer
        /// during the tween will be preserved to allow Color
        /// and Alpha tweens to function concurrently.
        /// </summary>
        PreserveAlpha
    }
}
