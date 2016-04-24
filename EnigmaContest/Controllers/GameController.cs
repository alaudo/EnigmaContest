using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnigmaContest.Models;
using EnigmaContest.Code;

namespace EnigmaContest.Controllers
{
    public class GameController : Controller
    {
        internal StringEncrypt se = new StringEncrypt();
        internal LinqEnigmaContentDataContext db = new LinqEnigmaContentDataContext();
        // GET: Game
        [Authorize]
        [HttpGet]
        public ActionResult Play()
        {
            var login = MvcHelpers.GetUsername();
            var user = db.Users.Where(a => a.Username == login).FirstOrDefault();
            int complexity = 1;
            if (user != null)
            {
                complexity = user.Complexity ?? 1;
            }
            else
            {
                var newuser = MvcHelpers.GetUser();
                newuser.Complexity = 1;

                db.Users.InsertOnSubmit(newuser);
                db.SubmitChanges();

            }
            ViewBag.User = login;
            var msg = se.GetNewMessage(complexity);
            msg.OriginalMessage = "";
            return View(msg);
        }

        [HttpPost]
        [Authorize]
        public ActionResult Play(DecodedMessage msg)
        {
            var m = db.Messages.Where(ms => ms.Id == msg.MessageId).FirstOrDefault();

            DecodeResult dr = new DecodeResult();

            if (m != null)
            {
                double concordance = 0;
                if (!string.IsNullOrWhiteSpace(msg.OriginalMessage))
                {
                    concordance = m.Text.FuzzyCompare(msg.OriginalMessage);
                }
                dr.Fitness = concordance;
                dr.Score = msg.Score;

                if (concordance > 0.85)
                {
                    dr.IsCorrect = true;
                }
                else
                {
                    dr.IsCorrect = false;
                }
            }
            else
            {
                dr.IsCorrect = false;
            }

            var login = MvcHelpers.GetUsername();
            var user = db.Users.Where(a => a.Username == login).First();
            ViewBag.User = login;
            user.Score = (user.Score ?? 0) + (dr.IsCorrect ? msg.Score : -1);
            user.Complexity = Math.Max(msg.Complexity, user.Complexity ?? 0) + (dr.IsCorrect ? 1 : 0);
            user.LastAttempt = DateTime.Now;
            user.Attempted = (user.Attempted ?? 0) + 1;
            user.Deciphered = (user.Deciphered ?? 0) + (dr.IsCorrect ? 1 : 0);
            db.SubmitChanges();

            dr.TotalScore = user.Score ?? 0;

            return View("Result",dr);
        }


        [HttpGet]
        public ActionResult Practice()
        {
            return View();
        }

    }
}