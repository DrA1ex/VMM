namespace VMM.Model
{
    public enum ChangeType
    {
        Deleted, Moved
    }

    public class MusicListChange
    {
        public ChangeType ChangeType { get; set; }
        public object Data { get; set; }
    }

    public class DeleteSong
    {
        public long SongId { get; set; }
    }

    public class MovedSong
    {
        public long SongId { get; set; }

        public long Position { get; set; }
    }

}
