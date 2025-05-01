using Timer.Timing;

namespace Timer;

public readonly record struct FinishedTimer : ITimer
{
	internal FinishedTimer(Timer.MonoBehaviour mb) {
		if (mb is null) throw new Timer.DisposedException();
		FixedDuration = mb._timer._fixedSpan;
		UnfixedDuration = mb._timer._unfixedSpan;
		Engine.Object.Destroy(mb);
	}

	public FixedInterval FixedDuration { get; }
	public UnfixedInterval UnfixedDuration { get; }
}
