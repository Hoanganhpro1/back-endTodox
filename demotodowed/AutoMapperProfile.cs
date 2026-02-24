using AutoMapper;
using TodoApp.Core.DTOs;
using TodoApp.Core.Entities;
using TodoApp.Core.Enums;

namespace TodoApp.API
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<TodoItem, TodoDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString().ToLower()));

            CreateMap<CreateTodoDto, TodoItem>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TodoStatus.Active));

            CreateMap<UpdateTodoDto, TodoItem>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src =>
                        src.Status.ToLower() == "completed" ? TodoStatus.Completed : TodoStatus.Active));
        }
    }
}