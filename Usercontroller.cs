[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] JoinRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest("Username already exists.");

        if (string.IsNullOrEmpty(request.AccessCode))
        {
            // First ever user (Tier1)
            if (!_context.Users.Any())
            {
                var user = new User
                {
                    Username = request.Username,
                    Tier = "Tier1",
                    AccessCode = Guid.NewGuid().ToString().Substring(0, 8)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Registered as Tier1", accessCode = user.AccessCode });
            }
            else
            {
                return BadRequest("Access code required.");
            }
        }

        var parent = await _context.Users.FirstOrDefaultAsync(u => u.AccessCode == request.AccessCode);
        if (parent == null)
            return BadRequest("Invalid access code.");

        string tier = parent.Tier switch
        {
            "Tier1" => "Tier2",
            "Tier2" => "Tier3",
            _ => null
        };

        if (tier == null)
            return BadRequest("Cannot register under this tier.");

        var newUser = new User
        {
            Username = request.Username,
            Tier = tier,
            AccessCode = Guid.NewGuid().ToString().Substring(0, 8),
            ParentUserId = parent.Id
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Registered as {tier}", accessCode = newUser.AccessCode });
    }

    public class JoinRequest
    {
        public string Username { get; set; }
        public string AccessCode { get; set; }
    }
}
