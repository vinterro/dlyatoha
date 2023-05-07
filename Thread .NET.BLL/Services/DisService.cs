using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Thread_.NET.BLL.Hubs;
using Thread_.NET.BLL.Services.Abstract;
using Thread_.NET.Common.DTO.Dis;
using Thread_.NET.Common.DTO.User;
using Thread_.NET.Common.DTO.Post;
using Thread_.NET.DAL.Context;
using Thread_.NET.DAL.Entities.Abstract;
using Thread_.NET.DAL.Entities;
using Thread_.NET.Common.DTO.Comment;

namespace Thread_.NET.BLL.Services
{
    public sealed class DisService : BaseService


    {

        private readonly IHubContext<PostHub> _disHub;
        public DisService(ThreadContext context, IMapper mapper, IHubContext<PostHub> disHub) : base(context, mapper) {

            _disHub = disHub;
        }

        public async Task DisPost(NewDisreactionDTO disreaction)
        {
            var dises = _context.PostDisreactions.Where(x => x.UserId == disreaction.UserId && x.PostId == disreaction.EntityId);
            var likes = _context.PostReactions.Where(x => x.UserId == disreaction.UserId && x.PostId == disreaction.EntityId);

            var post = await _context.Posts.FindAsync(disreaction.EntityId);
            var PostDTO = _mapper.Map<PostDTO>(post);

            var user = await _context.Users.FindAsync(disreaction.UserId);
            var userDTO = _mapper.Map<UserDTO>(user);

            if (dises.Any())
            {
                _context.PostDisreactions.RemoveRange(dises);
                await _context.SaveChangesAsync();
                if (post.AuthorId != disreaction.UserId)
                {
                    await _disHub.Clients.Group(post.AuthorId.ToString()).SendAsync("RemoveDisedPostUser", disreaction.UserId, PostDTO);
                }

                return;
            }


            if (likes.Any())
            {
                _context.PostReactions.RemoveRange(likes);
                await _context.SaveChangesAsync();
                if (post.AuthorId != disreaction.UserId)
                {
                    await _disHub.Clients.Group(post.AuthorId.ToString()).SendAsync("RemoveLikedPostUser", disreaction.UserId, PostDTO);
                }
            }

            _context.PostDisreactions.Add(new DAL.Entities.PostDisreaction
            {
                PostId = disreaction.EntityId,
                IsDis = disreaction.IsDis,
                UserId = disreaction.UserId
            });


            await _context.SaveChangesAsync();
            if (post.AuthorId != disreaction.UserId)
            {
                await _disHub.Clients.Group(post.AuthorId.ToString()).SendAsync("DisedPostUser", PostDTO, userDTO);
            }
        }


        public async Task DisComment(NewDisreactionDTO disreaction)
        {
            var dises = _context.CommentDisreactions.Where(x => x.UserId == disreaction.UserId && x.CommentId == disreaction.EntityId);
            var likes = _context.CommentReactions.Where(x => x.UserId == disreaction.UserId && x.CommentId == disreaction.EntityId);

            var comment = await _context.Comments.FindAsync(disreaction.EntityId);
            var commentDTO = _mapper.Map<CommentDTO>(comment);

            var user = await _context.Users.FindAsync(disreaction.UserId);
            var userDTO = _mapper.Map<UserDTO>(user);

            if (dises.Any())
            {
                _context.CommentDisreactions.RemoveRange(dises);
                await _context.SaveChangesAsync();
                if (comment.AuthorId != disreaction.UserId)
                {
                    await _disHub.Clients.Group(comment.AuthorId.ToString()).SendAsync("RemoveDisedCommentUser", disreaction.UserId, commentDTO, comment.PostId);
                }
                return;
            }


            if (likes.Any())
            {
                _context.CommentReactions.RemoveRange(likes);
                if (comment.AuthorId != disreaction.UserId)
                {
                    await _disHub.Clients.Group(comment.AuthorId.ToString()).SendAsync("RemoveLikedCommentUser", disreaction.UserId, commentDTO, comment.PostId);
                }
            }

            _context.CommentDisreactions.Add(new DAL.Entities.CommentDisreaction
            {
                CommentId = disreaction.EntityId,
                IsDis = disreaction.IsDis,
                UserId = disreaction.UserId
            });

            await _context.SaveChangesAsync();
            if (comment.AuthorId != disreaction.UserId)
            {
                await _disHub.Clients.Group(comment.AuthorId.ToString()).SendAsync("DisedCommentUser", commentDTO, comment.PostId, userDTO);
            }

        }
    }
}



