/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    3 Apr 2017
 * 
 * File:    TableOperation.cs
 * Purpose: Collection of possible operations to perform on running fibers.
*/

namespace BeauRoutine.Internal
{
    public interface ITableOperation
    {
        void Execute(Fiber inFiber);
    }

    public struct NullOperation : ITableOperation
    {
        public void Execute(Fiber inFiber) { }
    }

    public struct PauseOperation : ITableOperation
    {
        public void Execute(Fiber inFiber)
        {
            inFiber.Pause();
        }
    }

    public struct ResumeOperation : ITableOperation
    {
        public void Execute(Fiber inFiber)
        {
            inFiber.Resume();
        }
    }

    public struct StopOperation : ITableOperation
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
