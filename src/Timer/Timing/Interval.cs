namespace Timer.Timing;

public interface IEngineInterval<Self> where Self: IEngineInterval<Self>
{
	uint Cycles { get; } 
	float Seconds { get; }	
}

public readonly record struct FixedInterval(uint Cycles, float Seconds) : IEngineInterval<FixedInterval>;
public readonly record struct UnfixedInterval(uint Cycles, float Seconds) : IEngineInterval<FixedInterval>;