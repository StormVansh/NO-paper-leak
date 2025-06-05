[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public DocumentController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string uploadedBy)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Invalid file.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == uploadedBy);
        if (user == null)
            return BadRequest("Invalid uploader.");

        var uploadsPath = Path.Combine(_env.ContentRootPath, "Uploads");
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var filePath = Path.Combine(uploadsPath, file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var document = new Document
        {
            FileName = file.FileName,
            FilePath = filePath,
            UploadDate = DateTime.UtcNow,
            UploadedBy = uploadedBy,
            Tier = user.Tier
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Upload successful." });
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
            return BadRequest("Invalid user.");

        List<string> accessibleTiers = user.Tier switch
        {
            "Tier1" => new List<string> { "Tier1", "Tier2", "Tier3" },
            "Tier2" => new List<string> { "Tier2", "Tier3" },
            "Tier3" => new List<string> { "Tier3" },
            _ => new List<string>()
        };

        var docs = await _context.Documents
            .Where(d => accessibleTiers.Contains(d.Tier))
            .ToListAsync();

        return Ok(docs);
    }
}
