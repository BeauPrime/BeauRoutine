/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
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
        static public float GetAxis(ref Vector4 inVector, Axis inAxis)
        {
            if ((inAxis & Axis.X) != 0)
                return inVector.x;
            if ((inAxis & Axis.Y) != 0)
                return inVector.y;
            if ((inAxis & Axis.Z) != 0)
                return inVector.z;
            if ((inAxis & Axis.W) != 0)
                return inVector.w;
            return float.NaN;
        }

        /// <summary>
        /// Returns the value of the vector for the given axis.
        /// </summary>
        static public float GetAxis(this Vector4 inVector, Axis inAxis)
        {
            if ((inAxis & Axis.X) != 0)
                return inVector.x;
            if ((inAxis & Axis.Y) != 0)
                return inVector.y;
            if ((inAxis & Axis.Z) != 0)
                return inVector.z;
            if ((inAxis & Axis.W) != 0)
                return inVector.w;
            return float.NaN;
        }

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
        static public Vector4 CopyFrom(this Vector4 inVector, Vector4 inSource, Axis inAxis = Axis.XYZW)
        {
            if ((inAxis & Axis.XYZW) == Axis.XYZW)
                return inSource;
            if (inAxis == 0)
                return inVector;

            if ((inAxis & Axis.X) != 0)
                inVector.x = inSource.x;
            if ((inAxis & Axis.Y) != 0)
                inVector.y = inSource.y;
            if ((inAxis & Axis.Z) != 0)
                inVector.z = inSource.z;
            if ((inAxis & Axis.W) != 0)
                inVector.w = inSource.w;

            return inVector;
        }

        /// <summary>
        /// Copies the source vector onto the given vector with the given axis values.
        /// </summary>
        static public void CopyFrom(ref Vector4 ioVector, Vector4 inSource, Axis inAxis = Axis.XYZW)
        {
            if ((inAxis & Axis.XYZW) == Axis.XYZW)
            {
                ioVector.Set(inSource.x, inSource.y, inSource.z, inSource.w);
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
            if ((inAxis & Axis.W) != 0)
                ioVector.w = inSource.w;
        }

        /// <summary>
        /// Returns the initial vector with the given axis values
        /// copied from the given source.
        /// </summary>
        static public Vector3 CopyFrom(this Vector3 inVector, Vector3 inSource, Axis inAxis = Axis.XYZ)
        {
            if ((inAxis & Axis.XYZ) == Axis.XYZ)
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
            if ((inAxis & Axis.XYZ) == Axis.XYZ)
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
            if ((inAxis & Axis.XY) == Axis.XY)
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
        static public void CopyFrom(ref Vector2 ioVector, Vector2 inSource, Axis inAxis = Axis.XY)
        {
            if ((inAxis & Axis.XY) == Axis.XY)
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
        static public void Add(ref Vector4 ioVector, Vector4 inAdd)
        {
            ioVector.x += inAdd.x;
            ioVector.y += inAdd.y;
            ioVector.z += inAdd.z;
            ioVector.w += inAdd.w;
        }

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
        static public void Add(ref Vector4 ioVector, Vector4 inAdd, float inCoefficient)
        {
            ioVector.x += inAdd.x * inCoefficient;
            ioVector.y += inAdd.y * inCoefficient;
            ioVector.z += inAdd.z * inCoefficient;
            ioVector.w += inAdd.w * inCoefficient;
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
        static public void Subtract(ref Vector4 ioVector, Vector4 inSub)
        {
            ioVector.x -= inSub.x;
            ioVector.y -= inSub.y;
            ioVector.z -= inSub.z;
            ioVector.w -= inSub.w;
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
        static public void Subtract(ref Vector4 ioVector, Vector4 inSub, float inCoefficient)
        {
            ioVector.x -= inSub.x * inCoefficient;
            ioVector.y -= inSub.y * inCoefficient;
            ioVector.z -= inSub.z * inCoefficient;
            ioVector.w -= inSub.w * inCoefficient;
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
    
        #region Multiply

        /// <summary>
        /// Multiplies the contents of one vector by the other.
        /// </summary>
        static public void Multiply(ref Vector4 ioVector, Vector4 inMult)
        {
            ioVector.x *= inMult.x;
            ioVector.y *= inMult.y;
            ioVector.z *= inMult.z;
            ioVector.w *= inMult.w;
        }

        /// <summary>
        /// Multiplies the contents of one vector by the other.
        /// </summary>
        static public void Multiply(ref Vector3 ioVector, Vector3 inMult)
        {
            ioVector.x *= inMult.x;
            ioVector.y *= inMult.y;
            ioVector.z *= inMult.z;
        }

        /// <summary>
        /// Multiplies the contents of one vector by the other.
        /// </summary>
        static public void Multiply(ref Vector2 ioVector, Vector2 inMult)
        {
            ioVector.x *= inMult.x;
            ioVector.y *= inMult.y;
        }

        /// <summary>
        /// Multiplies the contents of one vector by the other.
        /// </summary>
        static public void Multiply(ref Vector4 ioVector, Vector4 inMult, float inCoefficient)
        {
            ioVector.x *= inMult.x * inCoefficient;
            ioVector.y *= inMult.y * inCoefficient;
            ioVector.z *= inMult.z * inCoefficient;
            ioVector.w *= inMult.w * inCoefficient;
        }

        /// <summary>
        /// Multiplies the contents of one vector by the other.
        /// </summary>
        static public void Multiply(ref Vector3 ioVector, Vector3 inMult, float inCoefficient)
        {
            ioVector.x *= inMult.x * inCoefficient;
            ioVector.y *= inMult.y * inCoefficient;
            ioVector.z *= inMult.z * inCoefficient;
        }

        /// <summary>
        /// Multiplies the contents of one vector by the other.
        /// </summary>
        static public void Multiply(ref Vector2 ioVector, Vector2 inMult, float inCoefficient)
        {
            ioVector.x *= inMult.x * inCoefficient;
            ioVector.y *= inMult.y * inCoefficient;
        }

        /// <summary>
        /// Multiplies the contents of one vector by a coefficient.
        /// </summary>
        static public void Multiply(ref Vector4 ioVector, float inCoefficient)
        {
            ioVector.x *= inCoefficient;
            ioVector.y *= inCoefficient;
            ioVector.z *= inCoefficient;
            ioVector.w *= inCoefficient;
        }

        /// <summary>
        /// Multiplies the contents of one vector by a coefficient.
        /// </summary>
        static public void Multiply(ref Vector3 ioVector, float inCoefficient)
        {
            ioVector.x *= inCoefficient;
            ioVector.y *= inCoefficient;
            ioVector.z *= inCoefficient;
        }

        /// <summary>
        /// Multiplies the contents of one vector by a coefficient.
        /// </summary>
        static public void Multiply(ref Vector2 ioVector, float inCoefficient)
        {
            ioVector.x *= inCoefficient;
            ioVector.y *= inCoefficient;
        }

        /// <summary>
        /// Divides the contents of one vector by the other.
        /// </summary>
        static public void Divide(ref Vector4 ioVector, Vector4 inDiv)
        {
            ioVector.x /= inDiv.x;
            ioVector.y /= inDiv.y;
            ioVector.z /= inDiv.z;
            ioVector.w /= inDiv.w;
        }

        /// <summary>
        /// Divides the contents of one vector by the other.
        /// </summary>
        static public void Divide(ref Vector3 ioVector, Vector3 inDiv)
        {
            ioVector.x /= inDiv.x;
            ioVector.y /= inDiv.y;
            ioVector.z /= inDiv.z;
        }

        /// <summary>
        /// Divides the contents of one vector by the other.
        /// </summary>
        static public void Divide(ref Vector2 ioVector, Vector2 inDiv)
        {
            ioVector.x /= inDiv.x;
            ioVector.y /= inDiv.y;
        }

        /// <summary>
        /// Divides the contents of one vector by the other.
        /// </summary>
        static public void Divide(ref Vector4 ioVector, Vector4 inDiv, float inCoefficient)
        {
            ioVector.x /= inDiv.x * inCoefficient;
            ioVector.y /= inDiv.y * inCoefficient;
            ioVector.z /= inDiv.z * inCoefficient;
            ioVector.w /= inDiv.w * inCoefficient;
        }

        /// <summary>
        /// Divides the contents of one vector by the other.
        /// </summary>
        static public void Divide(ref Vector3 ioVector, Vector3 inDiv, float inCoefficient)
        {
            ioVector.x /= inDiv.x * inCoefficient;
            ioVector.y /= inDiv.y * inCoefficient;
            ioVector.z /= inDiv.z * inCoefficient;
        }

        /// <summary>
        /// Divides the contents of one vector by the other
        /// </summary>
        static public void Divide(ref Vector2 ioVector, Vector2 inDiv, float inCoefficient)
        {
            ioVector.x /= inDiv.x * inCoefficient;
            ioVector.y /= inDiv.y * inCoefficient;
        }

        /// <summary>
        /// Divides the contents of one vector by a divisor.
        /// </summary>
        static public void Divide(ref Vector4 ioVector, float inDivisor)
        {
            ioVector.x /= inDivisor;
            ioVector.y /= inDivisor;
            ioVector.z /= inDivisor;
            ioVector.w /= inDivisor;
        }

        /// <summary>
        /// Divides the contents of one vector by a divisor.
        /// </summary>
        static public void Divide(ref Vector3 ioVector, float inDivisor)
        {
            ioVector.x /= inDivisor;
            ioVector.y /= inDivisor;
            ioVector.z /= inDivisor;
        }

        /// <summary>
        /// Divides the contents of one vector by a divisor.
        /// </summary>
        static public void Divide(ref Vector2 ioVector, float inDivisor)
        {
            ioVector.x /= inDivisor;
            ioVector.y /= inDivisor;
        }

        #endregion
    }
}
