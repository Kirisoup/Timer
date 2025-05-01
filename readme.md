Timer
=======

All purpose general timer for unity game speedrunning. This is intended to be used as a library, 
	so no extra feature provided;

Usage:
--------

```cs
var tickingTimer = Timer.NewTicking(gameObject);
```

This creates a `TickingTimer`, running both on unity's FixedUpdate and Update cycle.


You can pass in extra callback expressions with signature
	`(ITimer, Timing.SystemTime, Timing.FixedTime) -> void` that is invoked 
	every fixedUpdate, and
	`(..., Timing.UnfixedTime) -> void` that is invoked every update.


The GameObject is needed for the timer to attach it's behaviour.

To pause a `TickingTimer`, you can run:

```cs
var (lifetime, pausedTimer) = tickingTimer.Pause();
```

which returns a `TimeSpan<CombinedTime>` indicating the lifetime of the previous state, 
	and a `PausedTimer`.


Similarly, you can resume a `PausedTimer` using:

```cs
var (lifetime, tickingTimer) = pausedTimer.Resume();
```

You can also directly obtain a new `PausedTimer` with:

```cs
var pausedTimer = Timer.NewPaused(gameObject);
```

---

Both `TickingTimer` and `PausedTimer` holds a reference to a timer monobehaviour, which allows them
	to be paused and resumed at any time.

When you are done with the timer, you can call either:

```cs
timer.Abort();
```

if you don't care about the result, or more usefully:

```cs
var (lifetime, finishedTimer) = timer.Finish();
```

Both operations destroys the internal monobehaviour. But `timer.Finish` extracts the final result
	before destroying. 

All the timer variations implements interface `ITimer`, which provides a 
	`Timing.FixedInterval FixedDuration` property and a `Timing.UnfixedInterval UnfixedDuration`.