using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Threading.Tasks;
using Thread_.NET.BLL.Services.Abstract;
using Thread_.NET.Common.DTO.Like;
using Thread_.NET.DAL.Context;
using Thread_.NET.BLL.Hubs;
using Thread_.NET.DAL.Entities.Abstract;
using Thread_.NET.DAL.Entities;
using Thread_.NET.Common.DTO.Post;
using Thread_.NET.Common.DTO.User;
using Thread_.NET.Common.DTO.Comment;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System;
using Thread_.NET.BLL.Exceptions;
using Microsoft.Extensions.Logging;

namespace Thread_.NET.BLL.Services
{
    public sealed class LikeService : BaseService
    {
        private readonly IHubContext<PostHub> _likeHub;
        private readonly ILogger<LikeService> _logger;
        public LikeService(ThreadContext context, ILogger<LikeService> logger, IMapper mapper, IHubContext<PostHub> likeHub) : base(context, mapper)
        {

            _likeHub = likeHub;
            _logger = logger;

        }

        public async Task LikePost(NewReactionDTO reaction)
        {
            var likes = _context.PostReactions.Where(x => x.UserId == reaction.UserId && x.PostId == reaction.EntityId);
            var dises = _context.PostDisreactions.Where(x => x.UserId == reaction.UserId && x.PostId == reaction.EntityId);

            var post = await _context.Posts.FindAsync(reaction.EntityId);
            var PostDTO = _mapper.Map<PostDTO>(post);
            

            var user = await _context.Users.FindAsync(reaction.UserId);// user who liekd the post
            var userDTO = _mapper.Map<UserDTO>(user);

            if (likes.Any())
            {
                _context.PostReactions.RemoveRange(likes);
                await _context.SaveChangesAsync();

                if (post.AuthorId != reaction.UserId) { 
                    await _likeHub.Clients.Group(post.AuthorId.ToString()).SendAsync("RemoveLikedPostUser", reaction.UserId, PostDTO);
                }
                return;
            }

            if (dises.Any()) 
            {
                _context.PostDisreactions.RemoveRange(dises);
                await _context.SaveChangesAsync();
                if (post.AuthorId != reaction.UserId)
                {
                    await _likeHub.Clients.Group(post.AuthorId.ToString()).SendAsync("RemoveDisedPostUser", reaction.UserId, PostDTO);
                }
            }

            _context.PostReactions.Add(new DAL.Entities.PostReaction
            {
                PostId = reaction.EntityId,
                IsLike = reaction.IsLike,
                UserId = reaction.UserId
            });

            
            await _context.SaveChangesAsync();
            
            
            
            if (post.AuthorId != reaction.UserId)
            {
                await _likeHub.Clients.Group(post.AuthorId.ToString()).SendAsync("LikedPostUser", PostDTO, userDTO);

                var userEntity = await _context.Users.FindAsync(post.AuthorId);
                
                
                await SendEmailAsync(userEntity.Email, userDTO.UserName);
                
            } 
            
        }

        public async Task<bool> SendEmailAsync(string email, string userName)
        {
            

            MailAddress from = new("vanyep3@gmail.com", "Tom");
            MailAddress to = new(email);
            MailMessage m = new(from, to)
            {
                Subject = $"{userName} liked your post",
                Body = $"{userName} just liked your post ",
                IsBodyHtml = true
            };
            SmtpClient smtp = new("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("vanyep3@gmail.com", "dkfejrfdsexuvwuo"),
                EnableSsl = true
            };
            await smtp.SendMailAsync(m);

            return true;
           
        }

        public async Task LikeComment(NewReactionDTO reaction)
        {
            var likes = _context.CommentReactions.Where(x => x.UserId == reaction.UserId && x.CommentId == reaction.EntityId);
            var dises = _context.CommentDisreactions.Where(x => x.UserId == reaction.UserId && x.CommentId == reaction.EntityId);

            var comment = await _context.Comments.FindAsync(reaction.EntityId);
            var commentDTO = _mapper.Map<CommentDTO>(comment);

            var user = await _context.Users.FindAsync(reaction.UserId);
            var userDTO = _mapper.Map<UserDTO>(user);

            if (likes.Any())
            {
                _context.CommentReactions.RemoveRange(likes);
                await _context.SaveChangesAsync();
                if (comment.AuthorId != reaction.UserId)
                {
                    await _likeHub.Clients.Group(comment.AuthorId.ToString()).SendAsync("RemoveLikedCommentUser", reaction.UserId, commentDTO, comment.PostId);
                }
                return;
            }

            if (dises.Any())
            {
                _context.CommentDisreactions.RemoveRange(dises);
                if (comment.AuthorId != reaction.UserId)
                {
                    await _likeHub.Clients.Group(comment.AuthorId.ToString()).SendAsync("RemoveDisedCommentUser", reaction.UserId, commentDTO, comment.PostId);
                }
                       
            }
            
               
            _context.CommentReactions.Add(new DAL.Entities.CommentReaction
            {
                CommentId = reaction.EntityId,
                IsLike = reaction.IsLike,
                UserId = reaction.UserId
            });

            await _context.SaveChangesAsync();
            if (comment.AuthorId != reaction.UserId)
            {
                await _likeHub.Clients.Group(comment.AuthorId.ToString()).SendAsync("LikedCommentUser", commentDTO, comment.PostId, userDTO);
            }
            
        }
    }
}

