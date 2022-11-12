using Microsoft.EntityFrameworkCore;

namespace QBCore.Configuration;

public interface IEntityFrameworkDbContext
{
	DbContext Context { get; }
}