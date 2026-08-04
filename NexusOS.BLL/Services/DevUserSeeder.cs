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
            if (_context.Users.Any(u => u.Id == Guid.Parse("00000000-0000-0000-0100-000000000000")))
                return;

            var devUser = new User
            {
                Id = Guid.Parse("00000000-0000-0000-0100-000000000000"),
                Username = "dev",
                Email = "tri.nguyen.sft@gmail.com",
                PasswordHash = PasswordHasher.HashPassword("Dev123!@#"),
            };

            _context.Users.Add(devUser);
            _context.SaveChanges();
        }
    }
}
