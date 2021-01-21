/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Jan 2021
 * 
 * File:    IFuture.cs
 * Purpose: Common future interface.
*/

using System;
using System.Collections;
using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Interface for a Future value.
    /// </summary>
    public interface IFuture : IDisposable
    {
        bool IsDone();

        // Progress
        bool IsInProgress();
        float GetProgress();
        void SetProgress(float inProgress);
        IFuture OnProgress(Action<float> inProgressCallback);

        // Completion
        bool IsComplete();

        // Failure
        bool IsFailed();
        Future.Failure GetFailure();
        bool TryGetFailure(out Future.Failure outFailure);
        void Fail();
        void Fail(object inArg);
        void Fail(Exception inException);
        void Fail(Future.Failure inFailure);
        void Fail(Future.FailureType inType);
        void Fail(Future.FailureType inType, object inArg);
        IFuture OnFail(Action inFailureCallback);
        IFuture OnFail(Action<Future.Failure> inFailureCallback);

        // Cancellation
        bool IsCancelled();
        void Cancel();

        // Coroutines
        IFuture LinkTo(Routine inRoutine);
        IEnumerator Wait();
    }
}