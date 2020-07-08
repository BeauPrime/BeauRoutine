/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    3 Apr 2017
 * 
 * File:    TableQuery.cs
 * Purpose: Collection of possible queries to perform on running fibers.
*/

using UnityEngine;

namespace BeauRoutine.Internal
{
    internal interface ITableQuery
    {
        bool Validate(Fiber inFiber);
    }

    internal struct NullQuery : ITableQuery
    {
        public bool Validate(Fiber inFiber) { return true; }
    }

    internal struct NameQuery : ITableQuery
    {
        public string Name;

        public bool Validate(Fiber inFiber)
        {
            return inFiber.HasName(Name);
        }
    }

    internal struct GroupQuery : ITableQuery
    {
        public int GroupMask;

        public bool Validate(Fiber inFiber)
        {
            return inFiber.HasGroups(GroupMask);
        }
    }

    internal struct MonoBehaviourQuery : ITableQuery
    {
        public MonoBehaviour Host;

        public bool Validate(Fiber inFiber)
        {
            return inFiber.HasHost(Host);
        }
    }

    internal struct MonoBehaviourNameQuery : ITableQuery
    {
        public MonoBehaviour Host;
        public string Name;

        public bool Validate(Fiber inFiber)
        {
            return inFiber.HasHost(Host) && inFiber.HasName(Name);
        }
    }

    internal struct MonoBehaviourGroupQuery : ITableQuery
    {
        public MonoBehaviour Host;
        public int GroupMask;

        public bool Validate(Fiber inFiber)
        {
            return inFiber.HasHost(Host) && inFiber.HasGroups(GroupMask);
        }
    }

    internal struct GameObjectQuery : ITableQuery
    {
        public GameObject Host;

        public bool Validate(Fiber inFiber)
        {
            return inFiber.HasHost(Host);
        }
    }

    internal struct GameObjectNameQuery : ITableQuery
    {
        public GameObject Host;
        public string Name;

        public bool Validate(Fiber inFiber)
        {
            return inFiber.HasHost(Host) && inFiber.HasName(Name);
        }
    }

    internal struct GameObjectGroupQuery : ITableQuery
    {
        public GameObject Host;
        public int GroupMask;

        public bool Validate(Fiber inFiber)
        {
            return inFiber.HasHost(Host) && inFiber.HasGroups(GroupMask);
        }
    }
}
