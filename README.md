# BeauRoutine

## About
BeauRoutine is a coroutine framework for Unity3D. Intended as an alternative to Unity's existing coroutine implementation, BeauRoutines are a fast, powerful, and flexible way of sequencing your logic.

BeauRoutine also includes a powerful coroutine-driven tweening system. Fast programmatic animations can be written and configured quickly for rapid prototyping as well as visual polish.

## Basic Usage

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
}

public IEnumerator MyOtherBeauRoutine()
{
	...
}
```

### Handles
Similar to Unity's ``Coroutine`` object, the ``Routine`` object returned after calling many BeauRoutine functions, including ``Routine.Start``, is a handle reference to a BeauRoutine.  You can use that to modify the BeauRoutine as long as it's running. It's also a safe reference - if the BeauRoutine it's pointing at expires, you can still call functions on the handle without fear of either exceptions or unintentionally modifying other active BeauRoutines.

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
A ``Sequence`` is a customizable sequence of coroutines, function calls, and delays. Instead of creating a new function for every combination of coroutines, you can instead construct a coroutine out of common parts using a simple fluent interface.

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

### Tweens as coroutines
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
audioSource.VolumeTo( 0.0f, 2.0f );
```

### Mirrored Tweens

Yoyo Tweens have a property called ``Mirrored``, which affects how easing functions are applied when the Tween is reversed.  Normally, reversed Tweens apply their easing functions as if the Tween was not reversed.  An EaseOut will have the same progression over time from end to start as it did from start to end.  A mirrored Tween will reverse the easing function as well.

Put in other terms, a normal Tween that starts slow and speeds up animating from start to end will also start slow and speed up when animating back to start.  The same Tween, mirrored, will start fast and slow down when animating back to start.

## Advanced Features

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
* ``Routine.TimeScale`` (alias for ``Time.timeScale``)
* Time scale for a BeauRoutine
* Per-object time scales (see RoutineIdentity)
* Per-group time scales (see Groups)

You can calculate the total time scale that will be applied to BeauRoutines on a given object by calling ``Routine.CalculateTimeScale``. This is the accumulation of ``Routine.TimeScale``, the per-object time scale, and the per-group time scale.

### Routine Identity

The ``RoutineIdentity`` component can be attached to any GameObject to apply a per-object time scale.  Note that this will only apply to BeauRoutines that are started after attaching the RoutineIdentity - it will not retroactively apply time scaling on BeauRoutines run before it was attached.

You can find the RoutineIdentity for a given object by calling ``RoutineIdentity.Find``.  If you want to ensure the object has a RoutineIdentity, call ``RoutineIdentity.Require``, which will lazily instantiate an instance on the given object.

Per-object time scale is controlled by the ``TimeScale`` property.

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
float groupTimeScale = Routine.GetGroupTimeScale( (int)ObjectGroup.Gameplay );
Routine.SetGroupTimeScale( (int)ObjectGroup.Gameplay, 0.5f );
Routine.ResetGroupTimeScale( (int)ObjectGroup.Gameplay ); // Resetting resets to 1.

// While a single group is referenced by index, the PauseGroups and ResumeGroups
// functions take in a bitmask of all affected groups.
// You'll need to transform the group index into a bitmask.
int groupMask = 2 << (int)ObjectGroup.Gameplay;
groupMask |= 2 << (int)ObjectGroup.UI;

// You can pause and resume multiple groups at a time.
Routine.PauseGroups( groupMask );
Routine.ResumeGroups( groupMask );
```

It's important to note that a BeauRoutine can only belong to a single group.  This is to prevent any unwanted timescaling behavior.

### Precise Timing

Normally, yielding into a nested coroutine will wait until the next frame to begin executing the nested coroutine. BeauRoutine provides a way of immediately entering and executing a nested coroutine by wrapping your coroutine in a ``Routine.Immediately`` call. This is useful if you need to time your coroutines in a very precise manner.

```csharp
IEnumerator RootCoroutine()
{
	Debug.Log("This is the RootCoroutine log");
	yield return Routine.Immediately(NestedCoroutine());
}

IEnumerator NestedCoroutine()
{
	Debug.Log("This will log on the same frame as the RootCoroutine log");
	...
}
```

There is a similar frame delay when exiting a nested coroutine. This can be avoided by yielding ``Routine.Command.BreakAndResume`` instead of letting your coroutine exit normally. This will immediately exit the coroutine and resume execution on the coroutine that yielded into it.

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

Be careful when using these features. Immediate execution offers more precise timing but runs the risk of executing too much in a single frame if overused.  

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
		..
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
    Routine.Start( CalculateHashAsyncImpl( future, str ) );
    
    // This future represents the eventual result of CalculateHashAsyncImpl
    return future;
}

// We need access to the Future in order to complete it
// We can pass it in as an argument to accomplish this
IEnumerator CalculateHashAsyncImpl( Future<int> future, string str )
{
	// Some artificial delay to make this asynchronous
	yield return 0.5f;
    
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
}
```

### Debugger

BeauRoutine includes a simple profiler and debugger, found under the menu item ``Window/BeauRoutine Debugger``.

The ``STATS`` page displays the number of BeauRoutines currently running, the maximum you've had running at the same time, and the total you can run concurrently without allocating more memory.  It also keeps track of the average time it takes to execute all the currently running BeauRoutines, and maintains a snapshot of what was running when the last maximum was recorded.

The ``OPTIONS`` page displays the current time scale, as well as options for resetting it, doubling it, or halving it. It also contains a button enabling or disabling the snapshot feature on the STATS page.

The ``DETAILS`` page displays all the currently running BeauRoutines in a list.  From here, you can pause, resume, or stop any of them, as well as rename them or set their time scale.  Any Combine or Race routines are presented in a hierarchical format.

## Tips and Tricks

#### Stopping by ``Routine`` handles
Stop BeauRoutines by maintaining a ``Routine`` handle. Stopping by string name is a comparatively much more expensive process.

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

#### Using ``Routine`` handles as slots
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

## Reference

### Routine Utilities

| Function | Description |
| -------- | ----------- |
| **Start Shortcuts** | |
| ``Routine.StartDelay`` | Starts a BeauRoutine that calls a function or executes a coroutine after a certain number of seconds. |
| ``Routine.StartLoop`` | Starts a BeauRoutine that calls a function every frame. |
| ``Routine.StartLoopRoutine`` | Starts a BeauRoutine that executes a coroutine in a never-ending loop. |
| ``Routine.StartCall`` | Starts a BeauRoutine that calls a function at the end of the frame. |
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
| **Execution** | |
| ``Routine.Delay`` | Calls a function after the specified number of seconds. |
| ``Routine.Call`` | Calls a function at the end of the frame. |
| ``Routine.PerSecond`` | Calls a function the given number of times per second. |
| ``Routine.Combine`` | Runs multiple coroutines concurrently, and completes all. |
| ``Routine.Race`` | Runs multiple coroutines concurrently, and stops when one expires. |
| ``Routine.Immediately `` | Executes the given coroutine immediately after yielding into it. |

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
| ---- | ------ | ---------- |
| Transform | Position | ``MoveTo``, ``MoveToWithSpeed`` |
| | Scale | ``ScaleTo`` |
| | Rotation | ``RotateTo``, ``LookAt`` |
| | Transform | ``TransformTo`` |
| RectTransform | Anchored Position | ``AnchorPosTo`` |
| | Anchors | ``AnchorTo`` |
| | Size Delta | ``SizeDeltaTo`` |
| | Pivot | ``PivotTo`` |
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