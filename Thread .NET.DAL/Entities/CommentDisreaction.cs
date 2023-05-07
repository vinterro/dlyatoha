using Thread_.NET.DAL.Entities.Abstract;

namespace Thread_.NET.DAL.Entities
{
    public sealed class CommentDisreaction : Disreaction
    {
        public int CommentId { get; set; }
        public Comment Comment { get; set; }
    }
}
