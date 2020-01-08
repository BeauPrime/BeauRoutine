using System;
using System.Collections;
using System.Threading;
using BeauRoutine;
using UnityEngine;
using UnityEngine.UI;

namespace BeauRoutine.Examples
{
    public class Example9_Script : MonoBehaviour
    {
        private AsyncHandle handle;

        private void Start()
        {
            var handle1 = Async.For(0, int.MaxValue, LogTimestamp, AsyncFlags.LowPriority | AsyncFlags.MainThreadOnly);
            var handle2 = Async.For(int.MinValue, 0, LogTimestamp, AsyncFlags.HighPriority);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                handle.Cancel();
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                Routine.Settings.ForceSingleThreaded = !Routine.Settings.ForceSingleThreaded;
            }
        }

        private void LogTimestamp(int i)
        {
            if ((i % 100000) == 0)
            {
                TimeSpan currentTime = DateTime.UtcNow.TimeOfDay;
                Debug.Log(string.Format("Counter [{0}] {1}", i, Thread.CurrentThread.Name));
            }
        }
    }
}