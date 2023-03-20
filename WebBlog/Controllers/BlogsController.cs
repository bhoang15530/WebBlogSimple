using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBlog.Data;
using WebBlog.Filter;

namespace WebBlog.Controllers
{
    public class BlogsController : Controller
    {
        // Create Object of WebBlogContext to accessing and manipulating databases
        private readonly WebBlogContext _context;

        public BlogsController(WebBlogContext context)
        {
            _context = context;
        }

        #region Index
        // GET: /Blogs/Index
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            // Set page size
            const int pageSize = 10;
            // Get all blogs
            var blogs = await _context.Blogs.ToListAsync();

            // If user logined then get Username and thier Id, return to view by ViewData and ViewBag
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            var userId = user?.UserId;
            ViewData["UserId"] = userId;
            ViewBag.UserId = userId.ToString();

            // Get Username who write thier Blog and display it in right way
            var blogUserPairs = await _context.Blogs
                .Join(_context.Users, b => b.UserId, u => u.UserId, (b, u) => new { Blog = b, User = u })
                .ToListAsync();

            ViewData["Usernames"] = blogUserPairs.ToDictionary(p => p.Blog.BlogId, p => p.User.Username);

            // Add search functionality
            if (!string.IsNullOrEmpty(searchString))
            {
                blogs = blogs.Where(b => b.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                                         b.User.Username.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                             .ToList();
                return View(blogs);
            }

            var totalBlogs = blogs.Count;
            var totalPages = (int)Math.Ceiling((double)totalBlogs / pageSize);
            var paginatedBlogs = blogs.Skip((page - 1) * pageSize).Take(pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(paginatedBlogs);
        }
        #endregion

        #region Details
        // GET: Blogs/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            // For dev, if find no id then return client notfound
            if (id == null || _context.Blogs == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.BlogId == id);

            // For dev, if find no blog then return client notfound
            if (blog == null)
            {
                return NotFound();
            }

            // Select Username/UserId from User Table link with UserId in Blog Table
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            var userId = user?.UserId;
            ViewData["UserId"] = userId;
            ViewBag.UserId = userId.ToString();
            ViewData["BlogId"] = id;

            // Select Username suitable with BlogId through UserId in Both Table
            var blogB = await _context.Blogs
                .Join(
                    _context.Users,
                    b => b.UserId,
                    u => u.UserId,
                    (b, u) => new { Blog = b, User = u }
                )
                .Where(x => x.Blog.BlogId == id)
                .Select(x => new {
                    Blog = x.Blog,
                    Username = x.User.Username
                })
                .FirstOrDefaultAsync();

            ViewData["Username"] = blogB.Username;

            return View(blog);
        }
        #endregion

        #region Create
        // GET: Blogs/Create
        [CustomAuthorize(Roles = "User")]
        public async Task<IActionResult> Create()
        {
            // User logined then get thier Username and UserId
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            var userId = user?.UserId;

            // For Admin when delete user, if they try to access this Action then return to Sign In View
            if (userId == null)
            {
                TempData["Message"] = "Your account has been deleted. Please contact an administrator for more information.";
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("SignIn", "Account");
            }

            ViewData["UserId"] = userId;
            return View();
        }

        // POST: Blogs/Create/{BlogId}{Title}{Content}{UserId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "User")]
        public async Task<IActionResult> Create([Bind("BlogId,Title,Content,UserId")] Blog blog)
        {
            // User logined then get thier Username and UserId
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            var userId = user?.UserId;

            // For Admin when delete user, if they try to access this Action then return to Sign In View
            if (userId == null)
            {
                TempData["Message"] = "Your account has been deleted. Please contact an administrator for more information.";
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("SignIn", "Account");
            }

            // If model have suitable properties with POST then add a Blogs and Save
            if (ModelState.IsValid)
            {
                blog.Content = blog.Content.Replace(Environment.NewLine, "<br>");

                _context.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["UserId"] = blog.UserId;
            return View();
        }
        #endregion

        #region Edit
        // GET: Blogs/Edit/{blogId?}
        [CustomAuthorize(Roles = "User")]
        public async Task<IActionResult> Edit(int? blogId)
        {
            // User logined then get thier Username and UserId
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            var userId = user?.UserId;

            // For Admin when delete user, if they try to access this Action then return to Sign In View
            if (userId == null)
            {
                TempData["Message"] = "Your account has been deleted. Please contact an administrator for more information.";
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("SignIn", "Account");
            }

            // For dev test
            if (blogId == null || _context.Blogs == null)
            {
                return NotFound();
            }

            // find blog follow blogId
            var blog = await _context.Blogs.FindAsync(blogId);
            // For dev test
            if (blog == null)
            {
                return NotFound();
            }

            ViewData["UserId"] = userId;
            return View(blog);
        }

        // POST: Blogs/Edit/{BlogId}{Title}{Content}{UserId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "User")]
        public async Task<IActionResult> Edit([Bind("BlogId,Title,Content,UserId")] Blog blog)
        {
            // User logined then get thier Username and UserId
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            var userId = user?.UserId;

            // For Admin when delete user, if they try to access this Action then return to Sign In View
            if (userId == null)
            {
                TempData["Message"] = "Your account has been deleted. Please contact an administrator for more information.";
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("SignIn", "Account");
            }

            // If model have suitable properties with POST then add a Blogs and Save
            if (ModelState.IsValid)
            {
                blog.Content = blog.Content.Replace(Environment.NewLine, "<br>");
                _context.Update(blog);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = blog.UserId;
            return View(blog);
        }
        #endregion

        #region Delete
        // GET: Blogs/Delete/5
        [CustomAuthorize(Roles = "User")]
        public async Task<IActionResult> Delete(int? id)
        {
            // User logined then get thier Username and UserId
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            var userId = user?.UserId;

            // For Admin when delete user, if they try to access this Action then return to Sign In View
            if (userId == null)
            {
                TempData["Message"] = "Your account has been deleted. Please contact an administrator for more information.";
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("SignIn", "Account");
            }

            // For dev test
            if (id == null || _context.Blogs == null)
            {
                return NotFound();
            }

            // Select blog follow BlogId
            var blog = await _context.Blogs
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.BlogId == id);
            // For dev test
            if (blog == null)
            {
                return NotFound();
            }

            ViewData["UserId"] = userId;
            ViewBag.UserId = userId.ToString();

            return View(blog);
        }

        // POST: Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "User")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            var userId = user?.UserId;

            if (userId == null)
            {
                TempData["Message"] = "Your account has been deleted. Please contact an administrator for more information.";
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("SignIn", "Account");
            }

            if (_context.Blogs == null)
            {
                return Problem("Entity set 'WebBlogContext.Blogs'  is null.");
            }
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        private bool BlogExists(int id)
        {
          return (_context.Blogs?.Any(e => e.BlogId == id)).GetValueOrDefault();
        }
    }
}
