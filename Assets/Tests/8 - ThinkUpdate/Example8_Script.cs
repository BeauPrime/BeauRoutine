using System;
using System.Collections;
using System.Collections.Generic;
using BeauRoutine;
using UnityEngine;
using UnityEngine.UI;

namespace BeauRoutine.Examples
{
    public class Example8_Script : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return null;
            // Routine.Start(this, Executing("ThinkUpdate: ")).SetPhase(RoutinePhase.ThinkUpdate);
            // Routine.Start(this, Executing("CustomUpdate: ")).SetPhase(RoutinePhase.CustomUpdate);

            // PerSecondRoutine.Start(this, Executing("PerSecond: "), 5);
            var f = Routine.Start(this, Routine.PerSecond(Executing("PerSecond: "), 5));
            //Routine.Start(this, Routine.PerSecond(started, 5f));

            Stack<RoutineLock> locks = new Stack<RoutineLock>(8);
            Action loopedInput = () =>
            {
                if (Input.GetKeyDown(KeyCode.L))
                    locks.Push(f.GetLock());
                else if (Input.GetKeyDown(KeyCode.U) && locks.Count > 0)
                    locks.Pop().Release();
            };

            Routine.StartLoop(this, loopedInput);
        }

        private IEnumerator Executing(string inPrefix)
        {
            while (true)
            {
                yield return null;
                Debug.Log(inPrefix + Routine.DeltaTime);
                //yield return Random.value;

                //if (Random.value < 0.1f)
                //    break;
            }
        }
    }
}