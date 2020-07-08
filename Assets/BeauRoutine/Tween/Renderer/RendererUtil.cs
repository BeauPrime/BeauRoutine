/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    26 Nov 2018
 * 
 * File:    RendererUtil.cs
 * Purpose: Extension methods for dealing with Renderers and Canvas objects.
 */

using UnityEngine;
using UnityEngine.UI;

namespace BeauRoutine
{
    /// <summary>
    /// Contains helper functions for dealing with renderers.
    /// Provides a common interface for setting alpha and color independently.
    /// </summary>
    public static class RendererUtil
    {
        #region SpriteRenderer

        static public float GetAlpha(this SpriteRenderer inRenderer)
        {
            return inRenderer.color.a;
        }

        static public void SetAlpha(this SpriteRenderer inRenderer, float inAlpha)
        {
            Color src = inRenderer.color;
            src.a = inAlpha;
            inRenderer.color = src;
        }

        static public Color GetColor(this SpriteRenderer inRenderer)
        {
            return inRenderer.color;
        }

        static public void SetColor(this SpriteRenderer inRenderer, Color inColor, ColorUpdate inUpdateMode = ColorUpdate.PreserveAlpha)
        {
            Color src = inColor;
            if (inUpdateMode == ColorUpdate.PreserveAlpha)
                src.a = inRenderer.color.a;
            inRenderer.color = src;
        }

        #endregion // SpriteRenderer

        #region TextMesh

        static public float GetAlpha(this TextMesh inRenderer)
        {
            return inRenderer.color.a;
        }

        static public void SetAlpha(this TextMesh inRenderer, float inAlpha)
        {
            Color src = inRenderer.color;
            src.a = inAlpha;
            inRenderer.color = src;
        }

        static public Color GetColor(this TextMesh inRenderer)
        {
            return inRenderer.color;
        }

        static public void SetColor(this TextMesh inRenderer, Color inColor, ColorUpdate inUpdateMode = ColorUpdate.PreserveAlpha)
        {
            Color src = inColor;
            if (inUpdateMode == ColorUpdate.PreserveAlpha)
                src.a = inRenderer.color.a;
            inRenderer.color = src;
        }

        #endregion // TextMesh

        #region Graphic

        static public float GetAlpha(this Graphic inRenderer)
        {
            return inRenderer.color.a;
        }

        static public void SetAlpha(this Graphic inRenderer, float inAlpha)
        {
            Color src = inRenderer.color;
            src.a = inAlpha;
            inRenderer.color = src;
        }

        static public Color GetColor(this Graphic inRenderer)
        {
            return inRenderer.color;
        }

        static public void SetColor(this Graphic inRenderer, Color inColor, ColorUpdate inUpdateMode = ColorUpdate.PreserveAlpha)
        {
            Color src = inColor;
            if (inUpdateMode == ColorUpdate.PreserveAlpha)
                src.a = inRenderer.color.a;
            inRenderer.color = src;
        }

        #endregion // Graphic
    }
}