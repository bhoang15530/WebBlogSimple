using WebBlog.Data;

namespace WebBlog.Models
{
    public class AdminUserViewModel
    {
        // Use both properties of BlogViewModel and User Model
        public List<BlogViewModel> Blogs{ get; set; }
        public List<User> Users { get; set; }
    }
}
