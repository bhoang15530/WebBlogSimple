namespace WebBlog.Data;

public partial class Blog
{
    // Blog Table Properties
    public int BlogId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    // FK - Has describe by FluentAPI in WebBlogContext
    public int UserId { get; set; }

    // navigation property - This mean allow access User Table(Model) by FK is UserId
    public virtual User? User { get; set; }
}
