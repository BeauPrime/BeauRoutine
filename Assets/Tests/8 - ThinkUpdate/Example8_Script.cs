using System;
using System.Collections;
using System.Collections.Generic;
using BeauRoutine;
using BeauRoutine.Splines;
using UnityEngine;
using UnityEngine.UI;

namespace BeauRoutine.Examples
{
    public class Example8_Script : MonoBehaviour
    {
        public MultiSpline SplineArgs;
        public SplineTweenSettings SplineTween;

        public AnimationCurve curve;

        private Routine m_Path;

        private IEnumerator Start()
        {
            yield return null;
            yield return null;
            // Routine.Start(this, Executing("ThinkUpdate: ")).SetPhase(RoutinePhase.ThinkUpdate);
            // Routine.Start(this, Executing("CustomUpdate: ")).SetPhase(RoutinePhase.CustomUpdate);

            // PerSecondRoutine.Start(this, Executing("PerSecond: "), 5);
            // var f = Routine.Start(this, Routine.PerSecond(Executing("PerSecond: "), 5));
            //Routine.Start(this, Routine.PerSecond(started, 5f));

            // Stack<RoutineLock> locks = new Stack<RoutineLock>(8);
            // Action loopedInput = () =>
            // {
            //     if (Input.GetKeyDown(KeyCode.L))
            //         locks.Push(f.GetLock());
            //     else if (Input.GetKeyDown(KeyCode.U) && locks.Count > 0)
            //         locks.Pop().Release();
            // };

            // Routine.StartLoop(this, loopedInput);
            //     }

            //     t.SetRotation(new Vector3(0, y, z), Axis.YZ, s);
            // };

            // Routine.Start(this, Tween.Spline(
            //     SplineArgs, OnSplineUpdate, 8f, SplineTween
            // ).Loop().Randomize()).SetPhase(RoutinePhase.FixedUpdate);

            // for (int i = 0; i < SplineArgs.GetVertexCount(); ++i)
            // {
            //     SplineArgs.SetVertexUserData(i, new Color(Random.value, Random.value, Random.value));
            // }
        }

        private void OnSplineUpdate(SplineUpdateInfo inInfo)
        {
            Vector3 diff = inInfo.Point - transform.position;
            transform.position = inInfo.Point;

            float speed = diff.magnitude / Routine.DeltaTime;
            Debug.Log("Speed: " + speed.ToString());

            SplineSegment seg = inInfo.GetSegment();
            GetComponent<SpriteRenderer>().color =
                Color.Lerp((Color)inInfo.Spline.GetVertexUserData(seg.VertexA), (Color)inInfo.Spline.GetVertexUserData(seg.VertexB), seg.Interpolation);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Camera c = Camera.main;
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = transform.position.z - c.transform.position.z;
                Vector3 worldMouse = c.ScreenToWorldPoint(mousePos);

                var spline = Spline.Simple(transform.position, worldMouse, 0.5f, new Vector3(0, 8, 0));
                m_Path.Replace(this, transform.MoveAlong(spline, .65f, Axis.XY));
            }
        }

        private IEnumerator SquashStretch()
        {
            yield return transform.SquashStretchTo(1.2f, 2f, Axis.Y, Axis.X).YoyoLoop().Wave(Wave.Function.CosFade, 5f).Randomize();
        }
    }
}