/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    31 Oct 2017
 * 
 * File:    RoutineDecorator.cs
 * Purpose: Decorator object for modifying routine execution.
*/

using System;
using System.Collections;

namespace BeauRoutine.Internal
{
    [Flags]
    internal enum RoutineDecoratorFlag
    {
        Inline   = 0x001
    }

    internal struct RoutineDecorator : IEnumerator, IDisposable
    {
        public IEnumerator Enumerator;
        public RoutineDecoratorFlag Flags;

        public object Current { get { return Enumerator.Current; } }

        public bool MoveNext()
        {
            return Enumerator.MoveNext();
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            if (Enumerator != null)
            {
                ((IDisposable)Enumerator).Dispose();
                Enumerator = null;
            }
        }

        public override string ToString()
        {
            if (Enumerator == null)
                return "null";

            return Enumerator.ToString() + " [" + Flags.ToString() + "]";
        }
    }
}
