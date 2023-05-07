namespace Thread_.NET.DAL.Entities.Abstract
{
    public abstract class Disreaction : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public bool IsDis { get; set; }
    }
}
