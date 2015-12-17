using System;

namespace VMM.Player.Reader
{
    public interface IBufferedObservable
    {
        event EventHandler<long> Buffed;

        long Length { get; }
    }
}
