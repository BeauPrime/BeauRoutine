/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    EulerStorage.cs
 * Purpose: Internal storage for maintaining consistent Euler values
 *          when interpolating Euler angles.
*/

using System.Collections.Generic;
using UnityEngine;

namespace BeauRoutine.Internal
{
	// There are multiple ways to represent one Quaternion rotation
	// in Euler angles.  An euler of (180, 0, 0) could also be (0, 180, 180).
	// Because of this, we can't rely on the value of a transform's euler angles
	// at any given frame to be consistent with what it was previously.
	// That has the potential to mess up single-axis rotations.
	// This provides a storage space for the last known euler angles
	// to prevent any axis oddities.

	internal sealed class EulerStorage
	{
		/// <summary>
		/// Gets the euler angles for the given space.
		/// </summary>
		public Vector3 Get(Space inSpace)
		{
			return (inSpace == Space.Self ? m_Local : m_World);
		}

        /// <summary>
        /// Copies the euler angles for the given space into the given vector.
        /// </summary>
        public void Get(ref Vector3 ioVector, Space inSpace)
        {
            if (inSpace == Space.Self)
                ioVector.Set(m_Local.x, m_Local.y, m_Local.z);
            else
                ioVector.Set(m_World.x, m_World.y, m_World.z);
        }

		/// <summary>
		/// Sets the euler angles for the given space.
		/// </summary>
		public void Set(Space inSpace, Vector3 inEuler)
		{
			if (inSpace == Space.Self)
			{
				m_Local = inEuler;
			}
			else
			{
				m_World = inEuler;
			}
		}

		private Vector3 m_Local;
		private Vector3 m_World;
		private int m_RefCount;

		private EulerStorage(Transform inTransform)
		{
			m_Local = inTransform.localEulerAngles;
			m_World = inTransform.eulerAngles;
			m_RefCount = 0;
		}

		private void AddRef()
		{
			++m_RefCount;
		}

		private bool RemoveRef()
		{
			return --m_RefCount > 0;
		}

		#region Static Access

		static private Dictionary<int, EulerStorage> s_Records = new Dictionary<int, EulerStorage>();

		/// <summary>
		/// Finds or creates the EulerStorage for the given transform.
		/// </summary>
		static public EulerStorage AddTransform(Transform inTransform)
		{
			EulerStorage record;
			int id = inTransform.GetInstanceID();

			if (!s_Records.TryGetValue(id, out record))
			{
				record = new EulerStorage(inTransform);
				s_Records.Add(id, record);
			}

			record.AddRef();
			return record;
		}

		/// <summary>
		/// Removes a reference to the EulerStorage for the given transform.
		/// </summary>
		static public void RemoveTransform(Transform inTransform)
		{
			RemoveTransform(inTransform.GetInstanceID());
		}

		/// <summary>
		/// Removes a reference to the EulerStorage for the transform that has the given instanceID.
		/// Used for removing references to objects that have been destroyed.
		/// </summary>
		static public void RemoveTransform(int inTransformID)
		{
			EulerStorage record;
			if (s_Records.TryGetValue(inTransformID, out record) && !record.RemoveRef())
			{
				s_Records.Remove(inTransformID);
			}
		}

		/// <summary>
		/// Returns the EulerStorage associated with the given transform.
		/// </summary>
		static public EulerStorage GetRecord(Transform inTransform)
		{
			EulerStorage record;
			int id = inTransform.GetInstanceID();
			s_Records.TryGetValue(id, out record);
			return record;
		}

		#endregion
	}
}
