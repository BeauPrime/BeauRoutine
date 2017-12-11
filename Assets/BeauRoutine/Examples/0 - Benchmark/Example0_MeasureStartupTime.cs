using UnityEngine;
using System.Collections;
using System;
using BeauRoutine;
using System.Diagnostics;

namespace BeauRoutine.Examples
{
    public class Example0_MeasureStartupTime : MonoBehaviour
    {
        private readonly int[] ROUTINE_COUNTS = new int[] { 2000, 5000, 10000, 16000, 64000 };
        private readonly int SAMPLE_COUNT = 4;

        private IEnumerator Start()
        {
            yield return null;

#if UNITY_5_5
        UnityEngine.Profiling.Profiler.maxNumberOfSamplesPerFrame = 8000000;
#elif !UNITY_5_5_OR_NEWER && !UNITY_5_0
        UnityEngine.Profiler.maxNumberOfSamplesPerFrame = 8000000;
#endif

            Application.targetFrameRate = -1;

            Routine.Initialize();
            RoutineIdentity.Require(this);

            yield return null;

            for (int i = 0; i < ROUTINE_COUNTS.Length; ++i)
            {
                float timeAccumulation = 0;
                for (int j = 0; j < SAMPLE_COUNT; ++j)
                {
                    timeAccumulation += MeasureStartupTime(ROUTINE_COUNTS[i]);
                    yield return new WaitForSeconds(5f);
                    Routine.StopAll();
                    yield return null;
                }
                float avgTime = timeAccumulation / SAMPLE_COUNT;
                print("[" + ROUTINE_COUNTS[i].ToString() + ", startup]: " + avgTime.ToString() + "ms");
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        private float MeasureStartupTime(int numFloats)
        {
            float[] startingValues = new float[numFloats];
            float[] endingValues = new float[numFloats];

            for (int i = 0; i < numFloats; ++i)
            {
                startingValues[i] = UnityEngine.Random.value;
                endingValues[i] = 1 + UnityEngine.Random.value;
            }

            Action<float> floatSetter = (f) => { };

            Routine.Settings.SetCapacity(numFloats);
            Routine.Settings.SnapshotEnabled = false;

            Stopwatch watch = Stopwatch.StartNew();
            for (int i = 0; i < numFloats; ++i)
            {
                Routine.Start(this, Tween.Float(startingValues[i], endingValues[i], floatSetter, 1.0f).Loop());
            }
            watch.Stop();
            return (watch.ElapsedTicks / 10000f);
        }
    }
}