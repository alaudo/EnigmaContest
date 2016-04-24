using EnigmaContest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnigmaContest.Controllers
{
    public class ResultController : Controller
    {
        internal LinqEnigmaContentDataContext db = new LinqEnigmaContentDataContext();
        // GET: Result
        public ActionResult Leaderboard()
        {
            var usrs =
            db.Users
                .OrderByDescending(m => m.Score)
                .ThenBy(m1 => m1.Age)
                .ToList();
            return View(usrs);
        }

        public ActionResult Administration()
        {
            return View();
        }

    }
}