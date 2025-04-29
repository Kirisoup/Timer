global using Engine = UnityEngine;
using GameObject = UnityEngine.GameObject;

using Timer.Timing;

namespace Timer;

public sealed class Timer : IDisposable
{
	public enum TimerState : byte { Waiting, Ticking, Paused, Finished, Aborted }

	public delegate void TimerCycleCallback<EngineTime>(Timer source, SystemTime sysTime, EngineTime engineTime)
		where EngineTime: IEngineTime;

	private sealed class MonoBehaviour : Engine.MonoBehaviour
	{
		public Timer timer = null!;

		void FixedUpate() => timer.FixedCycle();
		void Update() => timer.UnfixedCycle();
	}

	private Timer(MonoBehaviour mb, 
		TimerCycleCallback<FixedTime>? fcCallback, TimerCycleCallback<UnfixedTime>? ucCallback) {
		_mb = mb;
		_fcCallback = fcCallback;
		_ucCallback = ucCallback;
	}

	private readonly MonoBehaviour _mb;

	private readonly TimerCycleCallback<FixedTime>? _fcCallback;
	private readonly TimerCycleCallback<UnfixedTime>? _ucCallback;

	public CombinedTime StartTime { get; private set; }

	public FixedSpan FixedSpan { get; private set; }
	public UnfixedSpan UnfixedSpan { get; private set; }

	public TimerState State { get; private set; }

	public static Timer New(GameObject root,
		TimerCycleCallback<FixedTime>? fixedCycleCallback, TimerCycleCallback<UnfixedTime>? unfixedCycleCallback) 
	{
		var mb = root.AddComponent<MonoBehaviour>();
		return mb.timer = new(mb, fixedCycleCallback, unfixedCycleCallback);
	}

	private void FixedCycle() {
		var sysTime = SystemTime.Now();
		if (State is not TimerState.Ticking) return;
		FixedSpan = new(
			FixedSpan.Cycles + 1, 
			FixedSpan.Seconds + Engine.Time.fixedDeltaTime);
		_fcCallback?.Invoke(this, sysTime, FixedTime.Now());
	}

	private void UnfixedCycle() {
		var sysTime = SystemTime.Now();
		if (State is not TimerState.Ticking) return;
		UnfixedSpan = new(
			UnfixedSpan.Cycles + 1, 
			UnfixedSpan.Seconds + Engine.Time.deltaTime);
		_ucCallback?.Invoke(this, sysTime, UnfixedTime.Now());
	}

	public bool StartTimer(out CombinedTime timeStamp) {
		var sysTime = SystemTime.Now();
		timeStamp = default;
		if (State is not TimerState.Waiting) return false;
		State = TimerState.Ticking;
		StartTime = new(sysTime, FixedTime.Now(), UnfixedTime.Now());
		timeStamp = StartTime;
		return true; 
	}

	public bool Pause(out CombinedTime timeStamp) {
		var sysTime = SystemTime.Now();
		timeStamp = default;
		if (State is not TimerState.Ticking) return false;
		State = TimerState.Paused;
		timeStamp = new(sysTime, FixedTime.Now(), UnfixedTime.Now());
		return true;
	} 

	public bool Resume(out CombinedTime timeStamp) {
		var sysTime = SystemTime.Now();
		timeStamp = default;
		if (State is not TimerState.Paused) return false;
		State = TimerState.Paused;
		timeStamp = new(sysTime, FixedTime.Now(), UnfixedTime.Now());
		return true;
	}

	public bool Finish(out CombinedTime timeStamp) {
		var sysTime = SystemTime.Now();
		timeStamp = default;
		if (State is not TimerState.Ticking) return false;
		State = TimerState.Finished;
		timeStamp = new(sysTime, FixedTime.Now(), UnfixedTime.Now());
		return true;
	}

	public void Abort() {
		State = TimerState.Aborted;
		Engine.Object.Destroy(_mb);
	}

	void IDisposable.Dispose() => Abort();
}