using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ElevenNote.Data.Entities;
using ElevenNote.Models.Note;

namespace ElevenNote.Models.Maps
{
    public class NoteMapProfile : Profile
    {
        public NoteMapProfile()
        {
            // Map from NoteEntity to NoteDetail/NoteListItem
            CreateMap<NoteEntity, NoteDetail>();
            CreateMap<NoteEntity, NoteListItem>();

            // Map from NoteCreate to NoteEntity, and establish current DateTimeOffset
            CreateMap<NoteCreate, NoteEntity>()
            .ForMember(note => note.CreatedUtc,
            opt => opt.MapFrom(src => DateTimeOffset.Now));

            // Map from NoteUpdate to NoteEntity, and establish current DateTimeOffset
            CreateMap<NoteUpdate, NoteEntity>()
            .ForMember(note => note.ModifiedUtc,
            opt => opt.MapFrom(src => DateTimeOffset.Now));
        }
    }
}