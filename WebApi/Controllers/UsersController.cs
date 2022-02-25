using System;
using System.Linq;
using AutoMapper;
using Game.Domain;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
        private readonly IUserRepository repo;
        private readonly IMapper mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            repo = userRepository;
            this.mapper = mapper;
        }

        [HttpHead("{userId}")]
        [HttpGet("{userId}", Name = nameof(GetUserById))]
        [Produces("application/json", "application/xml")]
        public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
        {
            var user = repo.FindById(userId);

            if (user is null)
            {
                return NotFound();
            }

            var userDto = mapper.Map<UserDto>(user);
            
            return Ok(userDto);
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] UserToCreateDto user)
        {
            if (user is null) return BadRequest();
            if (!ModelState.IsValid) return UnprocessableEntity(ModelState);

            var entity = mapper.Map<UserEntity>(user);
            entity = repo.Insert(entity);

            return CreatedAtRoute(nameof(GetUserById), new {userId = entity.Id}, entity.Id);
        }

        [HttpPut("{userId}")]
        public IActionResult UpsertUser([FromRoute] Guid userId, [FromBody] UserToUpdateDto user)
        {
            if (user is null || userId == Guid.Empty) return BadRequest();
            if (!ModelState.IsValid) return UnprocessableEntity(ModelState);

            repo.UpdateOrInsert(mapper.Map<UserToUpdateDto, UserEntity>(user.WithId(userId)), out var isInserted);

            return !isInserted 
                ? NoContent() 
                : CreatedAtRoute(nameof(GetUserById), new { userId}, userId);
        }
        
        [HttpDelete("{userId}")]
        public IActionResult DeleteUser([FromRoute] Guid userId)
        {
            if (repo.FindById(userId) is null) return NotFound();
            repo.Delete(userId);
            return NoContent();
        }
        
        [HttpOptions]
        public IActionResult GetOptions()
        {
            Response.Headers.Add("Allow", "HeaderValue");
            return Ok();
        }
    }
}