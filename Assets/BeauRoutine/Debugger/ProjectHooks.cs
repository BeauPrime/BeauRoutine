/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    10 April 2018
 * 
 * File:    ProjectHooks.cs
 * Purpose: Contains project-level settings and hooks.
*/

#if UNITY_EDITOR

using System;
using BeauRoutine.Internal;
using UnityEditor;

namespace BeauRoutine.Editor
{
    static internal class ProjectHooks
    {
        [InitializeOnLoadMethod]
        static private void ApplyScriptExecutionOrders()
        {
            MonoScript[] allScripts = MonoImporter.GetAllRuntimeMonoScripts();
            ApplyScriptExecutionOrder(allScripts, typeof(RoutineUnityHost), 20000);
            ApplyScriptExecutionOrder(allScripts, typeof(RoutineBootstrap), -20000);
        }

        static private void ApplyScriptExecutionOrder(MonoScript[] inAllScripts, Type inType, int inOrder)
        {
            for (int i = 0; i < inAllScripts.Length; ++i)
            {
                MonoScript script = inAllScripts[i];
                Type type = script.GetClass();
                if (type != null && type == inType)
                {
                    if (MonoImporter.GetExecutionOrder(script) != inOrder)
                        MonoImporter.SetExecutionOrder(script, inOrder);
                    return;
                }
            }
        }
    }
}

#endif // UNITY_EDITOR