using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using WebBlog.Data;
using WebBlog.Models;
using Microsoft.EntityFrameworkCore;
using WebBlog.Filter;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebBlog.Controllers
{
    public class AccountController : Controller
    {
        // Create Object of WebBlogContext to accessing and manipulating databases
        private readonly WebBlogContext _context;

        // Create key int and key string to make Hash Password
        private const int KeyLen = 10000;
        private const string KeyName = "WebBlog";

        public AccountController(WebBlogContext context)
        {
            _context = context;
        }

        // GET: /Account/SignIn
        public IActionResult SignIn()
        {
            ViewBag.Message = TempData["Message"]?.ToString();
            ViewBag.Success = TempData["Success"]?.ToString();
            return View();
        }

        #region Sign In Method

        // POST: /Account/SignIn/{Username}{Password}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn([Bind("Username, Password")] SignIn signIn )
        {
            // Check User is exist or not
            bool CheckUser = await _context.Users.AnyAsync(u => u.Username == signIn.Username);

            // Check if sign in with admin account
            // Here I set Admin Account with Specify value, not save into database
            if (signIn.Username == "Administrator" && signIn.Password == "12345678")
            {
                // Assign Role and Name in Claim card
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, signIn.Username),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(claims, "signIn");
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(principal);

                return RedirectToAction("AdminSetting", "Account");
            }

            // If User exist then Encode Password to check Password is correct or not
            else if (CheckUser == true)
            {
                // Create salt
                byte[] encode = new byte[KeyLen];
                encode = System.Text.Encoding.UTF8.GetBytes(KeyName);

                // Encoding password to Base64 with specify salt
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: signIn.Password!,
                    salt: encode,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

                // assign hashed to password
                signIn.Password = hashed;
            }

            // Check if Username with Encode Password is correct or not
            bool checkPassword = await _context.Users.AnyAsync(u => u.Password == signIn.Password && u.Username == signIn.Username);

            if (checkPassword == true)
            {
                // If password is true then Create Claim card with Name and Role
                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, signIn.Username),
                        new Claim(ClaimTypes.Role, "User")
                    };
                var identity = new ClaimsIdentity(claims, "signIn");
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(principal);

                return RedirectToAction("Index", "Home");
            }

            // If all thing above is wrong then sure that Username or Password is wrong
            ViewBag.Error = "Invalid username or password";
            return View(signIn);

        }
        #endregion

        // GET: /Account/SignUp
        public IActionResult SignUp()
        {
            return View();
        }

        #region Sign Up Method

        // POST: /Account/SignUp/{Username}{Password}{ConfirmPassword}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([Bind("Username, Password, ConfirmPassword")] SignUp signUp )
        {
            // Check Username exist
            bool checkAccount = _context.Users.Any(u => u.Username == signUp.Username);
            if (checkAccount == true)
            {
                ViewBag.UsernameExist = "This Username already exist";
                return View(signUp);
            }

            // Check Username not allow the name "Administrator"
            if (signUp.Username == "Administrator")
            {
                ViewBag.UsernameNotAllow = "You cannot use this Username";
                return View(signUp);
            }

            #region Encode Password
            // If all condition above is false then Encode Password
            byte[] encode = new byte[KeyLen];
            encode = System.Text.Encoding.UTF8.GetBytes(KeyName);

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: signUp.Password!,
                salt: encode,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            signUp.Password = hashed;
            #endregion

            // Add User into Users table
            User u = new()
            {
                Username = signUp.Username,
                Password = signUp.Password
            };

            // Add and Save Account to database
            await _context.Users.AddAsync(u);
            await _context.SaveChangesAsync();

            // Tell the cshtml sign up success
            TempData["Success"] = "Sign up success";

            return RedirectToAction(nameof(SignIn));
        }
        #endregion

        // GET: /Account/SignOut
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(SignIn));
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            // This action work when user or admin try to access the page that not allow
            return View();
        }

        #region Admin Area
        // GET: /Account/AdminSetting
        [CustomAuthorize(Roles = "Admin")]
        public IActionResult AdminSetting()
        {
            // Select all blogs from table Blogs join table Users with 4 column as below code
            var blogs = (from blog in _context.Blogs
                         join user in _context.Users on blog.UserId equals user.UserId
                         select new BlogViewModel
                         {
                             BlogId = blog.BlogId,
                             Title = blog.Title,
                             Content = blog.Content,
                             Username = user.Username
                         }).ToList();

            // Select all users from table Users with 2 column as below code
            var users = _context.Users.Select(u => new User
            {
                UserId = u.UserId,
                Username = u.Username
            }).ToList();

            // Create object model with value of both table and return to View
            var model = new AdminUserViewModel
            {
                Blogs = blogs,
                Users = users
            };

            return View(model);
        }

        // AJAX-GET: /Account/GetBlogContent/{blogId}
        [HttpGet]
        [CustomAuthorize(Roles = "Admin")]
        [Route("Account/GetBlogContent/{blogId}")]
        public IActionResult GetBlogContent(int blogId)
        {
            // Find blogId in Blogs table
            var blog = _context.Blogs.Find(blogId);

            // For Dev test, is blogId find the Id or Not and return response to Client
            if (blog == null)
            {
                return NotFound();
            }

            // Return content of blog
            return Ok(new { content = blog.Content });
        }

        // AJAX-DELETE: /Account/DeleteBlog/{blogId}
        [HttpDelete]
        [CustomAuthorize(Roles = "Admin")]
        [Route("Account/DeleteBlog/{blogId}")]
        public async Task<IActionResult> DeleteBlog(int blogId)
        {
            // Find blogId in Blogs table
            var blog = await _context.Blogs.FindAsync(blogId);

            // For Dev test, is blogId find the Id or Not and return response to Client
            if (blog == null)
            {
                return NotFound();
            }

            // Delete and save change
            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // AJAX-DELETE: /Account/DeleteUser/{userId}
        [HttpDelete]
        [CustomAuthorize(Roles = "Admin")]
        [Route("Account/DeleteUser/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            
            // Get user
            var user = await _context.Users.FindAsync(userId);

            // If cannot find any user then return response NotFound to Client
            if (user == null)
            {
                return NotFound();
            }

            // Because Blogs table have UserId so when delete User, delete thier blog too
            // get list blogs of defined user
            var blogs = await _context.Blogs.Where(u => u.UserId== userId).ToListAsync();

            // Delete user and thier blog. Save change
            _context.Blogs.RemoveRange(blogs);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok();
        }
        #endregion
    }
}
