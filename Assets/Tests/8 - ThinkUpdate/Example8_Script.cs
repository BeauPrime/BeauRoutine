using System.Collections;
using BeauRoutine;
using BeauRoutine.Splines;
using UnityEngine;
using UnityEngine.UI;

namespace BeauRoutine.Examples
{
    public class Example8_Script : MonoBehaviour
    {
        public VertexSpline Spline;

        private IEnumerator Start()
        {
            yield return null;
            // Routine.Start(this, Executing("ThinkUpdate: ")).SetPhase(RoutinePhase.ThinkUpdate);
            // Routine.Start(this, Executing("CustomUpdate: ")).SetPhase(RoutinePhase.CustomUpdate);

            // PerSecondRoutine.Start(this, Executing("PerSecond: "), 5);
            // Routine.Start(this, Routine.PerSecond(Executing("PerSecond: "), 5));
            //Routine.Start(this, Routine.PerSecond(started, 5f));

            Routine.Start(this, SquashStretch());

            // Routine.Start(this, Camera.main.BackgroundColorTo(Color.black, 0.5f).YoyoLoop());

            transform.SetPosition(transform.position + Random.onUnitSphere * 10);

            Routine.Start(this, transform.MoveAlong(Spline, 3, Axis.XYZ, Space.Self, SplinePositioning.Absolute).Loop().Randomize());
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

        private IEnumerator SquashStretch()
        {
            yield return transform.SquashStretchTo(1.2f, 2f, Axis.Y, Axis.X).YoyoLoop().Wave(Wave.Function.CosFade, 5f).Randomize();
        }
    }
}