using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SB_Blog.Models
{
    public class BlogDetailsViewModel
    {
        public BlogPost CurrentBlog { get; set; }
        public BlogPost PreviousBlog { get; set; }
        public BlogPost NextBlog { get; set; }
    }
}