using Microsoft.EntityFrameworkCore;

namespace CPMSender.Services;

public class PosttradeDbContext(DbContextOptions<PosttradeDbContext> options) : DbContext(options);