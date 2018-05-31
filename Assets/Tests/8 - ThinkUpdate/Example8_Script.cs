using System.Collections;
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
            // Routine.Start(this, Routine.PerSecond(Executing("PerSecond: "), 5));
            //Routine.Start(this, Routine.PerSecond(started, 5f));

            // Routine.Start(this, SquashStretch());

            // Routine.Start(this, Camera.main.BackgroundColorTo(Color.black, 0.5f).YoyoLoop());

            // transform.SetPosition(transform.position + Random.onUnitSphere * 10);

            // SplineArgs.Process();

            SplineTween.Orient = SplineOrientation.Custom;
            SplineTween.OrientCallback = (t, r, s) =>
            {
                Debug.Log("Direction: " + r);
                float y = 0;
                float z = Mathf.Atan2(r.y, r.x) * Mathf.Rad2Deg;
                if (z < 0)
                    z += 360f;

                if (z > 90 && z < 270)
                {
                    y = -180;
                    z = 180 - z;
                }

                t.SetRotation(new Vector3(0, y, z), Axis.YZ, s);
            };

            Routine.Start(this,
                transform.MoveAlongWithSpeed(SplineArgs, 8, Axis.XYZ, Space.World, SplineTween).Loop().Randomize()
            );

            Debug.Log(JsonUtility.ToJson(SplineArgs));
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