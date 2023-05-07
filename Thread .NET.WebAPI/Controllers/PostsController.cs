using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thread_.NET.BLL.Services;
using Thread_.NET.Common.DTO.Like;
using Thread_.NET.Common.DTO.Dis;
using Thread_.NET.Common.DTO.Post;
using Thread_.NET.Extensions;
using Thread_.NET.DAL.Entities;
using Org.BouncyCastle.Asn1.Ocsp;
using Newtonsoft.Json.Linq;

namespace Thread_.NET.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly LikeService _likeService;
        private readonly DisService _disService;

        public PostsController(PostService postService, LikeService likeService, DisService disService)
        {
            _postService = postService;
            _likeService = likeService;
            _disService = disService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ICollection<PostDTO>>> Get()
        {
            return Ok(await _postService.GetAllPosts());
        }

        [HttpPost]
        public async Task<ActionResult<PostDTO>> CreatePost([FromBody] PostCreateDTO dto)
        {
            dto.AuthorId = this.GetUserIdFromToken();

            return Ok(await _postService.CreatePost(dto));
        }

        [HttpPost("edit")]
        public async Task<ActionResult<PostDTO>> EditPost([FromBody] PostDTO post)
        {
            return Ok(await _postService.EditPost(post));
        }

        [HttpDelete("delete/{postId}")]

        public async Task<ActionResult<int>> DeletePost( int postId)
        {
            return Ok(await _postService.DeletePost(postId));
        }

        [HttpGet("{postId}")]

        public async Task<ActionResult<int>> GetPost(int postId)
        {
            return Ok(await _postService.GetPost(postId));
        }

        [HttpPost("share/{postId}")]
        public async Task<IActionResult> SharePost(int postId, [FromBody] EmailShare request)

        {
            
           

            return Ok(await _postService.SendEmailAsync(request.Email, postId, request.UserName));

            
        }




        [HttpPost("like")]
        public async Task<IActionResult> LikePost(NewReactionDTO reaction)
        {
            reaction.UserId = this.GetUserIdFromToken();

            await _likeService.LikePost(reaction);
            return Ok();
        }

        [HttpPost("dis")]
        public async Task<IActionResult> DisPost(NewDisreactionDTO disreaction)
        {
            disreaction.UserId = this.GetUserIdFromToken();

            await _disService.DisPost(disreaction);
            return Ok();
        }
    }
}