using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using BeauRoutine;
using UnityEngine;
using UnityEngine.UI;

namespace BeauRoutine.Examples
{
    public class Example9_Script : MonoBehaviour
    {
        private PerformanceMeasurement p1;
        private PerformanceMeasurement p2;

        private void Start()
        {
            p1 = new PerformanceMeasurement();
            p2 = new PerformanceMeasurement();

            p1.Iterate(0, int.MaxValue, AsyncFlags.LowPriority);
            // p2.Iterate(int.MinValue, 0, AsyncFlags.HighPriority);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Routine.Settings.ForceSingleThreaded = !Routine.Settings.ForceSingleThreaded;
            }
        }

        private class PerformanceMeasurement
        {
            private const int Measurement = 1000;

            private long lastTimestamp;
            private AsyncHandle handle;
            private bool first;

            public void Iterate(int inFrom, int inTo, AsyncFlags inFlags)
            {
                handle.Cancel();
                handle = Async.For(inFrom, inTo, LogTimestamp, inFlags);
                first = true;
            }

            private void LogTimestamp(int i)
            {
                if ((i % Measurement) == 0)
                {
                    long ts = Stopwatch.GetTimestamp();
                    if (first)
                    {
                        lastTimestamp = ts;
                        first = false;
                    }
                    else
                    {
                        double avgTicksPerSec = TimeSpan.TicksPerMillisecond * Measurement / (double) ((ts - lastTimestamp));
                        UnityEngine.Debug.Log(string.Format("Counter [{0}] {1}/ms", i, avgTicksPerSec));
                        lastTimestamp = ts;
                    }
                }
            }
        }
    }
}