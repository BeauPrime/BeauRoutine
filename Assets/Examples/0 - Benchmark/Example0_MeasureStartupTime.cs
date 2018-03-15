using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;

#if UNITY_5_5_OR_NEWER
using Profiler = UnityEngine.Profiling.Profiler;
#elif !UNITY_5_0
using Profiler = UnityEngine.Profiler;
#endif

#if !UNITY_5_0 && !UNITY_5_5_OR_NEWER
#define REQUIRE_MAX_SAMPLE_ADJUST
#endif

namespace BeauRoutine.Examples
{
    public class Example0_MeasureStartupTime : MonoBehaviour
    {
        private readonly int[] ROUTINE_COUNTS = new int[] { 1, 17, 2000, 5000, 10000, 16000, 64000 };
        private readonly int SAMPLE_COUNT = 4;

        private IEnumerator Start()
        {
            yield return null;

#if REQUIRE_MAX_SAMPLE_ADJUST
            Profiler.maxNumberOfSamplesPerFrame = 8000000;
#endif

            Application.targetFrameRate = -1;

            Routine.Initialize();
            Routine.Settings.DebugMode = false;
            Routine.Settings.SnapshotEnabled = false;
            RoutineIdentity.Require(this);

            yield return null;
            yield return null;

            for (int i = 0; i < ROUTINE_COUNTS.Length; ++i)
            {
                float timeAccumulation = 0;
                for (int j = 0; j < SAMPLE_COUNT; ++j)
                {
                    Routine.Settings.SetCapacity(ROUTINE_COUNTS[i]);
                    yield return null;

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
            // float[] startingValues = new float[numFloats];
            // float[] endingValues = new float[numFloats];

            // for (int i = 0; i < numFloats; ++i)
            // {
            //     startingValues[i] = UnityEngine.Random.value;
            //     endingValues[i] = 1 + UnityEngine.Random.value;
            // }

            Action<float> floatSetter = (f) => { };

            // Stopwatch watch = Stopwatch.StartNew();
            for (int i = 0; i < numFloats; ++i)
            {
                Profiler.BeginSample("Tween");
                // Routine.Start(this, Tween.Float(startingValues[i], endingValues[i], floatSetter, 1.0f).Loop());
                Routine.Start(this, Tween.Float(0, 1, floatSetter, 1.0f).Loop());
                Profiler.EndSample();
            }
            // watch.Stop();
            // return (watch.ElapsedTicks / 10000f);
            return 0;
        }
    }
}