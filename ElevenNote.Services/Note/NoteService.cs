using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ElevenNote.Models.Note;
using ElevenNote.Data;
using Microsoft.EntityFrameworkCore;
using ElevenNote.Data.Entities;
using AutoMapper;

namespace ElevenNote.Services.Note
{
    public class NoteService : INoteService
    {
        private readonly int _userId;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _dbContext;
        public NoteService(IHttpContextAccessor httpContextAccessor, IMapper mapper, ApplicationDbContext dbContext)
        {
            var userClaims = httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            var value = userClaims.FindFirst("Id")?.Value;
            var validId = int.TryParse(value, out _userId);
            if (!validId) 
            throw new Exception("Attempted to build NoteService without User Id claim.");

            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<bool> CreateNoteAsync(NoteCreate request)
        {
            var noteEntity = _mapper.Map<NoteCreate, NoteEntity>(request, opt => 
                opt.AfterMap((src, dest) => dest.OwnerId = _userId));

            _dbContext.Notes.Add(noteEntity);

            var numberOfChanges = await _dbContext.SaveChangesAsync();
            return numberOfChanges == 1;
        }

        public async Task<IEnumerable<NoteListItem>> GetAllNotesAsync()
        {
            var notes = await _dbContext.Notes
                .Where(entity => entity.OwnerId == _userId)
                .Select(entity => _mapper.Map<NoteListItem>(entity))
                .ToListAsync();

                return notes;
        }
        public async Task<NoteDetail> GetNoteByIdAsync(int noteId)
        {
            // Find the first note that has the given Id and an OwnerId that matches the requesting userId
            var noteEntity = await _dbContext.Notes
                .FirstOrDefaultAsync(e => e.Id == noteId && e.OwnerId == _userId);

            // If noteEntity is null then return null, otherwise initialize and return a new NoteDetail
            return noteEntity is null ? null : _mapper.Map<NoteDetail>(noteEntity);
        }
        public async Task<bool> UpdateNoteAsync(NoteUpdate request)
        {
            // Check the database to see if there's a note entity that matches request 
            // Any returns true if any entity exists
            var noteIsUserOwned = await _dbContext.Notes.AnyAsync(note => 
                note.Id == request.Id && note.OwnerId == _userId);

            if (!noteIsUserOwned)
            return false;

            // Map from Update to Entity and set OwnerId again
            var newEntity = _mapper.Map<NoteUpdate, NoteEntity>(request, opt =>
                opt.AfterMap((src, dest) => dest.OwnerId = _userId));

            // Update the Entry State, which is another way to tell the DbContext something has changed
            _dbContext.Entry(newEntity).State = EntityState.Modified;

            // Because we don't currently have access to CreatedUtc value, we'll just make it as not modified
            _dbContext.Entry(newEntity).Property(e => e.CreatedUtc).IsModified = false;

            // Save the changes to the database and capture how many rows were updated
            var numberOfChanges = await _dbContext.SaveChangesAsync();

            // numberOfChanges should be 1 because only 1 row is updated
            return numberOfChanges == 1;
        }
        public async Task<bool> DeleteNoteAsync(int noteId)
        {
            var noteEntity = await _dbContext.Notes.FindAsync(noteId);
            if (noteEntity?.OwnerId != _userId)
            return false;

            _dbContext.Notes.Remove(noteEntity);
            return await _dbContext.SaveChangesAsync() == 1;
        }
    }
}