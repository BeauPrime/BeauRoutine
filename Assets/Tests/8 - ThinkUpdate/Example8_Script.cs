using System.Collections;
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
            Routine.Start(this, Executing("ThinkUpdate: ")).SetPhase(RoutinePhase.ThinkUpdate);
            Routine.Start(this, Executing("CustomUpdate: ")).SetPhase(RoutinePhase.CustomUpdate);
        }

        private IEnumerator Executing(string inPrefix)
        {
            while (true)
            {
                yield return null;
                Debug.Log(inPrefix + Time.frameCount);
                yield return Routine.WaitForCustomUpdate();
                Debug.Log(inPrefix + "(custom) " + Time.frameCount);
                yield return Routine.WaitForThinkUpdate();
                Debug.Log(inPrefix + "(think) " + Time.frameCount);
            }
        }
    }
}