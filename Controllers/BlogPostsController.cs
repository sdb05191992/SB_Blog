using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SB_Blog.Helpers;
using SB_Blog.Models;
using PagedList;
using PagedList.Mvc;

namespace SB_Blog.Controllers
{
    [RequireHttps]
    public class BlogPostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BlogPosts
        public ActionResult Index(int? page, string searchStr)
        {
            //List<BlogPost> model = db.Posts.OrderByDescending(b => b.Created).Take(6).ToList();
            ViewBag.Search = searchStr;
            ViewBag.CommentCount = db.Comments.Count();
            ViewBag.Progress = IndexSearch("Progress").Count();
            ViewBag.Reflection = IndexSearch("Reflection").Count();
            var blogList = IndexSearch(searchStr);

            int pageSize = 6;
            int pageNumber = (page ?? 1);
            return View(blogList.ToPagedList(pageNumber, pageSize));
        }

        public IQueryable<BlogPost> IndexSearch(string searchStr)
        {
            IQueryable<BlogPost> result = null;
            if (searchStr != null)
            {
                result = db.Posts.AsQueryable();
                result = result.Where(p => p.Title.Contains(searchStr) ||
                                       p.Body.Contains(searchStr) ||
                                       p.Comments.Any(c => c.Body.Contains(searchStr) ||
                                                       c.Author.FirstName.Contains(searchStr) ||
                                                       c.Author.LastName.Contains(searchStr) ||
                                                       c.Author.DisplayName.Contains(searchStr) ||
                                                       c.Author.Email.Contains(searchStr)));
            }
            else
            {
                result = db.Posts.AsQueryable();
            }
            return result.OrderByDescending(p => p.Created);
        }

        // GET: BlogPosts/Details/5

        public ActionResult Details(string Slug)
        {
            //This action checks the slug to see if it is valid
            //If the slug is not valide return a bad request
            if (String.IsNullOrWhiteSpace(Slug))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogDetailsViewModel model = new BlogDetailsViewModel();
            IEnumerable<BlogPost> blogList = db.Posts;

            //Getting ViewModel data
            BlogPost blog = db.Posts.FirstOrDefault(p => p.Slug == Slug);
            var prev = GetPrevious(blogList, blog);
            var next = GetNext(blogList, blog);

            //Adding Data to ViewModel
            model.CurrentBlog = blog;
            model.PreviousBlog = prev;
            model.NextBlog = next;

            //FirstOrDefault finds the first or default record that matches the slug that I am searching for
            //BlogPost blogPost = db.Posts.FirstOrDefault(p => p.Slug == Slug);

            if (blog == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        private static T GetNext<T>(IEnumerable<T> list, T current)
        {
            try
            {
                return list.SkipWhile(x => !x.Equals(current)).Skip(1).First();
            }
            catch
            {
                return default(T);
            }
        }

        private static T GetPrevious<T>(IEnumerable<T> list, T current)
        {
            try
            {
                return list.TakeWhile(x => !x.Equals(current)).Last();
            }
            catch
            {
                return default(T);
            }
        }

        [Authorize(Roles = "Admin")]
        // GET: BlogPosts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BlogPosts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Body,MediaURL,Published")] BlogPost blogPost, HttpPostedFileBase image)
        {
            //This if statement ensures all data required above ^^^ is entered & valid
            if (ModelState.IsValid)
            {
                var Slug = StringUtilities.URLFriendly(blogPost.Title);
                if (String.IsNullOrWhiteSpace(Slug))
                {
                    ModelState.AddModelError("Title", "Invalid title");
                    return View(blogPost);
                }
                if (db.Posts.Any(p => p.Slug == Slug))
                {
                    ModelState.AddModelError("Title", "The title must be unique");
                    return View(blogPost);
                }

                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                    blogPost.MediaURL = "/Uploads/" + fileName;
                }

                blogPost.Slug = Slug;                   //This assigns each blog post a slug (plain english URL)
                blogPost.Created = DateTimeOffset.Now;  //This grabs the systems time & automatically sets the value for Created
                db.Posts.Add(blogPost);                 //Go to the db & find the Posts table & add it (the blogPost) to the table
                db.SaveChanges();                       //Self explanatory - saves changes to the db
                return RedirectToAction("Index");       //Once it saves return to the Index page listed to the Index Action above in Line 19

            }

            return View(blogPost);
        }

        // GET: BlogPosts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.Posts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Created,Updated,Title,Slug,Body,MediaURL,Published")] BlogPost blogPost, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                    blogPost.MediaURL = "/Uploads/" + fileName;
                }

                db.Entry(blogPost).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(blogPost);
        }

        // GET: BlogPosts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.Posts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // POST: BlogPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BlogPost blogPost = db.Posts.Find(id);
            db.Posts.Remove(blogPost);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
