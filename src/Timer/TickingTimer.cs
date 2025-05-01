using Timer.Timing;

namespace Timer;

public readonly struct TickingTimer : ITimer
{
	internal TickingTimer(CombinedTime startTime, Timer.MonoBehaviour mb) {
		_mb = mb;
		mb.enabled = true;
		StartTime = startTime;
	}

	private readonly Timer.MonoBehaviour _mb;

	public CombinedTime StartTime { get; }
	public FixedInterval FixedDuration => _mb._timer._fixedSpan;
	public UnfixedInterval UnfixedDuration => _mb._timer._unfixedSpan;

	public Timer.OperationResult<PausedTimer> Pause() {
		var pauseTime = CombinedTime.Now();
		return new(
			LifeTime: new(StartTime, pauseTime), 
			NewTimer: new(pauseTime, _mb ?? throw new Timer.DisposedException()));
	}

	public Timer.OperationResult<FinishedTimer> Finish() {
		var finishTime = CombinedTime.Now();
		return new(
			LifeTime: new(StartTime, finishTime), 
			NewTimer: new(_mb ?? throw new Timer.DisposedException()));
	}

	public void Abort() => Engine.Object.Destroy(_mb);
}
