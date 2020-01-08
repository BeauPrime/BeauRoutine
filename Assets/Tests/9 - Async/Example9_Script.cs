using System.Collections;
using System.Threading;
using BeauRoutine;
using UnityEngine;
using UnityEngine.UI;

namespace BeauRoutine.Examples
{
    public class Example9_Script : MonoBehaviour
    {
        private void Start()
        {
            Async.Schedule(ThreadedThing, Internal.AsyncPriority.Normal);
        }

        private void ThreadedThing()
        {
            int i = 0;
            while (i++ < 1000)
            {
                Debug.Log(i);
                Thread.Sleep(100 + i * 2);
            }
        }
    }
}