namespace Timer.Timing;

public readonly record struct TimeSpan<Time>(Time Start, Time End) where Time: ITime<Time>;
