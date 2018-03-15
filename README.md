# BeauRoutine

**Current Version: 0.9.0**  
Updated 15 March 2018 | [Changelog](https://github.com/FilamentGames/BeauRoutine/blob/master/CHANGELOG.md)

## About
BeauRoutine is a coroutine framework for Unity3D. Intended as a replacement for Unity's existing coroutine implementation, BeauRoutines are a fast, powerful, and flexible way of sequencing your logic. BeauRoutine implements all the features of default Unity coroutines and several advanced features on top, giving you precise control over how and when your coroutines execute.

BeauRoutine also includes a powerful coroutine-driven tweening system. Tweens are highly configurable and easily integrated into a coroutine, allowing you to quickly iterate on programmatic animations and implement visual polish.

**Note**: BeauRoutine is still in development. If you encounter any issues, please either [create an issue in GitHub](https://github.com/FilamentGames/BeauRoutine/issues/new) or [send me an email](mailto:abeauchesne@filamentgames.com).

### Table of Contents
1. [Basic Usage](#basic-usage)
    * [Installing BeauRoutine](#installing-beauroutine)
	* [Running a Routine](#running-a-routine)
	* [Hosting Routines](#hosting-routines)
	* [Expanded Coroutine Syntax](#expanded-coroutine-syntax)
	* [Handles](#handles)
	* [Combine](#combine)
	* [Race](#race)
	* [Sequences](#sequences)
2. [Tweens](#tweens)
    * [Tweens as Coroutines](#tweens-as-coroutines)
	* [Modifying a Tween](#modifying-a-tween)
	* [Value Shortcuts](#value-shortcuts)
	* [Extension Methods](#extension-methods)
	* [Mirrored Tweens](#mirrored-tweens)
3. [Advanced Features](#advanced-features)
	* [Routine Identity](#routine-identity)
	* [Time Scale](#time-scale)
	* [Update Phase](#update-phase)
	* [Priority](#priority)
	* [Groups](#groups)
	* [Inlined Coroutines](#inlined-coroutines)
	* [Futures](#futures)
	* [Custom Tweens](#custom-tweens)
	* [Manual Updates](#manual-updates)
	* [Debugger](#debugger)
4. [Tips and Tricks](#tips-and-tricks)
5. [Technical Notes](#technical-notes)
	* [On Starting BeauRoutines and Update Phases](#on-starting-beauroutines-and-update-phases)
	* [On Stopping BeauRoutines](#on-stopping-beauroutines)
	* [On Reserved Routine Names](#on-reserved-routine-names)
	* [On Delta Time](#on-delta-time)
	* [On Memory Allocation](#on-memory-allocation)
	* [On Important Differences between BeauRoutines and Unity Coroutines](#on-important-differences-between-beauroutines-and-unity-coroutines)
6. [Reference](#reference)
	* [Routine Functions](#routine-functions)
	* [Routine Utilities](#routine-utilities)
	* [Routine Extensions](#routine-extensions)
	* [Global Settings](#global-settings)
	* [Future Functions](#future-functions)
	* [Future Utilities](#future-utilities)
	* [Tween Shortcuts](#tween-shortcuts)
	* [Tween Modifiers](#tween-modifiers)
	* [Utilities](#utilities)
----------------

## Basic Usage

### Installing BeauRoutine

**Note:** BeauRoutine requires Unity version 5.2 or newer.

1. Download the package from the repository: [BeauRoutine.unitypackage](https://github.com/FilamentGames/BeauRoutine/raw/master/BeauRoutine.unitypackage)
2. Unpack into your project.

BeauRoutine uses the ``BeauRoutine`` namespace. You'll need to add the statement ``using BeauRoutine;`` to the top of any scripts using it.

### Running a Routine

To run a BeauRoutine, call
```csharp
Routine.Start( MyCoroutine() );
```
This will begin executing the given coroutine.

To stop a BeauRoutine, call
```csharp
Routine.Stop( "MyCoroutine" );
```

**Note:** This method of stopping a BeauRoutine is maintained to make upgrading from Unity's existing coroutines to BeauRoutine as smooth as possible. It is highly recommended you read the [Handles](#handles) section.

### Hosting Routines

Much like Unity's default Coroutines will only execute while their host object is alive, a BeauRoutine can be given a host to bind its lifetime to that of the host. To bind a host to a BeauRoutine, call
```csharp
Routine.Start( this, MyCoroutine() );
```
This will bind the given object as a host. As long as the host is alive, the BeauRoutine will be able to execute. If the host is deactivated, the BeauRoutine will pause until the host is again active.  Once the host is destroyed, any BeauRoutines it hosted will be halted.

To stop a hosted BeauRoutine, call
```csharp
Routine.Stop( this, "MyCoroutine" );
```

It is highly recommended you host your BeauRoutines in most cases. In the event of a scene change, an unhosted BeauRoutine will keep executing, which may cause exceptions if your coroutine references objects from the previous scene.

**Note:** Unity's built-in coroutines _will not pause_ while their host is inactive. By default, BeauRoutines _will pause_ in the same circumstances. If you are depending on the default Unity behavior, it is highly recommended you read the [Handles](#handles) section to understand how to enable it.

### Expanded Coroutine Syntax

BeauRoutines allow for more flexibility when writing your coroutine logic.

```csharp
public IEnumerator MyCustomBeauRoutine()
{
	DoAThing();
	
	// All four of these statements will wait
	// one second before resuming execution.
	yield return 1;
	yield return 1.0f;
	yield return Routine.WaitSeconds(1.0f);
	yield return new WaitForSeconds(1.0f);
	
	// You can still yield YieldInstructions,
	// WWW objects, and CustomYieldInstructions.
	yield return new WWW(...);
	yield return new WaitForSeconds(2.0f);
	yield return new WaitForEndOfFrame();
	
	// You can also yield commands that will
	// modify the state of the currently executing
	// routine.
	yield return Routine.Command.Pause;
	yield return Routine.Command.Stop;
	
	// You can yield into other coroutines.
	// This will execute the other coroutine
	// and return execution here once it has
	// concluded.
	yield return MyOtherBeauRoutine();
	
	// You can wait for a given function to evaluate to true.
	yield return Routine.WaitCondition(CanProceed);
}

public IEnumerator MyOtherBeauRoutine()
{
	...
}

public bool CanProceed()
{
	...
}
```

It is recommended you use ``Routine.DeltaTime`` in place of any usages of ``Time.deltaTime`` in your coroutines, as the former takes into account BeauRoutine-specific time scaling. See [Time Scale](#time-scale) for more information.

### Handles

Similar to Unity's ``Coroutine`` object, the ``Routine`` object returned after calling many BeauRoutine functions, including ``Routine.Start``, is a handle reference to a BeauRoutine.  You can use that to modify the BeauRoutine as long as it's running. It's also a safe reference - if the BeauRoutine it points at expires, you can still call functions on the handle without fear of either exceptions or unintentionally modifying other active BeauRoutines.

```csharp
// Initialize the Routine object to point at nothing.
Routine myRoutine = Routine.Null;

// myRoutine now points to the running BeauRoutine
myRoutine = Routine.Start( this, MyCoroutine() );

// You can pause, resume, or stop a BeauRoutine with the reference.
myRoutine.Pause();
myRoutine.Resume();
myRoutine.Stop();

// You can call these functions even if the BeauRoutine has expired.
// If it has expired, there will be no effect.
myRoutine = Routine.Start( this, MyCoroutine2() );
myRoutine.Pause(); // Pauses
myRoutine.Stop(); // Stops
myRoutine.Pause(); // No effect

// You can even use a Routine as an IDisposable
using(myRoutine.Replace( this, MyCoroutine() )
{
	...
	// Exiting the using statement will stop the BeauRoutine.
}

// Dispose is an alias of Stop, maintained strictly
// for IDisposable compatibility
myRoutine.Dispose();

// You can test if a BeauRoutine still exists.
if ( myRoutine.Exists() ) { ... }

// It will also implicitly cast to a bool with the same result.
if ( myRoutine ) { ... }

// Routine.Wait will generate a coroutine that waits until
// the coroutine that's being pointed at has expired.
yield return myRoutine.Wait();

// Yielding a Routine directly will have the same effect
// as Routine.Wait
yield return myRoutine;

// You can direct a BeauRoutine to ignore the active state of its host object.
// By calling ExecuteWhileDisabled, your BeauRoutine will continue to execute while
// the host object is disabled.
// This is the default behavior of Unity's built-in coroutines.
// Call this if you depend on that behavior.
myRoutine.ExecuteWhileDisabled();

// You can also restore the default BeauRoutine pausing behavior.
myRoutine.ExecuteWhileEnabled();

// You can set when this BeauRoutine is updated.
myRoutine.SetUpdatePhase(RoutinePhase.FixedUpdate);

// You can delay the routine by a certain number of seconds.
// Note that this is cumulative and affected by time scale.
// See the section on Time Scale for more information.
myRoutine.DelayBy(2f);
```

Routine objects can also be used to start BeauRoutines.

```csharp
Routine myRoutine = Routine.Null;

// You can start a BeauRoutine from the reference
myRoutine.Replace( this, MyCoroutine() );

// Replace will stop the currently running BeauRoutine
// and replace it with another routine.
// This also modifies the contents of the handle,
// so you don't have to re-assign to the variable.
myRoutine.Replace( this, MyCoroutine2() );

// You can replace a Routine with a reference to another
// BeauRoutine if necessary. This will only change the
// reference and will not otherwise modify the running
// BeauRoutine.
Routine myRoutine2;
myRoutine2.Replace( myRoutine );
```

You can also assign a name to a BeauRoutine using a Routine object.

```csharp
// Assign a name with SetName
Routine explicitlyNamed = Routine.Start( MyCoroutine() );
explicitlyNamed.SetName( "aDifferentName" );

// If you don't assign a name, the name of the function
// will be used the first time you request the name.
Routine implicitlyNamed = Routine.Start( MyCoroutine() );

// Write out the names of each function
Debug.Log( explicitlyNamed.GetName() ); // "aDifferentName"
Debug.Log( implicitlyNamed.GetName() ); // "MyCoroutine"

// You can stop BeauRoutines with these names!
Routine.Stop( "MyCoroutine" ); // stops implicitlyNamed
Routine.Stop ("aDifferentName" ); // stops explicitlyNamed
```

### Combine

``Routine.Combine`` is a coroutine that executes multiple coroutines concurrently. This can be incredibly useful for programmatic animations or syncing operations between objects. It concludes when all the given coroutines have either concluded or halted.

```csharp
public IEnumerator MyCombineRoutine()
{
	// This will execute Operation1 and Operation2 concurrently.
	// Both debug messages will be logged.
	yield return Routine.Combine( Operation1(), Operation2() );
}

public IEnumerator Operation1()
{
	yield return 1.0f;
	Debug.Log("Operation 1 has concluded.");
}
public IEnumerator Operation2()
{
	yield return 2.0f;
	Debug.Log("Operation 2 has concluded.");
}
```

### Race

``Routine.Race`` is similar to Combine, but it concludes when one of the given coroutines has concluded or halted. None of the other given coroutines are allowed to execute after one has concluded.

```csharp
public IEnumerator MyCombineRoutine()
{
	// This will only log the Operation1 debug message.
	yield return Routine.Race( Operation1(), Operation2() );
}

public IEnumerator Operation1()
{
	yield return 1.0f;
	Debug.Log("Operation 1 has concluded.");
}
public IEnumerator Operation2()
{
	yield return 2.0f;
	Debug.Log("Operation 2 has concluded.");
}
```

### Sequences

A ``Sequence`` is a customizable sequence of coroutines, function calls, and delays. Instead of creating a new function for every combination of coroutines, you can instead construct a coroutine out of common parts.

```csharp
public void Awake()
{
	// This will run FirstBeauRoutine, wait 5 seconds, then call Destroy(gameObject).
	Routine.Start(this,
		Sequence.Create(FirstBeauRoutine())
		.Wait(5.0f)
		.Then(() => { Destroy(gameObject); })
		);
}

public IEnumerator FirstBeauRoutine()
{
	...
}
```

## Tweens

### Tweens as Coroutines
In BeauRoutine, tweens are implemented as a special kind of coroutine called ``Tween``.  They perform animation over time.

```csharp
// Create the tween
Tween tween = Tween.Float(...);

// Start it as if it were a coroutine.
Routine.Start( this, tween );

// Or combine it into one line.
Routine.Start( this, Tween.Float(...) );
```

### Modifying a Tween
After creating a Tween, you can then enhance it with curves, loops, and wave functions before starting it.

```csharp
// Create the tween.
Tween tween = Tween.Float(...);

// You can control how many times a Tween will run.
tween.Once(); // Only run once. Default behavior.
tween.Loop(); // Loop forever.
tween.Loop(2); // Loop 2 times then stop.
tween.Yoyo(); // Run forwards then reversed once.
tween.YoyoLoop(); // Run forwards and reversed forever.
tween.YoyoLoop(2); // Run forwards then reversed 2 times.

// You can even set the Tween to run FROM the given target
// to the current value instead of the other way around.
tween.From();

// You can also apply easing functions.
tween.Ease( Curve.Smooth ); // Apply a pre-made easing function.
tween.Ease( AnimationCurve.Linear(0, 0, 1, 1) ); // Apply a Unity AnimationCurve for more customizable control.

// Wave functions are also supported.
// Frequency values are the number of complete waves
// over the duration of one run of the Tween.
tween.Wave( Wave.Function.Sin, 2.0f ); // Modify the tween by a sine function with a frequency of 2.
tween.Wave( new Wave(Wave.Function.Sin, 2.0f) ); // Same effect as previous line.

// You can also change the duration.
tween.Duration(2.0f);

// You can delay a tween or make it start from a specific time.
// Note that these are mutually exclusive operations.
tween.DelayBy(2.0f); // Will not start for 2 seconds.
tween.StartsAt(2.0f); // Will fast forward 2 seconds when starting.

// These can be chained together for a more compressed and fluent syntax.
Tween tween = Tween.Float(...).Ease(Curve.Smooth).YoyoLoop();
```

For further info about easing functions, I recommend checking out [Robert Penner's Easing Functions](http://robertpenner.com/easing/) and [Easings.net](http://easings.net/).

### Value Shortcuts
BeauRoutine comes with a set of shortcut functions and extension methods for generating Tweens.

Value tweens shortcuts will generally have the following signature:
```csharp
TypeName ( Start, End, Set Function, Duration, [Required Args], [Optional Args] )

//Examples
Tween.Float( 0f, 1f, (f) => { myFloat = f; }, 2 );
Tween.Color( Color.white, Color.black, (c) => { myColor = c; }, 3, ColorUpdate.FullColor );

//Generic version requires an interpolator function
Tween.Value<float>( 0.0f, 1.0f, (f) => { myFloat = f; }, Mathf.Lerp, 2f );
```

### Extension Methods

Extension methods will generally have the following signature:
```csharp
// If called from TweenShortcuts
PropertyTo ( Object, End, Duration, [Required Args], [Optional Args] )
// If called as extension (e.g. transform.MoveTo)
PropertyTo ( End, Duration, [Required Args], [Optional Args] )

//Examples
transform.MoveTo( new Vector3( 1.0f, 1.0f, 1.0f ), 2.0f );
rectTransform.AnchorPosTo( 0f, 0.3f, Axis.X );
audioSource.VolumeTo( 0.0f, 2.0f );
```

### Mirrored Tweens

Yoyo Tweens have a property called ``Mirrored``, which affects how easing functions are applied when the Tween is reversed.  Normally, reversed Tweens apply their easing functions as if the Tween was not reversed.  An EaseOut will have the same progression over time from end to start as it did from start to end.  A mirrored Tween will reverse the easing function as well.

Put in other terms, a normal Tween that starts slow and speeds up animating from start to end will also start slow and speed up when animating back to start.  The same Tween, mirrored, will start fast and slow down when animating back to start.

## Advanced Features

### Routine Identity

The ``RoutineIdentity`` component can be attached to any GameObject to apply a per-object time scale.  Note that this will only apply to BeauRoutines that are started after attaching the RoutineIdentity - it will not retroactively apply time scaling on BeauRoutines run before it was attached.

You can find the RoutineIdentity for a given object by calling ``RoutineIdentity.Find``.  If you want to ensure the object has a RoutineIdentity, call ``RoutineIdentity.Require``, which will lazily instantiate an instance on the given object.

Per-object time scale is controlled by the ``TimeScale`` property. See [Time Scale](#time-scale) for information about how time scale affects a BeauRoutine.

```csharp
// Make sure we have a RoutineIdentity for this object.
RoutineIdentity identity = RoutineIdentity.Require(this);

// Apply some slow motion.
identity.TimeScale = 0.25f;

// MyCoroutine will now run at 0.25 speed.
Routine.Start( this, MyCoroutine() );
```

In the case you may want a BeauRoutine to ignore the per-object time scale,  you can call ``DisableObjectTimeScale``.  ``EnableObjectTimeScale`` will re-enable it.

```csharp
// Make sure we have a RoutineIdentity for this object.
RoutineIdentity identity = RoutineIdentity.Require(this);

// Apply some slow motion.
identity.TimeScale = 0.25f;

// Do something in slow motion.
Routine.Start( this, MyCoroutine() );

// After 2 real seconds, reset the time scale on this object's RoutineIdentity.
Routine.Start (this, ResetTimeScale( 2.0f ) ).DisableObjectTimeScale();

IEnumerator MyCoroutine()
{
	...
	// This will run at 0.25 speed.
}

IEnumerator ResetTimeScale(float delay)
{
	// This will run at normal speed.
	yield return delay;
	RoutineIdentity.Require( this ).TimeScale = 1.0f;
}
```

### Time Scale

BeauRoutine offers a powerful time scaling system.  Every BeauRoutine has its own time scaling value, which can be retrieved or set by calling ``GetTimeScale`` or ``SetTimeScale`` on a Routine object.

```csharp
// Create a Routine
Routine r = Routine.Start( this, MyCoroutine() );

// Retrieve the time scale for the Routine and double it
// Time scaling, by default, starts at 1
float timeScale = r.GetTimeScale();
timeScale *= 2;
r.SetTimeScale( timeScale ); 

// Run at half speed
timeScale = 0.5f;
r.SetTimeScale( timeScale );
```

Time scale affects ``Routine.DeltaTime``, which is calculated directly before a BeauRoutine is run every frame.  DeltaTime is a product of ``Time.deltaTime`` and any scaling factors that need to be applied to a BeauRoutine.  If you need unscaled time for any reason, use ``Routine.UnscaledDeltaTime``.

```csharp
IEnumerator IncrementByTime()
{
	float myFloat = 0.0f;
	while( true )
	{
		myFloat += Routine.DeltaTime;
		yield return null;
	}
}
```

Time scale also affects any waiting you perform within a coroutine (e.g. ``yield return 1.0f``, ``yield return Routine.WaitSeconds(1.0f)``).

```csharp
// This will wait 1 second.
Routine normalSpeed = Routine.Start( this, MyCoroutine() );
Routine halfSpeed = Routine.Start( this, MyCoroutine() ).SetTimeScale(0.5f);
Routine doubleSpeed = Routine.Start( this, MyCoroutine() ).SetTimeScale(2);

IEnumerator MyCoroutine()
{   
	// Any waiting you perform like this is specified in unscaled time.
	// In normalSpeed, this will take 1 real second.
	// In halfSpeed, this will take 2 real seconds.
	// In doubleSpeed, this will take 0.5 real seconds.
	yield return 1.0f;
}
```

Time scale also applies to any Tweens you're currently running.

In total, there are four time scale values that can apply to a BeauRoutine.
* ``Routine.TimeScale`` (multiplied with ``Time.timeScale`` to get global time scale)
* Per-group time scales (see [Groups](#groups))
* Per-object time scales (see [Routine Identity](#routine-identity))
* Time scale for an individual BeauRoutine

You can calculate the total time scale that will be applied to BeauRoutines on a given object by calling ``Routine.CalculateTimeScale``. This is the accumulation of ``Routine.TimeScale``, the per-object time scale, and the per-group time scale.

### Update Phase

By default, BeauRoutines update during Unity's LateUpdate phase. This default can be changed, however, and BeauRoutines can be set to execute at one of four times: ``LateUpdate``, ``Update``, ``FixedUpdate``, and ``Manual``. Manual updates, as the name implies, are called manually and have several rules and restrictions, which are documented [here](#manual-updates).

``FixedUpdate`` routines work off of a consistent delta time.

```csharp
// By default, BeauRoutine starts routines in the LateUpdate phase.
Routine.Start( this, MyCoroutine() ); // This executes in LateUpdate

// You can set the phase by calling SetPhase on a routine
Routine.Start( this, MyCoroutine() ).SetPhase( RoutinePhase.FixedUpdate ); // This will execute during FixedUpdate

// You can also set the default phase for new routines.
Routine.Settings.DefaultPhase = RoutinePhase.FixedUpdate;
Routine.Start( this, MyCoroutine() ).GetPhase(); // GetPhase will return FixedUpdate

Routine.Start( this, MyCoroutine() ).SetPhase( RoutinePhase.Update ); // This will execute during Update
```

Yielding a ``WaitForFixedUpdate``, ``WaitForEndOfFrame``, ``WaitForLateUpdate``, ``WaitForUpdate``, or any ``RoutinePhase`` will interrupt the normal timing of a BeauRoutine, and will instead execute it during those events. This does not apply for Manual updates, however; see the [Manual Updates](#manual-updates) section for more information. 

```csharp
Routine.Start( this, MyCoroutine() ).SetPhase( RoutinePhase.Update );

IEnumerator MyCoroutine()
{
	// Executing during Update
	Debug.Log( Routine.DeltaTime ); // Normal deltaTime

	yield return Routine.WaitForFixedUpdate();
	// Now executing during FixedUpdate.
	Debug.Log( Routine.DeltaTime ); // Now using fixedDeltaTime

	yield return null;
	// Executing during Update again
	Debug.Log( Routine.DeltaTime ); // Back to normal deltaTime

	yield return Routine.WaitForEndOfFrame();
	// Executing after all rendering is completed
	Debug.Log( Routine.DeltaTime ); // Still using normal deltaTime

	yield return RoutinePhase.FixedUpdate; // Same as WaitForFixedUpdate
	
	// Waits until the next Update phase
	yield return Routine.WaitForUpdate();
	yield return RoutinePhase.Update;

	// Waits until the next LateUpdate phase
	yield return Routine.WaitForLateUpdate();
	yield return RoutinePhase.LateUpdate;

	// This does not do anything meaningful, and is equivalent to yielding null.
	yield return RoutinePhase.Manual;
}
```

The ``WaitForLateUpdate`` and ``WaitForUpdate`` events occur after their respective LateUpdate and Update phases. If a BeauRoutine is already executing in LateUpdate or Update, yielding for those phases will wait until after the next time the phase is executed, instead of executing immediately after the phase is complete.

See Unity's [Execution Order of Event Function](https://docs.unity3d.com/Manual/ExecutionOrder.html) documentation for further information on update timing.

### Priority

BeauRoutine provides a way of changing the priority of any individual BeauRoutine. Priority determines the order in which BeauRoutines are executed, from highest priority to lowest. This could be useful if the execution of one routine is dependent on the result of another. BeauRoutines with equal priority will be executed in order of the last time priority was set, or creation if priority was never specifically set. Priority will default to 0.

```csharp
// Create the routines
// r1 will be executed before r2, since it was created first
Routine r1 = Routine.Start( this, MyCoroutineA() );
Routine r2 = Routine.Start( this, MyCoroutineB() );

// Set the priorities of the routines
// Higher priorities will be executed first
// Priority defaults to 0
int r1Priority = r1.GetPriority(); // 0
r2.SetPriority(r1Priority + 100); // 100

// r2 will now execute before r1,
// even though it was started after r1.
```

Note that this only affects order within an update phase. See [Update Phase](#update-phase) for more information.

### Groups

Routines can be organized into one of 32 groups that can be paused, resumed, or time-scaled.  Groups are automatically assigned based on the ``Group`` property of the attached RoutineIdentity.  While groups are maintained as integers, it's highly encouraged to use an enum type instead to name your groups.

```csharp
// Tag any enum with this attribute and the Group property
// in the RoutineIdentity inspector will be displayed as
// an dropdown list instead of an integer field.
[RoutineGroupEnum]
public enum ObjectGroup
{
	Default = 0,
	Gameplay = 1,
	UI = 2
}

// Assign a group to this RoutineIdentity
RoutineIdentity.Require( this ).Group = (int)ObjectGroup.Gameplay;

// This BeauRoutine now has ObjectGroup.Gameplay (1) as its group.
Routine.Start( this, MyCoroutine() );

// You can get, set, or reset the timescale for a group
float groupTimeScale = Routine.GetGroupTimeScale( ObjectGroup.Gameplay );
Routine.SetGroupTimeScale( ObjectGroup.Gameplay, 0.5f );
Routine.ResetGroupTimeScale( ObjectGroup.Gameplay ); // Resetting resets to 1.

// While a single group is referenced by index, the PauseGroups and ResumeGroups
// functions take in a bitmask of all affected groups.
int groupMask = Routine.GetGroupMask( ObjectGroup.Gameplay, ObjectGroup.UI );

// You can pause and resume multiple groups at a time.
Routine.PauseGroups( groupMask );
Routine.ResumeGroups( groupMask );
```

It's important to note that a BeauRoutine can only belong to a single group.  This is to ensure consistent timescaling behavior.

### Inlined Coroutines

Normally, yielding into a nested coroutine will wait until the next frame to begin executing the nested coroutine. BeauRoutine provides a way of immediately entering and executing a nested coroutine by wrapping your coroutine in a ``Routine.Inline`` call. Inlined coroutines, once finished, will also immediately resume execution on the coroutine that yielded it, skipping the one frame delay. This is useful if you need to time your coroutines in a very precise manner.

```csharp
IEnumerator RootCoroutine()
{
	Debug.Log("This is the RootCoroutine log");
	yield return Routine.Inline( NestedCoroutine() );
	Debug.Log("This will log on the same frame as the NestedCoroutine log");
}

IEnumerator NestedCoroutine()
{
	Debug.Log("This will log on the same frame as the RootCoroutine log");
	...
}
```

Note that this is not supported when starting a new BeauRoutine. See [On Starting BeauRoutines and Update Phases](#on-starting-beauroutines-and-update-phases) for more information.

```csharp
// Despite its appearance, this will not execute immediately.
Routine.Start( Routine.Inline( RootCoroutine() ) );

IEnumerator RootCoroutine()
{
	...
}
```

You can also avoid the one frame delay when exiting a nested coroutine by yielding ``Routine.Command.BreakAndResume`` instead of letting your coroutine exit normally.

```csharp
IEnumerator RootCoroutine()
{
	yield return NestedCoroutine();
	Debug.Log("This will log on the same frame as the NestedCoroutine log");
}

IEnumerator NestedCoroutine()
{
	Debug.Log("This is the NestedCoroutine log");
	yield return Routine.Command.BreakAndResume;
}
```

Be careful when using these features. Inlined execution offers more precise timing but runs the risk of executing too much in a single frame if overused.

### Futures

BeauRoutine includes a basic implementation of Futures. Futures provide a way to asynchronously return values from functions and coroutines, without requiring the use of a callback.

A ``Future`` object  represents a value that will be returned by a function or coroutine at some point in the future. A Future must be completed with the ``Complete`` call before its value can be obtained. Alternately, a Future can ``Fail`` with an optional error token. A Future can only be completed or failed once. Once either operation has been performed, future calls to those functions will throw an exception.

Futures are handy when dealing with asynchronous services - making calls to a REST server, for example.

```csharp
// This will asynchronously calculate the hash of a string
Future<int> CalculateHashAsync( string str )
{
	// To create a new future, call Future.Create<T>()
	Future<int> future = Future.Create<int>();
    
    // Start a routine that will eventually complete the promise
    Routine routine = Routine.Start( CalculateHashAsyncImpl( future, str ) );
    
    // Link the Routine to the Future to ensure the Routine will stop
    // if the Future is cancelled.
    // A linked Routine will also cancel the Future if it ends prematurely.
    future.LinkTo( routine );
    
    // This future represents the eventual result of CalculateHashAsyncImpl
    return future;
}

// We need access to the Future in order to complete it
// We can pass it in as an argument to accomplish this
IEnumerator CalculateHashAsyncImpl( Future<int> future, string str )
{
	// Some artificial delay to make this asynchronous
	yield return 0.25f;

	// Report progress information to the future.
	future.SetProgress(0.5f);

	// More artificial delay
	yield return 0.25f;
    
    if (str == null)
    {
    	future.Fail();
    }
    else
    {
    	int hash = str.GetHashCode();
        future.Complete(hash);
    }
}

IEnumerator DoSomething()
{
	var future = CalculateHashAsync( "someArbitraryString" );
    
    // You can yield a Future to wait for it to
    // either complete or fail
    yield return future;
    
    if ( future.IsComplete() )
    {
    	// Implicitly cast a future to its return type
        // If the future is not complete, this will throw an exception.
    	int hash = future;
        Debug.Log( hash );
    }
    else if ( future.IsFailed() )
    {
    	Debug.Log("error occurred");
    }
    
    // You can provide OnComplete and OnFail callbacks
    // These will get called at the end of the frame when
    // the Future completes or fails.
    var future2 = CalculateHashAsync( null )
    	.OnComplete( (i) => { Debug.Log(i); } )
        .OnFail( () => { Debug.Log("error occurred"); } );

	// You can also provide progress callbacks.
	// These will get called when progress through a future changes.
	future2.OnProgress( (progress) => { Debug.Log("progress: " + progress); } );
}
```

### Custom Tweens

Tweens in BeauRoutine are highly extensible through the ``ITweenData`` interface. Tweens perform the timing, easing, and looping logic; an ITweenData object applies the animation.  To use one of your own ITweenData-derived objects, you can use the ``Tween.Create`` function with your object as an argument.

```csharp
// Making your own ITweenData
public class SomeObjectTweenData : ITweenData
{
	private SomeObject m_Object;
	
	private float m_Start;
	private float m_End;
	
	public SomeObjectTweenData( SomeObject inObject, float inEnd )
	{
		m_Object = inObject;
		
		m_Start = inObject.MyValue;
		m_End = inEnd;
	}

	public void OnTweenStart()
	{
		...
		// Any custom logic when a tween starts can go here
	}
	
	public void ApplyTween(float inPercent)
	{
		m_Object.MyValue = Mathf.Lerp( m_Start, m_End, inPercent );
	}
	
	public void OnTweenEnd()
	{
		...
		// Any custom logic when a tween ends or is killed can go here
	}
}

public class SomeObject
{
	public float MyValue = 0;
	
	public Tween MyValueTo( float inValue, float inTime )
	{
		SomeObjectTweenData tweenData = new SomeObjectTween( this, inValue );
		return Tween.Create( iTweenData, inTime );
	}
}
```

### Manual Updates

You can manually update both individual BeauRoutines and the set of BeauRoutines with their phase set to ``Manual``.

#### Individual Routines  
You can call ``TryManuallyUpdate`` on a BeauRoutine to attempt to force it to update, optionally providing a timestep. The BeauRoutine will fail to update if it is already updating. You can attempt to force other BeauRoutines to update from within a ``TryManuallyUpdate`` call. This call is not restricted by update phase.

#### Manual Set
You can call ``Routine.ManualUpdate`` to attempt to update the set of BeauRoutines set to the ``Manual`` phase. BeauRoutines that are already updating will not update. You cannot nest these calls.

#### Restrictions
Manual BeauRoutines will not respond to yields that would change their update phase, such as ``WaitForFixedUpdate`` or ``WaitForEndOfFrame``. While these phase changes make sense for an automatically-updated BeauRoutine, they do not for a manually-updated BeauRoutine. As such, these yields are equivalent to waiting for a frame.

### Debugger

BeauRoutine includes a simple profiler and debugger, found under the menu item ``Window/BeauRoutine Debugger``.

The ``STATS`` page displays the number of BeauRoutines currently running, the maximum you've had running at the same time, and the total you can run concurrently without allocating more memory.  It also keeps track of the average time it takes to execute all the currently running BeauRoutines, and maintains a snapshot of what was running when the last maximum was recorded.

The ``OPTIONS`` page displays the current time scale, as well as options for resetting it, doubling it, or halving it. It also contains a button enabling or disabling the snapshot feature on the STATS page.

The ``DETAILS`` page displays all the currently running BeauRoutines in a list.  From here, you can pause, resume, or stop any of them, as well as rename them or set their time scale.  Any Combine or Race routines are presented in a hierarchical format.

## Tips and Tricks

#### Use ``Routine`` handles as slots

Maintain ``Routine`` handles as "slots" for executing coroutines. This can be helpful for ensuring you only have one instance of a particular type of operation executing at a time. It can also help keep track of executing BeauRoutines, allowing you to selectively clean up any operations instead of stopping all BeauRoutines on a given object.

This is particularly necessary if you have operations that execute on shared data. Two Tweens fading the same SpriteRenderer will interfere with one another, so ensuring you only have one Tween executing on that property at a time will help prevent unwanted behavior.

```csharp
// This class will move and fade the given object on command.
// It can also perform an important operation on a string.
public class MyAnimatingObject : MonoBehaviour
{
	// These Routine objects effectively act as slots.
	// If you don't want duplicates of an operation running simultaneously,
	// you can use these handles to stop old operations and start new ones
	private Routine fadeAnimation;
	private Routine moveAnimation;
	private Routine importantRoutine;
	
	public void FadeTo(float alpha, float duration, Curve curve = Curve.Linear)
	{
		// Replace is safe to call, even if the Routine isn't currently referencing an active BeauRoutine
		fadeAnimation.Replace( this, GetComponent<SpriteRenderer>().FadeTo( alpha, duration ).Ease( curve ) );
	}
	
	public void MoveTo(Vector3 position, float duration, Curve curve = Curve.Linear)
	{
		moveAnimation.Replace( this, transform.MoveTo( position, duration ).Ease( curve ) );
	}
	
	public void DoSomethingImportant(string data)
	{
		importantRoutine.Replace( this, DoSomethingImportantRoutine( data ) );
	}
	
	private IEnumerator DoSomethingImportantRoutine(string data)
	{
		...
	}
}
```

#### Limit a BeauRoutine's lifetime with a ``using`` statement

Execute a BeauRoutine in the background while executing a block in your coroutine with a ``using`` statement. This is useful if you want an operation to execute only for as long as a block in the coroutine is executing.
```csharp
IEnumerator DoImportantWork(string[] importantData)
{
	using( Routine.Start( this, PlayAnAnnoyingSoundLoop() )
	{
 		// As long as DoImportantWork is executing within this block,
 		// PlayAnAnnoyingSoundLoop will execute.
		
		for(int i = 0; i < importantData.Length; ++i)
		{
			Debug.Log("Doing some important work with: " + importantData[i]);
			yield return 1;
	  	}
	}

	using( Routine.Start( this, PlayAnAnnoyingSoundOnce() )
	{
		// Within this block, PlayAnAnnoyingSoundOnce will execute
		// We exit this block before PlayAnnoyingSoundOnce has
		// a chance to log its message, however.

		Debug.Log("Doing important work with no data");
		yield return 1;
	}
}

IEnumerator PlayAnAnnoyingSoundLoop()
{
	while(true)
	{
		Debug.Log("Playing an annoying sound");
		yield return 0.2f + Random.value * 0.25f;
	}
}

IEnumerator PlayAnAnnoyingSoundOnce()
{
	yield return 2;
	Debug.Log("Playing an annoying sound");
}
```

#### Split your coroutine functions to execute code immediately

When starting a BeauRoutine, the work is queued up to execute at the next opportunity for the BeauRoutine's update phase. If you need some code to execute immediately, you can split your coroutine functions into instant and an over-time portions.

```csharp
// This needs to happen as soon as the BeauRoutine starts.
void ResetVisuals() { }

// This happens within the BeauRoutine.
void UpdateVisuals() { }

Routine.Start( ReturnsACoroutine() );

// This is a regular function.
IEnumerator ReturnsACoroutine()
{
	// Instant work goes here
	...

	Debug.Log( "This is executing outside of the BeauRoutine." );

	// This needs to happen immediately.
	// Putting it in here ensures it will execute
	// as soon as InstantWorkIntoRoutine is called.
	ResetVisuals();

	// You can return an IEnumerator directly.
	return IsACoroutine();
}

// This is a coroutine.
IEnumerator IsACoroutine()
{
	// If we call ResetVisuals here instead,
	// it will happen on a one-frame delay.

	UpdateVisuals();

	// Important work goes here
	...

	// An IEnumerator function only becomes a coroutine
	// once a yield statement is encountered.
	yield return null;
	
	Debug.Log( "This is executing within the BeauRoutine." );
}
```

``IEnumerator`` functions only become coroutines once a ``yield return`` statement is used. If you ``return`` normally, it is treated as a regular function.

See [On Starting BeauRoutines and Update Phases](#on-starting-beauroutines-and-update-phases) for more information.

#### Use ``TweenUtil.Lerp`` to asymptotically interpolate to a changing target

Tweens are great for interpolating towards fixed targets, but in cases where the target is changing continuously, they are suboptimal.

As an example: Say you want the camera to smoothly interpolate to a point determined by player input.
* You have a player character viewed from a top-down angle.
* You want the camera to rest somewhere halfway between the player and a point extending out from the player's facing direction.
* The facing direction may change often, since it is tied directly to player input.
* You want the camera to smoothly transition to its target position.

Given these constraints, there are several approaches you may take.
* Start a tween every time the direction changes. This may result in jittery, inconsistent camera movement.
* Interpolate with a fixed time towards your dynamic target. This may result in unwanted acceleration or deceleration as the target position changes during the interpolation.
* Apply some level of physics simulation to the camera, or perhaps a steering behavior. This is a valid, if more complicated approach.
* Forgo transitions entirely - instantly snap the camera to the target position. This is also a valid approach, if potentially disorienting depending on the perspective.

There are simpler ways of achieving this effect.

To achieve smoother, more consistent motion, you can asymptotically interpolate towards the target value. This involves moving the value by a percentage of the difference between the value and the target every frame. In effect, as the value gets closer to the target, the rate of change per-frame decreases, resulting in a smooth and pleasant deceleration.

In games with a fixed framerate, or games that calculate based on frame, not delta time, it may look something like this:
```csharp
// This will interpolate someValue to someTarget by 25% each frame
someValue = Mathf.Lerp( someValue, someTarget, 0.25f );
```
This approach is not sufficient for games with a variable framerate, however. As the game slows down and fewer frames are executed per second, the object will move at a slower rate towards the target.

A revised approach needs to account for varying delta time, perhaps by scaling the percentage. It might look like this:
```csharp
// This scales the interpolation percent based on delta time.
// The expected framerate is 60. Framerate multipled by delta time
// should get a ratio of expected frame time to actual frame time.
int targetFrameRate = 60;
float lerpScale = targetFrameRate * Time.deltaTime;
someValue = Mathf.Lerp( someValue, someTarget, 0.25f * lerpScale );
```
This feels more accurate, and but again, this approach falls apart with large timesteps, resulting in incorrect behavior. If, say, the game slows down enough to skip 60 frames, or 120 frames, you end up with larger and larger multiplies of your desired interpolation percentage, which could lead to reaching the target or even overshooting the target. This becomes more likely as the percentage increases, limiting the range of reasonable percentages you can use in this approach. Ultimately, this is not an accurate approach.

To correctly simulate this type of asymptotic interpolation, we not only need to account for the distance remaining at the time of the lerp, we also need to account for the change in distance remaining _within the lerp itself_. In other words, the change in rate of change needs to occur _continuously_, instead of at discrete intervals. This can be modeled in terms of [expotential decay](https://en.wikipedia.org/wiki/Exponential_decay).

BeauRoutine provides ``TweenUtil.Lerp``. This function scales the rate of change to appropriately handle fluctuations in framerate and maintain the asymptotic nature of the interpolation.

It is important to note that these functions, by default, interpret the given percentages as _per-second_, not per-frame. If you already have your percentages specified in terms of a fixed framerate, you can call ``TweenUtil.SetDefaultLerpPeriodByFramerate`` to specify the expected timestep. Both functions can also accept a period and a timestep. If a timestep is not provide, these functions will use ``Routine.DeltaTime``.

```csharp
// Since we've already been using 25% with an expectation of 60 frames per second,
// we can modify the default period to match.
int targetFrameRate = 60;
TweenUtil.SetDefaultLerpPeriodByFramerate( targetFrameRate );

float scaledLerp = TweenUtil.Lerp( 0.25f );
someValue = Mathf.Lerp( someValue, someTarget, scaledLerp );

// You could also multiply out the percentage by the framerate for the same effect.
TweenUtil.SetDefaultLerpPeriod(1);
scaledLerp = TweenUtil.Lerp( 0.25f * targetFrameRate ); // This is mathematically equivalent
```

``TweenUtil.LerpDecay`` returns the equivalent of ``1 - TweenUtil.Lerp``, which can be a useful shortcut when interpolating towards 0. This might occur when simulating friction or drag, for example.

## Technical Notes

#### On Starting BeauRoutines and Update Phases

When you start a BeauRoutine, you are adding it to a queue to execute during the next instance of its update phase. If you start a BeauRoutine for a currently-executing update phase, it will not begin executing until the next instance of that update phase.

```csharp
Routine.Start( PartA() ).SetPhase( RoutinePhase.Update );

IEnumerator PartA()
{
	...
	// This occurs on one frame

	// This routine will not execute until the next frame,
	// since PartA is executing in the same update phase
	Routine.Start( PartB() ).SetPhase( RoutinePhase.Update );
}

IEnumerator PartB()
{
	...
	// This occurs on the next frame
}
```

If you need code to execute immediately, this can be worked around by dividing your routine into two functions. See [Split your coroutine functions to execute code immediately](#split-your-coroutine-functions-to-execute-code-immediately) for more information.

This can also be circumvented by calling ``TryManuallyUpdate`` on a routine to attempt to force it to execute immediately. See [Manual Updates](#manual-updates) for more information.

#### On Stopping BeauRoutines

When you stop a BeauRoutine, it will not be cleaned up until the next time it attempts to execute. If the BeauRoutine is set to manually update, and is not currently executing, it will be cleaned up immediately.

#### On Reserved Routine Names

BeauRoutines must not be given a name that starts with ``[BeauRoutine]__``. This is a reserved prefix used for internal purposes. In the (admittedly unlikely) event you attempt to set a name with this prefix, it will log a warning instead and not change the name.

#### On Delta Time

For BeauRoutines paused for some amount of time, perhaps with a ``Routine.WaitSeconds`` or a ``yield return 1f``, ``Routine.DeltaTime`` will adjust when that period ends to account for time overlap. For example, if the BeauRoutine requests to wait for 2 seconds, but we wait for 2.1 seconds, delta time will be decreased by 0.1 seconds to compensate.

This time adjustment does not apply during the FixedUpdate phase, or after yielding a ``WaitForFixedUpdate``. This is to ensure consistent delta time during FixedUpdate.

#### On Memory Allocation

BeauRoutine will be initialized the first time a BeauRoutine operation is performed. You can also initialize BeauRoutine in advance by calling ``Routine.Initialize``.

BeauRoutine pre-allocates resources to run a default number of concurrent BeauRoutines. When there are no more resources available to execute all scheduled BeauRoutines, it will allocate more. This doubles the maximum number of concurrent BeauRoutines (16 -> 32 -> 64 -> ...), up to a maximum of 16,777,216. If you desire to reduce runtime allocations, you can call ``Routine.Settings.SetCapacity`` to pre-allocate the resources necessary to run the given number of BeauRoutines.

To determine your game's requirements, open up the [Debugger](#debugger) during gameplay and view the ``MAX`` field in the ``STATS`` page. This will tell you how many BeauRoutines that the system needed to allocate resources for during the current session. The ``CAPACITY`` field will tell you how many were actually allocated in order to support it. By calling ``Routine.Settings.SetCapacity``, you can pre-allocate for the peak number of BeauRoutines in your game and optimize your memory usage.

While BeauRoutine attempts to avoid runtime allocations as often as possible, there are unavoidable allocations associated with a C# coroutine framework. Coroutines, as implemented in C#, allocate memory when called. Coroutines are implemented in C# as _iterator blocks_. C# compilers transform iterator blocks into state machine classes. Calling an iterator block will allocate a new instance of its associated state machine. These allocations are tiny but worth mentioning for memory-constrained applications. Tweens and certain utilities, such as ``Routine.Combine`` and ``Routine.Delay``, will also allocate small amounts of memory.

For more information on how coroutines are implemented in C# compilers, read this article: [C# In Depth: Iterator block implementation details](http://csharpindepth.com/Articles/Chapter6/IteratorBlockImplementation.aspx).

#### On Important Differences between BeauRoutines and Unity Coroutines

| **Category** | **BeauRoutine** | **Unity coroutines** |
| ------------ | --------------- | -------------------- |
| Starting a coroutine | ``Routine.Start`` queues the coroutine to be executed during its update phase. | ``StartCoroutine`` executes the first frame of the coroutine immediately. |
| Enabling/Disabling MonoBehaviours | By default, BeauRoutines will pause when their host MonoBehaviour is inactive. | Unity coroutines will not pause when their host MonoBehaviour is inactive. |

## Reference

### Routine Functions

These functions can be called directly on a Routine handle.

| Function | Description |
| -------- | ----------- |
| **Pausing** |
| ``Pause`` | Pauses the BeauRoutine. |
| ``Resume`` | Resumes the BeauRoutine. |
| ``GetPaused`` | Returns if the BeauRoutine is paused. |
| ``Delay`` | Delays the BeauRoutine by the given number of seconds. This is cumulative. |
| **Stopping** | |
| ``Stop`` | Stops the BeauRoutine. |
| ``Replace`` | Stops the BeauRoutine and replaces it with another one. |
| **Time Scaling** | |
| ``GetTimeScale`` | Returns the per-BeauRoutine timescale. |
| ``SetTimeScale`` | Sets per-BeauRoutine timescale. |
| ``DisableObjectTimeScale`` | Ignores per-object timescale on this BeauRoutine. |
| ``EnableObjectTimeScale`` | [Default] Uses per-object timescale on this BeauRoutine. |
| **Execution**| |
| ``ExecuteWhileDisabled`` | BeauRoutine will continue to execute while its host is disabled. |
| ``ExecuteWhileEnabled`` | [Default] BeauRoutine will not execute while its host is disabled. |
| ``GetPriority`` | Returns the execution priority of the BeauRoutine. |
| ``SetPriority`` | Sets the execution priority of the BeauRoutine. Greater priority BeauRoutines are executed first. |
| ``GetPhase`` | Returns the update phase for the BeauRoutine. |
| ``SetPhase`` | Sets the update phase for the BeauRoutine. The Routine be executed the next time the phase updates. |
| ``TryManuallyUpdate`` | Attempts to manually update the BeauRoutine with the given delta time. |
| **Name** | |
| ``GetName`` | Gets the name of the BeauRoutine. If a name has not been set, this will return the name of the coroutine provided when starting the BeauRoutine. |
| ``SetName`` | Sets the name of the BeauRoutine. |
| **Events** | |
| ``OnComplete`` | Registers a function to execute once the BeauRoutine completes naturally. |
| ``OnStop`` | Registers a function to execute once the BeauRoutine completes prematurely. |
| ``OnException`` | Registers a function to execute if an exception is encountered while updating the BeauRoutine. |
| **Miscellaneous**| |
| ``Exists`` | Returns if this BeauRoutine has not been stopped. |
| ``Wait`` | Returns a coroutine that waits for the given BeauRoutine to end. |

### Routine Utilities

| Name | Description |
| -------- | ----------- |
| **Start** | |
| ``Routine.Start`` | Starts a BeauRoutine. |
| ``Routine.StartDelay`` | Starts a BeauRoutine that calls a function or executes a coroutine after a certain number of seconds. |
| ``Routine.StartLoop`` | Starts a BeauRoutine that calls a function every frame. |
| ``Routine.StartLoopRoutine`` | Starts a BeauRoutine that executes a coroutine in a never-ending loop. |
| ``Routine.StartCall`` | Starts a BeauRoutine that calls a function at the end of the frame. |
| **Time** | |
| ``Routine.DeltaTime`` | Delta time for the current routine, with all applicable scaling applied. |
| ``Routine.UnscaledDeltaTime`` | Unscaled delta time for all routines. |
| **Flow** | |
| ``Routine.Pause`` | Pauses the BeauRoutine with the given name. |
| ``Routine.PauseAll`` | Pauses all BeauRoutines. |
| ``Routine.Resume`` | Resumes the BeauRoutine with the given name. |
| ``Routine.ResumeAll`` | Resumes all BeauRoutines. |
| ``Routine.Stop`` | Stops the BeauRoutine with the given name. |
| ``Routine.StopAll`` | Stops all BeauRoutines. |
| **Query** | |
| ``Routine.Find`` | Returns the Routine for the BeauRoutine with the given name. |
| ``Routine.FindAll`` | Returns a list of Routines for the BeauRoutines with the given host. |
| **Wait** | |
| ``Routine.WaitFrames`` | Waits for the given number of frames. |
| ``Routine.WaitSeconds`` | Waits for the given number of seconds. |
| ``Routine.WaitRealSeconds`` | Waits for the given number of unscaled seconds. |
| ``Routine.WaitCondition`` | Waits for the given function to return ``true``. |
| ``Routine.WaitForever`` | Waits until the heat death of the universe. |
| ``Routine.WaitRoutines`` | Waits until the given Routines expire. |
| ``Routine.WaitForFixedUpdate `` | Waits until FixedUpdate completes. |
| ``Routine.WaitForEndOfFrame`` | Waits until rendering of the current frame completes. |
| ``Routine.WaitForLateUpdate`` | Waits until LateUpdate completes. |
| ``Routine.WaitForUpdate`` | Waits until Update completes. |
| **Execution** | |
| ``Routine.Delay`` | Calls a function after the specified number of seconds. |
| ``Routine.Call`` | Calls a function at the end of the frame. |
| ``Routine.PerSecond`` | Calls a function the given number of times per second. |
| ``Routine.Combine`` | Runs multiple coroutines concurrently, and completes all. |
| ``Routine.Race`` | Runs multiple coroutines concurrently, and stops when one expires. |
| ``Routine.Inline `` | Executes the given coroutine immediately after yielding into it. |
| ``Routine.Yield`` | Immediately yields the provided value. |
| ``Routine.ManualUpdate`` | Updates all routines in the Manual phase. |
| **Groups** | |
| ``Routine.PauseGroups`` | Pauses all BeauRoutines in the given groups |
| ``Routine.ResumeGroups`` | Resumes all BeauRoutines in the given groups |
| ``Routine.GetGroupPaused`` | Returns if the given group is paused |
| ``Routine.GetGroupTimeScale`` | Returns the time scale for the given group |
| ``Routine.SetGroupTimeScale`` | Sets the time scale for the given group |
| ``Routine.ResetGroupTimeScale`` | Resets the time scale for the given group |
| ``Routine.GetGroupMask`` | Returns the bitmask for the given groups |
| **Misc** | |
| ``Routine.Timer`` | Counts down for the given number of seconds, with a callback for time remaining. |
| ``Routine.Accumulate`` | Counts up for the given number of seconds, with a callback for time accumulated. |
| ``Routine.ForEach`` | Generates and executes, in sequence, a coroutine for every element in the given list. |
| ``Routine.ForEachParallel`` | Generates and executes, in parallel, a coroutine for every element in the given list. |

### Routine Extensions

BeauRoutine contains a few extension methods for generating coroutines.

| Type | Function | Description |
| ---- | -------- | ----------- |
| AudioSource | ``WaitToComplete`` | Waits until the AudioSource is no longer playing. |
| ParticleSystem | ``WaitToComplete `` | Waits until the ParticleSystem is no longer emitting and no longer has any live particles |
| Thread | ``WaitToComplete`` | Waits until the thread is no longer alive. |
| Animator | ``WaitToCompleteAnimation`` | Waits until the current animation stops playing or loops. |
| | ``WaitToCompleteState`` | Waits until the Animator is playing and exits the given state. |
| | ``WaitForState`` | Waits until the Animator is playing the given state. |
| | ``WaitForNotState`` | Waits until the Animator is not playing the given state. |
| UnityEvent | ``WaitForInvoke `` | Waits until the UnityEvent has been invoked. |

BeauRoutine also provides a set of extension methods to set an Update/FixedUpdate/LateUpdate routine on a MonoBehaviour as a substitute for creating an Update/FixedUpdate/LateUpdate function.

| Type | Function | Description |
| ---- | -------- | ----------- |
| MonoBehaviour | ``SetUpdateRoutine`` | Sets a single Update/FixedUpdate/LateUpdate routine for the MonoBehaviour. Could be used to replace Update/FixedUpdate/LateUpdate functions. |
| | ``GetUpdateRoutine`` | Returns the single Update/FixedUpdate/LateUpdate routine for the MonoBehaviour. |

Note that these work with a specific set of names.

### Global Settings

All settings are available in the editor. Non-development builds disable access to several debug settings for performance reasons. Note that those settings can be safely called, but not modified.

| Name | Description | Restrictions |
| ---- | ----------- | --------- |
| **Properties** | | |
| ``Routine.Settings.Paused`` | Enables/disables all update loops. Note that manual updates will still function with this disabled. | --- |
| ``Routine.Settings.DefaultPhase`` | Sets the default update phase for new BeauRoutines. | --- |
| ``Routine.Settings.Version`` | Returns the BeauRoutine version number. | --- |
| ``Routine.Settings.DebugMode`` | Enabled or disables additional error checks. | Debug Only |
| ``Routine.Settings.HandleExceptions`` | Enables or disables exception handling on all BeauRoutines. Note that BeauRoutines with explicitly set exception handlers will still handle exceptions, regardless of this setting. | Debug Only |
| ``Routine.Settings.SnapshotEnabled`` | Enables or disables snapshotting. This will take snapshots of the highest number of simultaneous executing BeauRoutines. | Debug Only |
| **Functions** | | |
| ``Routine.Settings.SetCapacity`` | Pre-allocates for the given number of simultaneous executing BeauRoutines. Useful for avoiding unexpected allocations. | --- |
| ``Routine.Initialize`` | Initializes BeauRoutine. BeauRoutine will auto-initialize when you perform your first BeauRoutine operation, but this can be called earlier to allocate the necessary resources. | --- |
| ``Routine.Shutdown`` | Shuts down BeauRoutine. Any BeauRoutine operations will now throw an exception until ``Routine.Initialize`` is called again. | --- |

### Future Functions

| Function | Description |
| -------- | ----------- |
| ``IsDone`` | Returns if the Future is no longer in progress. |
| **Progress** | |
| ``IsInProgress`` | Returns if the Future is in progress.
| ``GetProgress`` | Returns the reported Future progress. |
| ``SetProgress`` | Sets the reported Future progress. |
| ``OnProgress`` | Registers a handler for when progress changes. |
| **Complete** | |
| ``IsComplete`` | Returns if the Future has been completed. |
| ``Get`` | Returns the value the Future successfully completed with. Will throw an exception if the Future did not complete successfully. |
| ``TryGet`` | Attempts to get the value the Future successfully completed with. |
| ``Complete`` | Completes the Future successfully with a value. |
| ``OnComplete`` | Registers a handler for when the Future successfully completes. |
| **Fail** | |
| ``IsFailed`` | Returns if the Future has failed. |
| ``GetFailure`` | Returns a ``Future.Failure`` object if the future has failed. Throws an exception if the Future did not fail. |
| ``TryGetFailure`` | Attempts to get the ``Future.Failure`` object the Future failed with. |
| ``OnFail`` | Registers a handle for when the Future fails. |
| **Cancel** | |
| ``IsCancelled`` | Returns if the Future was cancelled. |
| ``Cancel`` | Cancels the Future. |
| **Misc** | |
| ``LinkTo`` | Links the Future to a BeauRoutine. If the Future is cancelled, the BeauRoutine will stop. If the BeauRoutine stops before the Future is completed, the Future will fail. |
| ``Wait`` | Waits for the Future to no longer be in progress. |

### Future Utilities

BeauRoutine contains methods for creating Futures for simple tasks.

| Function | Future Type | Description |
| -------- | ----------- | ----------- |
| **Download from URL** | |
| ``Future.Download.WWW`` | WWW | Downloads and completes with a WWW. |
| ``Future.Download.UnityWebRequest`` | UnityWebRequest | Downloads and completes with a UnityWebRequest. |
| ``Future.Download.Text`` | String | Downloads and completes with text from a WWW/UnityWebRequest. |
| ``Future.Download.Bytes`` | Byte[] | Downloads and completes with a byte array from a WWW/UnityWebRequest. |
| ``Future.Download.Texture`` | Texture2D |  Downloads and completes with a texture from a WWW/UnityWebRequest. |
| ``Future.Download.AudioClip`` | AudioClip | Downloads and completes with an audio clip from a WWW/UnityWebRequest. |
| **Loading Resources** | |
| ``Future.Resources.LoadAsync<T>`` | T (Object) | Wrapper for Unity's ``Resources.LoadAsync``. Loads an asset from the  Resources folder asynchronously. |
| **Function Calls** | |
| ``Future.Call.Func<T>`` | T | Completes with the return value of the given function. |
| ``Future.Call.Resolve<T>`` | T | Creates and passes a future into the given function for it to complete or fail. |

### Tween Shortcuts

Generic tween shortcuts currently exist for the following types:

| Type | Function |
| ---- | -------- |
| Float | ``Tween.Float`` |
| | ``Tween.ZeroToOne`` |
| | ``Tween.OneToZero`` |
| Integer | ``Tween.Int`` |
| Vector2 | ``Tween.Vector`` |
| Vector3 | ``Tween.Vector`` |
| Vector4 | ``Tween.Vector`` |
| Rect | ``Tween.Rect `` |
| RectOffset | ``Tween.RectOffset`` |
| Quaternion | ``Tween.Quaternion`` |
| Color | ``Tween.Color`` |
| AnimationCurve | ``Tween.FloatCurve`` |
| Gradient | ``Tween.Gradient`` |

Tween extension methods currently exist for the following types:

| Type | Property | Function |
| ---- | -------- | -------- |
| Transform | Position | ``MoveTo``, ``MoveToWithSpeed`` |
| | Scale | ``ScaleTo`` |
| | Rotation | ``RotateTo``, ``LookAt`` |
| | Transform | ``TransformTo`` |
| RectTransform | Anchored Position | ``AnchorPosTo`` |
| | Anchors | ``AnchorTo`` |
| | Size Delta | ``SizeDeltaTo`` |
| | Pivot | ``PivotTo`` |
| | RectTransform | ``RectTransformTo`` |
| AudioSource | Volume | ``VolumeTo`` |
| | Pitch | ``PitchTo`` |
| | Pan | ``PanTo`` |
| Camera | Orthographic Size | ``OrthoSizeTo`` |
| | Field of View | ``FieldOfViewTo`` |
| **Rendering** | | |
| SpriteRenderer | Color/Alpha | ``ColorTo``, ``FadeTo``, ``Gradient`` |
| TextMesh | Color/Alpha | ``ColorTo``, ``FadeTo``, ``Gradient`` |
| Material | Color/Alpha | ``ColorTo``, ``FadeTo``, ``Gradient`` |
| **Canvas** | | |
| CanvasGroup | Alpha | ``FadeTo`` |
| CanvasRenderer | Color/Alpha | ``ColorTo``, ``FadeTo``, ``Gradient`` |
| Graphic | Color/Alpha | ``ColorTo``, ``FadeTo``, ``Gradient`` |
| Image | Fill Amount | ``FillTo`` |
| RawImage | UV Rect | ``UVRectTo``, ``UVRectShift`` |
| **Layout** | | |
| LayoutElement | Min Width/Height| ``MinSizeTo``|
| | Preferred Width/Height | ``PreferredSizeTo``| 
| | Flexible Width/Height | ``FlexibleSizeTo`` |
| LayoutGroup | Padding | ``PaddingTo`` |
| HorizontalOrVerticalLayoutGroup | Spacing | ``SpacingTo`` |
| GridLayoutGroup | Spacing | ``SpacingTo`` |
| | Cell Size | ``CellSizeTo`` |
| **BeauRoutine** | | |
| Routine | Time Scale | ``TimeScaleTo`` |
| RoutineIdentity | Time Scale | ``TimeScaleTo`` |

### Tween Modifiers

These functions will modify Tween objects. Do not call once the Tween has started.

| Function | Description |
| -------- | ----------- |
| **Modifying Output** | |
| ``Ease`` | Applies a smoothing function or AnimationCurve to the Tween. |
| ``Wave`` | Applies a wave function to the Tween. |
| ``From`` | Start and end values are reversed. Tween runs from end to start. |
| ``To`` | [Default] Tween runs from start to end. |
| **Looping** | |
| ``Loop`` | Tween will loop, with an optional number of loops. |
| ``Yoyo`` | Tween will reach the end value, then repeat from end to start. |
| ``YoyoLoop`` | Tween will yoyo and loop, with an optional number of loops. |
| ``Once`` | [Default] Tween will play once. |
| **Timing** | |
| ``Duration`` | Sets the duration, in seconds, of a single cycle. |
| ``Randomize`` | Starts the Tween from a random position in its timeline. |
| ``StartsAt`` | Starts the Tween from a set position in its timeline. |
| ``DelayBy`` | Starts the Tween after a certain amount of seconds have elapsed. |
| **Events** | |
| ``OnStart`` | Registers a function called when the Tween starts up. |
| ``OnUpdate`` | Registers a function called every frame while the Tween is running. |
| ``OnComplete`` | Registers a function called when the Tween ends. |
| **Cancel Behavior** | |
| ``RevertOnCancel`` | Tween will revert back to starting value if cancelled mid-execution. |
| ``ForceOnCancel`` | Tween will skip to its end value if cancelled mid-execution. |
| ``KeepOnCancel`` | [Default] Tween will keep its current value if cancelled mid-execution. |

### Utilities

#### Classes

| Class | Description |
| ----- | ----- |
| ``TransformState`` | Records the state of a transform's properties in the given space. Can also be reapplied to transforms. Useful for resetting a transform to its original state. |
| ``RectTransformState`` | Records the state of a RectTransform's properties. Can also be reapplied to RectTransforms. Useful for resetting a transform to its original state, particularly before or after an animation. |

#### Functions

| Function | Extension Method? | Description |
| -------- | ----------------- | ----------- |
| **TweenUtil** | | |
| ``TweenUtil.Evaluate`` | ``Curve`` | Evaluates an easing function for a given percentage. |
| ``TweenUtil.Lerp `` | | Returns an interpolation percentage, corrected for delta time. |
| ``TweenUtil.LerpDecay`` | | Returns a decay multiplier, corrected for delta time. |
| ``TweenUtil.SetDefaultLerpPeriod`` | | Sets the default lerp period for use in ``TweenUtil.Lerp`` and ``TweenUtil.LerpDecay`` |
| ``TweenUtil.SetDefaultLerpPeriodByFramerate`` | | Sets the default lerp period for use in ``TweenUtil.Lerp`` and ``TweenUtil.LerpDecay`` to ``1 / framerate``. |
| **TransformUtil** | | 
| ``TransformUtil.GetPosition`` | ``Transform`` | Returns the position of a transform in the given space along the given axes. |
| ``TransformUtil.GetPositionAxis`` | ``Transform`` | Returns the position of transform in the given space for the given single axis. |
| ``Transformutil.SetPosition`` | ``Transform`` | Sets the position of a transform in the given space along the given axes. |
| ``TransformUtil.GetScale`` | ``Transform`` | Returns the scale of a transform along the given axes. |
| ``TransformUtil.GetScaleAxis`` | ``Transform`` | Returns the scale of transform for the given single axis. |
| ``Transformutil.SetScale`` | ``Transform`` | Sets the scale of a transform along the given axes. |
| ``TransformUtil.GetRotation`` | ``Transform`` | Returns the euler rotation of a transform in the given space along the given axes. |
| ``TransformUtil.GetRotationAxis`` | ``Transform`` | Returns the euler rotation of transform in the given space for the given single axis. |
| ``Transformutil.SetRotation`` | ``Transform`` | Sets the euler rotation of a transform in the given space along the given axes. |
| ``TransformUtil.GetAnchorPos`` | ``RectTransform`` | Returns the anchored position of a RectTransform along the given axes. |
| ``TransformUtil.GetAnchorPosAxis`` | ``RectTransform`` | Returns the anchored position of a RectTransform for the given single axis. |
| ``Transformutil.SetAnchorPos`` | ``RectTransform`` | Sets the anchored position of a RectTransform along the given axes. |
| ``TransformUtil.GetSizeDelta`` | ``RectTransform`` | Returns the sizeDelta of a RectTransform along the given axes. |
| ``TransformUtil.GetSizeDeltaAxis`` | ``RectTransform`` | Returns the sizeDelta of a RectTransform for the given single axis. |
| ``Transformutil.SetSizeDelta`` | ``RectTransform`` | Sets the sizeDelta of a RectTransform along the given axes. |
| **VectorUtil** | | |
| ``VectorUtil.GetAxis`` | | Returns the value of a vector along the given axis. |
| ``VectorUtil.CopyFrom`` | | Copies values from one vector to another for the given axes. |
| ``VectorUtil.Add`` | | Adds one vector to another, applying an optional coefficient to the added vector. |
| ``VectorUtil.Subtract`` | | Subtracts one vector from another, applying an optional coefficient to the subtracted vector. |