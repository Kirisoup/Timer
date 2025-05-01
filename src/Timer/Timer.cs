global using Engine = UnityEngine;
using GameObject = UnityEngine.GameObject;
using Timer.Timing;

namespace Timer;

public static class Timer
{
	public delegate void CycleCallback<Time>(ITimer source, SystemTime sysTime, Time engineTime)
		where Time: IEngineTime<Time>;

	public readonly record struct OperationResult<Timer>(
		TimeSpan<CombinedTime> LifeTime, 
		Timer NewTimer) 
		where Timer : ITimer;

	public static TickingTimer NewTicking(
		GameObject root,
		CycleCallback<FixedTime>? fixedCycle = null,
		CycleCallback<UnfixedTime>? unfixedCycle = null
	) => 
		new(CombinedTime.Now(), Instantiate(root, fixedCycle, unfixedCycle));

	public static PausedTimer NewPaused(
		GameObject root,
		CycleCallback<FixedTime>? fixedCycle = null,
		CycleCallback<UnfixedTime>? unfixedCycle = null
	) => 
		new(CombinedTime.Now(), Instantiate(root, fixedCycle, unfixedCycle));

	private static MonoBehaviour Instantiate(
		GameObject root,
		CycleCallback<FixedTime>? fixedCycleCallback,
		CycleCallback<UnfixedTime>? unfixedCycleCallback)
	{
		var mb = root.AddComponent<MonoBehaviour>();
		mb._timer = new TimerInternal();
		mb._fixedCallback = fixedCycleCallback;
		mb._unfixedCallback = unfixedCycleCallback;
		return mb;
	}

	internal sealed class MonoBehaviour : Engine.MonoBehaviour
	{
		public TimerInternal _timer = default!;
		public CycleCallback<FixedTime>? _fixedCallback = null;
		public CycleCallback<UnfixedTime>? _unfixedCallback = null;

		private MonoBehaviour() {}

		private void FixedUpate() {
			var sysTime = SystemTime.Now();
			var (cycles, seconds) = _timer._fixedSpan;
			_timer._fixedSpan = new(cycles + 1, seconds + Engine.Time.fixedDeltaTime);
			_fixedCallback?.Invoke(_timer, sysTime, FixedTime.Now());
		}

		private void Update() {
			var sysTime = SystemTime.Now();
			var (cycles, seconds) = _timer._unfixedSpan;
			_timer._unfixedSpan = new(cycles + 1, seconds + Engine.Time.deltaTime);
			_unfixedCallback?.Invoke(_timer, sysTime, UnfixedTime.Now());
		}
	}

	internal struct TimerInternal : ITimer
	{
		public FixedInterval _fixedSpan;
		public UnfixedInterval _unfixedSpan;

		readonly FixedInterval ITimer.FixedDuration => _fixedSpan;
		readonly UnfixedInterval ITimer.UnfixedDuration => _unfixedSpan;
	}

	public sealed class DisposedException : InvalidOperationException
	{
		internal DisposedException() : base("timer is finished or aborted") {}
	}
} 
