/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    3 Apr 2017
 * 
 * File:    TableOperation.cs
 * Purpose: Collection of possible operations to perform on running fibers.
*/

namespace BeauRoutine.Internal
{
    internal interface ITableOperation
    {
        void Execute(Fiber inFiber);
    }

    internal struct NullOperation : ITableOperation
    {
        public void Execute(Fiber inFiber) { }
    }

    internal struct PauseOperation : ITableOperation
    {
        public void Execute(Fiber inFiber)
        {
            inFiber.Pause();
        }
    }

    internal struct ResumeOperation : ITableOperation
    {
        public void Execute(Fiber inFiber)
        {
            inFiber.Resume();
        }
    }

    internal struct StopOperation : ITableOperation
    {
        public void Execute(Fiber inFiber)
        {
            if (inFiber.Manager.IsUpdating())
                inFiber.Stop();
            else
                inFiber.Dispose();
        }
    }
}
