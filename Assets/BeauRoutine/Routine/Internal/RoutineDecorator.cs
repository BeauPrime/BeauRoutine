/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
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
        Inline = 0x001,
        ContinueIfNull = 0x002,
    }

    internal class RoutineDecorator : IEnumerator, IDisposable
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
            DisposeUtils.DisposeEnumerator(ref Enumerator);
        }

        public override string ToString()
        {
            if (Enumerator == null)
                return "null";

            return Enumerator.ToString() + " [" + Flags.ToString() + "]";
        }
    }
}