using System.Data.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SongApi.Data;
using SongApi.Extensions;
using SongApi.Models;
using SongApi.ViewModels;
using SongApi.ViewModels.Playlists;

namespace SongApi.Controllers;

[ApiController]
public class PlaylistController : ControllerBase
{
    private readonly AppDbContext _context;

    public PlaylistController(AppDbContext context) 
        => _context = context;
    
    [HttpGet("v1/playlists")]
    public async Task<IActionResult> GetPlaylistsAsync()
    {
        var playlists = await _context.Playlists.
            Include(x=>x.User).
            AsNoTracking().
            Select(x=> new
            {
                x.Id,
                x.Title,
                x.User.Email
            }).
            ToListAsync();
        return Ok(new ResultViewModel<dynamic>(playlists));
    }

    [Authorize]
    [HttpPost("v1/playlists")]
    public async Task<IActionResult> CreatePlaylistAsync(
        [FromBody] EditorPlaylistViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));
        var userEmail = User.GetUserEmail();
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == userEmail);

        if (user is null)
            return NotFound(new ResultViewModel<string>("Usuário não autenticado"));
        
        var playlist = new Playlist
        {
            Id = 0,
            Title = model.Title,
            User = user
        };
        
        try
        {
            await _context.Playlists.AddAsync(playlist);
            await _context.SaveChangesAsync();
            return Created("v1/playlists", new ResultViewModel<dynamic>(new
            {
                playlist.Id,
                playlist.Title,
                playlist.User.Email,
                playlist.Songs
            }));
        }
        catch (DbException)
        {
            return StatusCode(500,new ResultViewModel<string>("Erro ao inserir playlist no banco"));
        }
    }
}