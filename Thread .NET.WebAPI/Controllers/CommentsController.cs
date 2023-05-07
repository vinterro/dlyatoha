using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Thread_.NET.BLL.Services;
using Thread_.NET.Common.DTO.Like;
using Thread_.NET.Common.DTO.Dis;
using Thread_.NET.Common.DTO.Comment;
using Thread_.NET.Extensions;

namespace Thread_.NET.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly CommentService _commentService;
        private readonly LikeService _likeService;
        private readonly DisService _disService;

        public CommentsController(CommentService commentService, LikeService likeService, DisService disService)
        {
            _commentService = commentService;
            _likeService = likeService;
            _disService = disService;
        }

        [HttpPost]
        public async Task<ActionResult<CommentDTO>> CreatePost([FromBody] NewCommentDTO comment)
        {
            comment.AuthorId = this.GetUserIdFromToken();
            return Ok(await _commentService.CreateComment(comment));
        }

        [HttpPost("edit")]
        public async Task<ActionResult<CommentDTO>> EditPost([FromBody] CommentDTO comment)
        {
            
            return Ok(await _commentService.EditComment(comment));
        }

        [HttpDelete("delete/{commentId}")]

        public async Task<ActionResult<int>> DeleteComment(int commentId)
        {
            return Ok(await _commentService.DeleteComment(commentId));
        }

        [HttpPost("like")]
        public async Task<IActionResult> LikeComment(NewReactionDTO reaction)
        {
            reaction.UserId = this.GetUserIdFromToken();

            await _likeService.LikeComment(reaction);
            return Ok();
        }

        [HttpPost("dis")]
        public async Task<IActionResult> DisComment(NewDisreactionDTO disreaction)
        {
            disreaction.UserId = this.GetUserIdFromToken();

            await _disService.DisComment(disreaction);
            return Ok();
        }
    }
}