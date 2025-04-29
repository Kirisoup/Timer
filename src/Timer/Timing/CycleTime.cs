namespace Timer.Timing;


public readonly record struct SystemTime(long Tick) 
{
	public static SystemTime Now() => new(DateTime.UtcNow.Ticks);
}

public interface IEngineTime
{
	float Second { get; }
}

public readonly record struct FixedTime(float Second) : IEngineTime
{
	public static FixedTime Now() => new(Engine.Time.fixedTime);
}

public readonly record struct UnfixedTime(float Second) : IEngineTime
{
	public static UnfixedTime Now() => new(Engine.Time.time);
}

public readonly record struct CombinedTime(SystemTime System, FixedTime Fixed, UnfixedTime Unfixed);

public interface IEngineSpan
{
	uint Cycles { get; } 
	float Seconds { get; }	
}

public readonly record struct FixedSpan(uint Cycles, float Seconds) : IEngineSpan;
public readonly record struct UnfixedSpan(uint Cycles, float Seconds) : IEngineSpan;
