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
        public bool forceSingleThread = false;

        private AsyncHandle handle;

        private void Start()
        {
            Routine.Settings.ForceSingleThreaded = forceSingleThread;
            handle = Async.For(0, int.MaxValue, LogTimestamp, AsyncPriority.Normal);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                handle.Cancel();
            }
        }

        private void LogTimestamp(int i)
        {
            TimeSpan currentTime = DateTime.UtcNow.TimeOfDay;
            Debug.Log(string.Format("[{0}] {1}", currentTime.TotalMilliseconds, i));
        }
    }
}