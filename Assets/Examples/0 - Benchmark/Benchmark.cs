using UnityEngine;
using System.Collections;
using System;
using BeauRoutine;
using System.Diagnostics;

public class Benchmark : MonoBehaviour
{
    private readonly int[] ROUTINE_COUNTS = new int[] { 2000, 5000, 16000, 64000 };
    private readonly int SAMPLE_COUNT = 10;

    private IEnumerator Start()
    {
        yield return null;

        Profiler.maxNumberOfSamplesPerFrame = 8000000;

        Application.targetFrameRate = -1;

        Routine.Initialize();
        RoutineIdentity.Require(this);

        for(int i = 0; i < ROUTINE_COUNTS.Length; ++i)
        {
            float timeAccumulation = 0;
            for (int j = 0; j < SAMPLE_COUNT; ++j)
            {
                timeAccumulation += MeasureStartupTime(ROUTINE_COUNTS[i]);
                yield return new WaitForSeconds(1.0f);
                Routine.StopAll();
                yield return null;
            }
            float avgTime = timeAccumulation / SAMPLE_COUNT;
            UnityEngine.Debug.Log("[" + ROUTINE_COUNTS[i].ToString() + ", startup]: " + avgTime.ToString() + "ms");
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

        Routine.SetCapacity(numFloats);
        Routine.SetSnapshotEnabled(false);

        Stopwatch watch = Stopwatch.StartNew();
        for (int i = 0; i < numFloats; ++i)
        {
            Routine.Start(this, Tween.Float(startingValues[i], endingValues[i], floatSetter, 1.0f).Loop());
        }
        watch.Stop();
        return (watch.ElapsedTicks / 10000f);
    }
}
