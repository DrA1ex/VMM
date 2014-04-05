namespace VMM.Model
{
    public enum ChangeType
    {
        Deleted, Edited
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

}
