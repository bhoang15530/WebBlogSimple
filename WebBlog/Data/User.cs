namespace WebBlog.Data;

public partial class User
{
    // Properties of User table here
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    // One-To-Many Configuration
    public virtual ICollection<Blog> Blogs { get; } = new List<Blog>();
}
