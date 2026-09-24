using NexusOS.DAL.Models;
using NexusOS.Util;

namespace NexusOS.BLL.Services
{
    public class DevUserSeeder
    {
        private readonly NexusOsContext _context; // Dùng để truy cập vào DbContext

        public DevUserSeeder(NexusOsContext context)
        {
            _context = context;
        }

        public void Seed()
        {
            var listId = new List<Guid>()
            {
                new Guid("00000000-0000-0000-0100-000000000000"),
                new Guid("00000000-0000-0000-0200-000000000000"),
                new Guid("00000000-0000-0000-0300-000000000000"),
            };

            if (_context.Users.Any(u => listId.Contains(u.Id))) return;

            var listUser = new List<User>()
            {
                new  User
                {
                    Id = Guid.Parse("00000000-0000-0000-0100-000000000000"),
                    Username = "director100",
                    Email = "director.manager@nexusost.com",
                    PasswordHash = PasswordHasher.HashPassword("Director2026!#"),
                },
                new  User
                {
                    Id = Guid.Parse("00000000-0000-0000-0200-000000000000"),
                    Username = "hr200",
                    Email = "hr.manager@nexusost.com",
                    PasswordHash = PasswordHasher.HashPassword("HRManager2026!#"),
                },
                new  User
                {
                    Id = Guid.Parse("00000000-0000-0000-0300-000000000000"),
                    Username = "emp300",
                    Email = "emp.john@nexusost.com",
                    PasswordHash = PasswordHasher.HashPassword("Employee2026!#"),
                },
            };

            _context.Users.AddRange(listUser);
            _context.SaveChanges();
        }
    }
}
