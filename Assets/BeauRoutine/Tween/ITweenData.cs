/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    ITweenData.cs
 * Purpose: Interface for data / interpolation logic to be used
 *          inside a Tween.
*/

namespace BeauRoutine
{
    /// <summary>
    /// Contains tweening values and how to apply them.
    /// </summary>
    public interface ITweenData
    {
        void OnTweenStart();
        void ApplyTween(float inPercent);
        void OnTweenEnd();
    }
}
