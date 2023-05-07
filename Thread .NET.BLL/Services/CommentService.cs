using AutoMapper;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Thread_.NET.BLL.Services.Abstract;
using Thread_.NET.Common.DTO.Comment;
using Thread_.NET.DAL.Context;
using Thread_.NET.DAL.Entities;

namespace Thread_.NET.BLL.Services
{
    public sealed class CommentService : BaseService
    {
        public CommentService(ThreadContext context, IMapper mapper) : base(context, mapper) { }

        public async Task<CommentDTO> CreateComment(NewCommentDTO newComment)
        {
            var commentEntity = _mapper.Map<Comment>(newComment);

            _context.Comments.Add(commentEntity);
            await _context.SaveChangesAsync();

            var createdComment = await _context.Comments
                .Include(comment => comment.Author)
                    .ThenInclude(user => user.Avatar)
                .FirstAsync(comment => comment.Id == commentEntity.Id);

            return _mapper.Map<CommentDTO>(createdComment);
        }


        public async Task<CommentDTO> EditComment(CommentDTO editComment)
        {
            
            var commentToUpdate = await _context.Comments.FindAsync(editComment.Id);

            if (commentToUpdate != null)
            {
                commentToUpdate.Body = editComment.Body;

                _context.Comments.Update(commentToUpdate);
                await _context.SaveChangesAsync();
            }

            



            return editComment;
        }


        public async Task<int> DeleteComment(int commentId)
        {
            var commentToDelete = await _context.Comments.FindAsync(commentId);

            if (commentToDelete == null)
            {
                return commentId;
            }

            var commentReactionsToDelete = _context.CommentReactions.Where(pr => pr.CommentId == commentId);
            _context.CommentReactions.RemoveRange(commentReactionsToDelete);

            var commentDisreactionsToDelete = _context.CommentDisreactions.Where(pr => pr.CommentId == commentId);
            _context.CommentDisreactions.RemoveRange(commentDisreactionsToDelete);

            await _context.SaveChangesAsync();

      

            _context.Comments.Remove(commentToDelete);


            await _context.SaveChangesAsync();



            return commentId;
        }
    }
}
