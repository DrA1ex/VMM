using System;

namespace VMM.Player.Exceptions
{
    public enum EngineError
    {
        WrongFileFormat,
        NetworkError,
        Unknown
    }

    public class MusicPlayerEngineException : Exception
    {
        public EngineError Error { get;}

        public MusicPlayerEngineException(EngineError error, string message, Exception ex = null)
            : base(message, ex)
        {
            Error = error;
        }

        public MusicPlayerEngineException(EngineError error, Exception ex = null)
            : base(null, ex)
        {
            Error = error;
        }
    }
}
