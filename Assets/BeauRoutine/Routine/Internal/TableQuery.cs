/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    3 Apr 2017
 * 
 * File:    TableQuery.cs
 * Purpose: Collection of possible queries to perform on running fibers.
*/

using UnityEngine;

namespace BeauRoutine.Internal
{
    public interface ITableQuery
    {
        bool Validate(Fiber inFiber);
    }

    public struct NullQuery : ITableQuery
    {
        public bool Validate(Fiber inFiber) { return true; }
    }

    public struct NameQuery : ITableQuery
    {
        public string Name;

        public bool Validate(Fiber inFiber)
        {
            return inFiber.HasName(Name);
        }
    }

    public struct GroupQuery : ITableQuery
    {
        public int GroupMask;

        public bool Validate(Fiber inFiber)
        {
            return inFiber.HasGroups(GroupMask);
        }
    }

    public struct MonoBehaviourQuery : ITableQuery
    {
        public MonoBehaviour Host;

        public bool Validate(Fiber inFiber)
        {
            return inFiber.HasHost(Host);
        }
    }

    public struct MonoBehaviourNameQuery : ITableQuery
    {
        public MonoBehaviour Host;
        public string Name;

        public bool Validate(Fiber inFiber)
        {
            return inFiber.HasHost(Host) && inFiber.HasName(Name);
        }
    }

    public struct MonoBehaviourGroupQuery : ITableQuery
    {
        public MonoBehaviour Host;
        public int GroupMask;

        public bool Validate(Fiber inFiber)
        {
            return inFiber.HasHost(Host) && inFiber.HasGroups(GroupMask);
        }
    }

    public struct GameObjectQuery : ITableQuery
    {
        public GameObject Host;

        public bool Validate(Fiber inFiber)
        {
            return inFiber.HasHost(Host);
        }
    }

    public struct GameObjectNameQuery : ITableQuery
    {
        public GameObject Host;
        public string Name;

        public bool Validate(Fiber inFiber)
        {
            return inFiber.HasHost(Host) && inFiber.HasName(Name);
        }
    }

    public struct GameObjectGroupQuery : ITableQuery
    {
        public GameObject Host;
        public int GroupMask;

        public bool Validate(Fiber inFiber)
        {
            return inFiber.HasHost(Host) && inFiber.HasGroups(GroupMask);
        }
    }
}
