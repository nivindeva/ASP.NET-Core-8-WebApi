using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Intranet.Application.DTOs;
using Intranet.Domain.Entities;

namespace Intranet.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Product Mappings
            CreateMap<Product, ProductDto>().ReverseMap(); // ReverseMap handles ProductDto -> Product if needed
            CreateMap<CreateUpdateProductDto, Product>(); // For creating/updating Product from DTO

            // Employee Mappings
            CreateMap<Employee, EmployeeDto>()
                // Example: If EmployeeDto had a FullName property not in Employee
                // .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ReverseMap(); // Allows EmployeeDto -> Employee mapping if needed
            CreateMap<CreateUpdateEmployeeDto, Employee>();

            // Department Mappings
            CreateMap<Department, DepartmentDto>().ReverseMap();
            CreateMap<CreateUpdateDepartmentDto, Department>();

            // Add other mappings here as needed
        }
    }
}
