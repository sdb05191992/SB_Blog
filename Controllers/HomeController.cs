using Microsoft.AspNet.Identity;
using SB_Blog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SB_Blog.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            EmailModel model = new EmailModel();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(EmailModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var body = "<p>Email From: <bold>{0}</bold>" +
                                "({1})</p><p>Message:</p><p>{2}</p><p>{3}</p>";
                    model.Body = "This is a message from your blog site. The name and " +
                                  "the email of the contacting person is above.";

                    var svc = new EmailService();
                    var msg = new IdentityMessage()
                    {
                        Subject = "Contact From Portfolio Site",
                        Body = string.Format(body, model.FromName, model.FromEmail,
                                        model.Body, model.Message1),
                        Destination = "sdb051992@gmail.com"
                    };
                    await svc.SendAsync(msg);
                    return View();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.FromResult(0);
                    return View(model);
                }
            }
            else { return View(model); }
        }
    }
}