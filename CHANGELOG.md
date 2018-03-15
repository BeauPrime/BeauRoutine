### Version 0.9.0
**15 March 2018**

First numbered version. All Unity coroutine features are now implemented and supported, and the documentation has been updated to be more thorough.

#### Features

* Added ability to change a BeauRoutine's phase. Previous versions would always execute during ``LateUpdate``, but this can now be switched to ``FixedUpdate``, ``Update``, ``LateUpdate``, or ``Manual``
* Added ability to attach an exception handler to a BeauRoutine with ``OnException``
* Yielding a ``WaitForEndOfFrame`` or ``WaitForFixedUpdate`` now corresponds to the same behavior in Unity coroutines
* Added extension methods to set update functions for a MonoBehaviour
* Added ability to manually update BeauRoutines
* Added ability to report progress on a Future
* Added ability to manually add a time delay to a BeauRoutine with ``DelayBy``

#### Fixes

* Fixed ``TweenUtil.LerpDecay(float, float, float)`` using the wrong parameter for part of the calculation
* Custom IEnumerators now throw a ``NotSupportedException`` instead of a ``NotImplementedException`` to match language specification

#### Improvements

* Expanded ``IFuture`` interface to incorporate more shared functionality
* Added ``UnityWebRequest`` support to all ``Future.Download`` utilities
* General performance improvements

#### Breaking Changes

* Renamed ``RectTransformState.State()`` to ``RectTransformState.Create()``

#### Documentation

* Documented update phase feature
* Added Technical Notes section
* Expanded table of contents
* Reorganized Advanced Features section
* Added more functions to Reference section
* Added two more examples to Tips and Tricks section