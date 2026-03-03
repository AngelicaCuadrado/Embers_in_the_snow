public interface IBaggable
{
    float Weight { get; }
    string PoolKey { get; }
    bool IsHeld { get; set; }
}