namespace WebBlog.Models
{
    public class BlogViewModel
    {
        // Use to get Username in table User
        public int BlogId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Username { get; set; }
    }
}
