using Develop.DTOs;
using Develop.DTOs.DVP;
using QBCore.DataSource;

namespace Develop.Services;

public interface IProjectService : IDataSource<int, ProjectCreateDto, ProjectSelectDto, ProjectUpdateDto, SoftDelDto, SoftDelDto> { }