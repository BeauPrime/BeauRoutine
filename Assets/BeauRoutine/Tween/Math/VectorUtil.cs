/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    VectorUtil.cs
 * Purpose: Extension methods for dealing with Vectors.
*/

using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Contains helper functions for dealing with vectors.
    /// </summary>
    public static class VectorUtil
    {
        #region Axis

        /// <summary>
        /// Returns the value of the vector for the given axis.
        /// </summary>
        static public float GetAxis(ref Vector3 inVector, Axis inAxis)
        {
            if ((inAxis & Axis.X) != 0)
                return inVector.x;
            if ((inAxis & Axis.Y) != 0)
                return inVector.y;
            if ((inAxis & Axis.Z) != 0)
                return inVector.z;
            return float.NaN;
        }

        /// <summary>
        /// Returns the value of the vector for the given axis.
        /// </summary>
        static public float GetAxis(this Vector3 inVector, Axis inAxis)
        {
            if ((inAxis & Axis.X) != 0)
                return inVector.x;
            if ((inAxis & Axis.Y) != 0)
                return inVector.y;
            if ((inAxis & Axis.Z) != 0)
                return inVector.z;
            return float.NaN;
        }

        /// <summary>
        /// Returns the value of the vector for the given axis.
        /// </summary>
        static public float GetAxis(ref Vector2 inVector, Axis inAxis)
        {
            if ((inAxis & Axis.X) != 0)
                return inVector.x;
            if ((inAxis & Axis.Y) != 0)
                return inVector.y;
            return float.NaN;
        }

        /// <summary>
        /// Returns the value of the vector for the given axis.
        /// </summary>
        static public float GetAxis(this Vector2 inVector, Axis inAxis)
        {
            if ((inAxis & Axis.X) != 0)
                return inVector.x;
            if ((inAxis & Axis.Y) != 0)
                return inVector.y;
            return float.NaN;
        }

        #endregion

        #region CopyFrom

        /// <summary>
        /// Returns the initial vector with the given axis values
        /// copied from the given source.
        /// </summary>
        static public Vector3 CopyFrom(this Vector3 inVector, Vector3 inSource, Axis inAxis = Axis.XYZ)
        {
            if (inAxis == Axis.XYZ)
                return inSource;
            if (inAxis == 0)
                return inVector;

            if ((inAxis & Axis.X) != 0)
                inVector.x = inSource.x;
            if ((inAxis & Axis.Y) != 0)
                inVector.y = inSource.y;
            if ((inAxis & Axis.Z) != 0)
                inVector.z = inSource.z;

            return inVector;
        }

        /// <summary>
        /// Copies the source vector onto the given vector with the given axis values.
        /// </summary>
        static public void CopyFrom(ref Vector3 ioVector, Vector3 inSource, Axis inAxis = Axis.XYZ)
        {
            if (inAxis == Axis.XYZ)
            {
                ioVector.Set(inSource.x, inSource.y, inSource.z);
                return;
            }
            if (inAxis == 0)
                return;

            if ((inAxis & Axis.X) != 0)
                ioVector.x = inSource.x;
            if ((inAxis & Axis.Y) != 0)
                ioVector.y = inSource.y;
            if ((inAxis & Axis.Z) != 0)
                ioVector.z = inSource.z;
        }

        /// <summary>
        /// Returns the initial vector with the given axis values
        /// copied from the given source.
        /// </summary>
        static public Vector2 CopyFrom(this Vector2 inVector, Vector2 inSource, Axis inAxis = Axis.XY)
        {
            if (inAxis == Axis.XY || inAxis == Axis.XYZ)
                return inSource;
            if (inAxis == 0)
                return inVector;

            if ((inAxis & Axis.X) != 0)
                inVector.x = inSource.x;
            if ((inAxis & Axis.Y) != 0)
                inVector.y = inSource.y;

            return inVector;
        }

        /// <summary>
        /// Copies the source vector onto the given vector with the given axis values.
        /// </summary>
        static public void CopyFrom(ref Vector2 ioVector, Vector2 inSource, Axis inAxis = Axis.XYZ)
        {
            if (inAxis == Axis.XY || inAxis == Axis.XYZ)
            {
                ioVector.Set(inSource.x, inSource.y);
                return;
            }
            if (inAxis == 0)
                return;

            if ((inAxis & Axis.X) != 0)
                ioVector.x = inSource.x;
            if ((inAxis & Axis.Y) != 0)
                ioVector.y = inSource.y;
        }

        #endregion

        #region Add

        /// <summary>
        /// Adds the contents of one vector to the other.
        /// </summary>
        static public void Add(ref Vector3 ioVector, Vector3 inAdd)
        {
            ioVector.x += inAdd.x;
            ioVector.y += inAdd.y;
            ioVector.z += inAdd.z;
        }

        /// <summary>
        /// Adds the contents of one vector to the other.
        /// </summary>
        static public void Add(ref Vector2 ioVector, Vector2 inAdd)
        {
            ioVector.x += inAdd.x;
            ioVector.y += inAdd.y;
        }

        /// <summary>
        /// Adds the contents of one vector to the other.
        /// </summary>
        static public void Add(ref Vector3 ioVector, Vector3 inAdd, float inCoefficient)
        {
            ioVector.x += inAdd.x * inCoefficient;
            ioVector.y += inAdd.y * inCoefficient;
            ioVector.z += inAdd.z * inCoefficient;
        }

        /// <summary>
        /// Adds the contents of one vector to the other.
        /// </summary>
        static public void Add(ref Vector2 ioVector, Vector2 inAdd, float inCoefficient)
        {
            ioVector.x += inAdd.x * inCoefficient;
            ioVector.y += inAdd.y * inCoefficient;
        }

        /// <summary>
        /// Subtracts the contents of one vector from the other.
        /// </summary>
        static public void Subtract(ref Vector3 ioVector, Vector3 inSub)
        {
            ioVector.x -= inSub.x;
            ioVector.y -= inSub.y;
            ioVector.z -= inSub.z;
        }

        /// <summary>
        /// Subtracts the contents of one vector from the other.
        /// </summary>
        static public void Subtract(ref Vector2 ioVector, Vector2 inSub)
        {
            ioVector.x -= inSub.x;
            ioVector.y -= inSub.y;
        }

        /// <summary>
        /// Subtracts the contents of one vector from the other.
        /// </summary>
        static public void Subtract(ref Vector3 ioVector, Vector3 inSub, float inCoefficient)
        {
            ioVector.x -= inSub.x * inCoefficient;
            ioVector.y -= inSub.y * inCoefficient;
            ioVector.z -= inSub.z * inCoefficient;
        }

        /// <summary>
        /// Subtracts the contents of one vector from the other.
        /// </summary>
        static public void Subtract(ref Vector2 ioVector, Vector2 inSub, float inCoefficient)
        {
            ioVector.x -= inSub.x * inCoefficient;
            ioVector.y -= inSub.y * inCoefficient;
        }

        #endregion
    }
}
