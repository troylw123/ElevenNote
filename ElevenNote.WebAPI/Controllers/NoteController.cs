using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElevenNote.Models.Note;
using ElevenNote.Services.Note;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElevenNote.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NoteController : ControllerBase
    {
        private readonly INoteService _noteService;
        public NoteController(INoteService noteService)
        {
            _noteService = noteService;
        }
        // GET api/Note
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<NoteListItem>),200)]
        public async Task<IActionResult> GetAllNotes()
        {
            var notes = await _noteService.GetAllNotesAsync();
            return Ok(notes);
        }
        // Post api/Note
        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] NoteCreate request)
        {
            if (!ModelState.IsValid)
            return BadRequest(ModelState);

            if (await _noteService.CreateNoteAsync(request))
            return Ok("Note created successfully.");

            return BadRequest("Note could not be created.");
        }
        // Get api/Note/5
        [HttpGet("{noteId:int}")]
        public async Task<IActionResult> GetNoteById([FromRoute] int noteId)
        {
            var detail = await _noteService.GetNoteByIdAsync(noteId);

            // Similar to our service method, we're using a ternary to determine our return type
            // If the returned value (detail) is not null, return it with a 200 OK
            // Otherwise return a NotFound() 404 response
            return detail is not null ? Ok(detail) : NotFound();
        }
        // PUT api/Note
        [HttpPut]
        public async Task<IActionResult> UpdateNoteById([FromBody] NoteUpdate request)
        {
            if (!ModelState.IsValid)
            return BadRequest(ModelState);

            return await _noteService.UpdateNoteAsync(request)
            ? Ok("Note updated successfully.")
            : BadRequest("Note could not be updated.");
        }
        // DELETE api/Note/5
        [HttpDelete("{noteId:int}")]
        public async Task<IActionResult> DeleteNote([FromRoute] int noteId)
        {
            return await _noteService.DeleteNoteAsync(noteId)
            ? Ok($"Note {noteId} was deleted successfully.")
            : BadRequest($"Note {noteId} could not be deleted.");
        }
    }
}
