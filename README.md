# BeauRoutine

_Documentation is currently under construction._

## About
BeauRoutine is a coroutine framework for Unity3D. Intended as an
alternative to Unity's existing coroutine implementation, BeauRoutines
are a fast, powerful, and flexible way of sequencing your logic.

BeauRoutine also includes a powerful coroutine-driven tweening system.
Fast programmatic animations can be written and configured quickly
for rapid prototyping as well as visual polish.

## Usage

### Running a Routine
To run a BeauRoutine, call
```csharp
Routine.Start( MyCoroutine() );
```
This will begin executing the given coroutine.

### Hosting Routines
Much like Unity's default Coroutines only execute while their host object is alive, a BeauRoutine can be given
a host to bind its lifetime to that of the host. To bind a host to a BeauRoutine, call
```csharp
Routine.Start( this, MyCoroutine() );
```
This will bind the given object as a host. As long as the host is alive, the BeauRoutine will be able to execute.
If the host is deactivated, the BeauRoutine will pause until the host is again active.  Once the host is destroyed,
any BeauRoutines it hosted will be halted.

### Expanded Coroutine Syntax
BeauRoutines allow for more flexibility when writing your coroutine logic.

```csharp
public IEnumerator MyCustomBeauRoutine()
{
	DoAThing();
    
    // All three of these statements will wait
    // one second before resuming execution.
	yield return 1;
	yield return 1.0f;
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

### Combine and Race
``Routine.Combine`` is a coroutine that executes multiple coroutines concurrently. This can be incredibly useful for programmatic animations or syncing operations between objects. It concludes when all the given coroutines have either concluded or halted.

``Routine.Race`` is similar, but it concludes when one of the given coroutines has concluded or halted. None of the other given coroutines are allowed to execute after one has concluded.

```csharp
public IEnumerator MyCombineRoutine()
{
	// This will execute Operation1 and Operation2 concurrently.
    // Both debug messages will be logged.
	yield return Routine.Combine( Operation1(), Operation2() );
    
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
	Routine.Start(this, Sequence.Create(FirstBeauRoutine())
    	.Wait(5.0f)
        .Then(() => { Destroy(gameObject); })
        );
}

public IEnumerator FirstBeauRoutine()
{
	...
}
```

### Tweens
_This section is still under construction._