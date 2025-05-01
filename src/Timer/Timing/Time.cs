namespace Timer.Timing;

/// <summary>
/// marker interface for an instant in time
/// </summary>

// use generic Self to force static dispatch and forbid interchangeability
public interface ITime<Self> where Self: ITime<Self> 
{
	// static Self Now();
}

/// <summary>
/// time instant in the UnityEngine's abstract time system 
/// </summary>
public interface IEngineTime<Self> : ITime<Self> where Self: IEngineTime<Self>
{
	float Second { get; }
}

public readonly record struct SystemTime(long Tick) : ITime<SystemTime>
{
	public static SystemTime Now() => new(DateTime.UtcNow.Ticks);
}

public readonly record struct FixedTime(float Second) : IEngineTime<FixedTime>
{
	public static FixedTime Now() => new(Engine.Time.fixedTime);
}

public readonly record struct UnfixedTime(float Second) : IEngineTime<UnfixedTime>
{
	public static UnfixedTime Now() => new(Engine.Time.time);
}

public readonly record struct CombinedTime(
	SystemTime System, FixedTime Fixed, UnfixedTime Unfixed) 
	: ITime<CombinedTime>
{
	public static CombinedTime Now() => new(SystemTime.Now(), FixedTime.Now(), UnfixedTime.Now());
}
