using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Thread_.NET.BLL.Exceptions;
using Thread_.NET.BLL.Hubs;
using Thread_.NET.BLL.Services.Abstract;
using Thread_.NET.Common.DTO.Post;
using Thread_.NET.DAL.Context;
using Thread_.NET.DAL.Entities;

namespace Thread_.NET.BLL.Services
{
    public sealed class PostService : BaseService
    {
        private readonly IHubContext<PostHub> _postHub;

        public PostService(ThreadContext context, IMapper mapper, IHubContext<PostHub> postHub) : base(context, mapper)
        {
            _postHub = postHub;
        }

        public async Task<ICollection<PostDTO>> GetAllPosts()
        {
            var posts = await _context.Posts
                .Include(post => post.Author)
                    .ThenInclude(author => author.Avatar)
                .Include(post => post.Preview)

                .Include(post => post.Reactions)
                    .ThenInclude(reaction => reaction.User)
                .Include(post => post.Disreactions)
                    .ThenInclude(disreaction => disreaction.User)

                .Include(post => post.Comments)
                    .ThenInclude(comment => comment.Reactions)
                .Include(post => post.Comments)
                    .ThenInclude(comment => comment.Disreactions)

                .Include(post => post.Comments)
                    .ThenInclude(comment => comment.Author)
                .OrderByDescending(post => post.CreatedAt)
                .ToListAsync();

            return _mapper.Map<ICollection<PostDTO>>(posts);
        }

        public async Task<ICollection<PostDTO>> GetAllPosts(int userId)
        {
            var posts = await _context.Posts
                .Include(post => post.Author)
                    .ThenInclude(author => author.Avatar)
                .Include(post => post.Preview)
                .Include(post => post.Comments)
                    .ThenInclude(comment => comment.Author)
                .Where(p => p.AuthorId == userId) // Filter here
                .ToListAsync();

            return _mapper.Map<ICollection<PostDTO>>(posts);
        }

        public async Task<ICollection<PostDTO>> GetPost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
                

            return _mapper.Map<ICollection<PostDTO>>(post);
        }

        public async Task<bool> SendEmailAsync(string email, int postId, string userName)
        {

            MailAddress from = new("vanyep3@gmail.com", "Tom");
            MailAddress to = new(email);
            MailMessage m = new(from, to)
            {
                Subject = $"{userName} shared post",
                Body = $"Click on this link to see the post: http://localhost:4200/posts/{postId}",
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



        public async Task<PostDTO> CreatePost(PostCreateDTO postDto)
        {
            var postEntity = _mapper.Map<Post>(postDto);

            _context.Posts.Add(postEntity);
            await _context.SaveChangesAsync();

            var createdPost = await _context.Posts
                .Include(post => post.Author)
					.ThenInclude(author => author.Avatar)
                .FirstAsync(post => post.Id == postEntity.Id);

            var createdPostDTO = _mapper.Map<PostDTO>(createdPost);
            await _postHub.Clients.All.SendAsync("NewPost", createdPostDTO);

            return createdPostDTO;
        }

        public async Task<PostDTO> EditPost(PostDTO editedPostDTO)
        {
            var postToUpdate = await _context.Posts.FindAsync(editedPostDTO.Id);
            if( postToUpdate != null)
            {
                postToUpdate.Body = editedPostDTO.Body;

                postToUpdate.Preview = new Image { URL = editedPostDTO.PreviewImage };





                _context.Posts.Update(postToUpdate);


                await _context.SaveChangesAsync();
            }
           


            await _postHub.Clients.All.SendAsync("EditPost", editedPostDTO);

            return editedPostDTO;
        }

        public async Task<int> DeletePost(int postId)
        {

            var postToDelete = await _context.Posts.FindAsync(postId);

            if (postToDelete == null)
            {
                return postId;
            }

            var postReactionsToDelete = _context.PostReactions.Where(cr => cr.PostId == postId);
            var postDisreactionsToDelete = _context.PostDisreactions.Where(cd => cd.PostId == postId);

            _context.PostReactions.RemoveRange(postReactionsToDelete);
            _context.PostDisreactions.RemoveRange(postDisreactionsToDelete);


            await _context.SaveChangesAsync();


            var commentsToDelete = await _context.Comments.Where(c => c.PostId == postId).ToListAsync();

            foreach (var comment in commentsToDelete)
            {
                var commentReactionsToDelete = _context.CommentReactions.Where(cr => cr.CommentId == comment.Id);
                var commentDisreactionsToDelete = _context.CommentDisreactions.Where(cd => cd.CommentId == comment.Id);

                _context.CommentReactions.RemoveRange(commentReactionsToDelete);
                _context.CommentDisreactions.RemoveRange(commentDisreactionsToDelete);

                await _context.SaveChangesAsync();
            }

            _context.Comments.RemoveRange(commentsToDelete);




            await _context.SaveChangesAsync();



            _context.Posts.Remove(postToDelete);
            await _context.SaveChangesAsync();


            await _postHub.Clients.All.SendAsync("DeletePost", postId);

            return postId;
        }


    }
}
