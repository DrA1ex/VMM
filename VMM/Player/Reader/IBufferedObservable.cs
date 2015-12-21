using System;

namespace VMM.Player.Reader
{
    public interface IBufferedObservable
    {
        event EventHandler<long> Buffed;

        event EventHandler<Exception> BufferingFailed;

        long Length { get; }

        long BufferedBytes { get; }

        byte[] GetBuffer { get; }
    }
}
