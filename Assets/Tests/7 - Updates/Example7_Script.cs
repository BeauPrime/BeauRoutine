using System.Collections;
using BeauRoutine;
using UnityEngine;
using UnityEngine.UI;

namespace BeauRoutine.Examples
{
    public class Example7_Script : MonoBehaviour
    {
        [SerializeField]
        private Transform m_PhysicsUpdater = null;

        [SerializeField]
        private Transform m_PhysicsUpdater2 = null;

        private Routine m_MasterUpdate;
        private Routine m_Update1;
        private Routine m_Update2;

        private enum TestEnum : byte { A, B, C };

        private IEnumerator Start()
        {
            Routine.Initialize();
            yield return null;

            m_MasterUpdate = Routine.Start(this, CheckInputs()).SetPhase(RoutinePhase.Update);
            yield return null;

            m_Update1 = Routine.Start(this, UpdateOnePhysics(m_PhysicsUpdater)).SetPhase(RoutinePhase.FixedUpdate).SetTimeScale(1).DelayBy(5);
            m_Update2 = Routine.Start(this, UpdateOnePhysics(m_PhysicsUpdater2)).SetPhase(RoutinePhase.LateUpdate).OnException((e) => { Debug.Log("We caught an exception!"); });

            m_Update1.TryManuallyUpdate(5);
        }

        private IEnumerator CheckInputs()
        {
            TweenUtil.SetDefaultLerpPeriodByFramerate(60);

            while(true)
            {
                Vector3 screenPoint = Input.mousePosition;
                screenPoint.z = 10;
                Vector3 mousePoint = Camera.main.ScreenToWorldPoint(screenPoint);
                transform.position = Vector3.LerpUnclamped(transform.position, mousePoint, TweenUtil.Lerp(0.1f));

                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    CycleRoutineUpdate(ref m_Update1, true);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    CycleRoutineUpdate(ref m_Update2, true);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    CycleRoutineUpdate(ref m_MasterUpdate, false);
                }

                if (Input.GetKey(KeyCode.Space))
                {
                    m_Update1.TryManuallyUpdate(Routine.DeltaTime * 2);
                    m_Update2.TryManuallyUpdate(Routine.DeltaTime * 2);
                }
                yield return Routine.WaitForUpdate();
            }
        }

        private void CycleRoutineUpdate(ref Routine ioRoutine, bool inbAllowManual)
        {
            switch(ioRoutine.GetPhase())
            {
                case RoutinePhase.FixedUpdate:
                    ioRoutine.SetPhase(RoutinePhase.LateUpdate);
                    break;

                case RoutinePhase.LateUpdate:
                    ioRoutine.SetPhase(RoutinePhase.Update);
                    break;

                case RoutinePhase.Update:
                    ioRoutine.SetPhase(inbAllowManual ? RoutinePhase.Manual : RoutinePhase.FixedUpdate);
                    if (inbAllowManual)
                    {
                        Routine.ManualUpdate(Routine.DeltaTime / 2);
                    }
                    break;

                case RoutinePhase.Manual:
                    ioRoutine.Stop();
                    ioRoutine.SetPhase(RoutinePhase.FixedUpdate);
                    break;
            }
        }

        private IEnumerator UpdateOnePhysics(Transform inTransform)
        {
            while (true)
            {
                yield return inTransform.MoveTo(inTransform.localPosition.x + 5, 2f, Axis.X, Space.Self).Wave(Wave.Function.Sin, 1).Loop(2);
                yield return Routine.Combine(
                    Routine.Combine(
                        Routine.Yield(CombinedThing()),
                        Routine.Yield(Routine.WaitForEndOfFrame())    
                    ),
                    Routine.Yield(Routine.WaitForEndOfFrame())
                );
                
                yield return 2;
            }
        }

        private IEnumerator CombinedThing()
        {
            yield return Routine.WaitForFixedUpdate();
            Debug.Log(Routine.DeltaTime);
        }

        private IEnumerator ExecuteForItem(int inItem)
        {
            Debug.Log("Checking item: " + inItem);
            yield return inItem / 2f;
            Debug.Log("Checked item: " + inItem);
        }
    }
}