using Timer.Timing;

namespace Timer;

public readonly struct PausedTimer : ITimer
{
	internal PausedTimer(CombinedTime pauseTime, Timer.MonoBehaviour mb) {
		_mb = mb;
		_mb.enabled = false;
		PauseTime = pauseTime;
	}

	private readonly Timer.MonoBehaviour _mb;

	public CombinedTime PauseTime { get; }
	public FixedInterval FixedDuration => _mb._timer._fixedSpan;
	public UnfixedInterval UnfixedDuration => _mb._timer._unfixedSpan;

	public readonly record struct PauseSpan(CombinedTime PauseTime, CombinedTime ResumeTime);

	public Timer.OperationResult<TickingTimer> Resume() {
		var resumeTime = CombinedTime.Now();
		return new(
			LifeTime: new(PauseTime, resumeTime),
			NewTimer: new(resumeTime, _mb ?? throw new Timer.DisposedException()));
	}

	public Timer.OperationResult<FinishedTimer> Finish() {
		var finishTime = CombinedTime.Now();
		return new(
			LifeTime: new(PauseTime, finishTime), 
			NewTimer: new(_mb ?? throw new Timer.DisposedException()));
	}

	public void Abort() => Engine.Object.Destroy(_mb);
}
