using Timer.Timing;

namespace Timer;

public interface ITimer
{
	FixedInterval FixedDuration { get; }
	UnfixedInterval UnfixedDuration { get; }
}
