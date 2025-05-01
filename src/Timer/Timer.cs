global using Engine = UnityEngine;
using GameObject = UnityEngine.GameObject;

using Timer.Timing;

namespace Timer;

public static class Timer
{
	public delegate void CycleCallback<EngineTime>(ITimer source, SystemTime sysTime, EngineTime engineTime)
		where EngineTime: IEngineTime;

	public static TickingTimer Start(
		GameObject root,
		CycleCallback<FixedTime>? fixedCycleCallback,
		CycleCallback<UnfixedTime>? unfixedCycleCallback
	) {
		var mb = root.AddComponent<MonoBehaviour>();
		mb._timer = new TimerInternal();
		mb._fixedCallback = fixedCycleCallback;
		mb._unfixedCallback = unfixedCycleCallback;
		return new(mb);
	}

	internal sealed class MonoBehaviour : Engine.MonoBehaviour
	{
		public TimerInternal _timer = default!;
		public CycleCallback<FixedTime>? _fixedCallback = null;
		public CycleCallback<UnfixedTime>? _unfixedCallback = null;

		void FixedUpate() {
			var sysTime = SystemTime.Now();
			var (cycles, seconds) = _timer._fixedSpan;
			_timer._fixedSpan = new(cycles + 1, seconds + Engine.Time.fixedDeltaTime);
			_fixedCallback?.Invoke(_timer, sysTime, FixedTime.Now());
		}

		void Update() {
			var sysTime = SystemTime.Now();
			var (cycles, seconds) = _timer._unfixedSpan;
			_timer._unfixedSpan = new(cycles + 1, seconds + Engine.Time.deltaTime);
			_unfixedCallback?.Invoke(_timer, SystemTime.Now(), UnfixedTime.Now());
		}
	}

	internal struct TimerInternal() : ITimer
	{
		public readonly CombinedTime _startTime = CombinedTime.Now();
		public FixedSpan _fixedSpan = new();
		public UnfixedSpan _unfixedSpan = new();

		readonly CombinedTime ITimer.StartTime => _startTime;
		readonly FixedSpan ITimer.FixedSpan => _fixedSpan;
		readonly UnfixedSpan ITimer.UnfixedSpan => _unfixedSpan;
	}

	public sealed class DisposedException : InvalidOperationException
	{
		internal DisposedException() : base("timer is finished or aborted") {}
	}
} 

public interface ITimer
{
	CombinedTime StartTime { get; }
	FixedSpan FixedSpan { get; }
	UnfixedSpan UnfixedSpan { get; }
}

public readonly struct TickingTimer : ITimer
{
	internal TickingTimer(Timer.MonoBehaviour mb) {
		_mb = mb;
		mb.enabled = true;
	}

	private readonly Timer.MonoBehaviour _mb;

	public CombinedTime StartTime => _mb._timer._startTime;

	public FixedSpan FixedSpan => _mb._timer._fixedSpan;
	public UnfixedSpan UnfixedSpan => _mb._timer._unfixedSpan;

	public PausedTimer Pause() => new(CombinedTime.Now(), 
		_mb ?? throw new Timer.DisposedException());
	
	public FinishedTimer Finish() => new(CombinedTime.Now(), 
		_mb ?? throw new Timer.DisposedException());

	public void Abort() => Engine.Object.Destroy(_mb);
}

public readonly struct PausedTimer : ITimer
{
	internal PausedTimer(CombinedTime pauseTime, Timer.MonoBehaviour mb) {
		_mb = mb;
		_mb.enabled = false;
		PauseTime = pauseTime;
	}

	private readonly Timer.MonoBehaviour _mb;

	public CombinedTime StartTime => _mb._timer._startTime;
	public CombinedTime PauseTime { get; }

	public FixedSpan FixedSpan => _mb._timer._fixedSpan;
	public UnfixedSpan UnfixedSpan => _mb._timer._unfixedSpan;

	public readonly record struct PauseSpan(CombinedTime PauseTime, CombinedTime ResumeTime);

	public (TickingTimer timer, PauseSpan span) Resume() {
		var resumeTime = CombinedTime.Now();
		return (
			new TickingTimer(_mb ?? throw new Timer.DisposedException()),
			new PauseSpan(PauseTime, resumeTime));
	}

	public FinishedTimer Finish() => new(CombinedTime.Now(), 
		_mb ?? throw new Timer.DisposedException());

	public void Abort() => Engine.Object.Destroy(_mb);
}

public readonly record struct FinishedTimer : ITimer
{
	internal FinishedTimer(CombinedTime finishTime, Timer.MonoBehaviour mb) {
		if (mb is null) throw new Timer.DisposedException();
		StartTime = mb._timer._startTime;
		FixedSpan = mb._timer._fixedSpan;
		UnfixedSpan = mb._timer._unfixedSpan;
		FinishTime = finishTime;
		Engine.Object.Destroy(mb);
	}

	public CombinedTime StartTime { get; }
	public FixedSpan FixedSpan { get; }
	public UnfixedSpan UnfixedSpan { get; }

	public CombinedTime FinishTime { get; }
}
