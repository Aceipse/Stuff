using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Facebook;

namespace Web.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        public void LogIn()
        {
            Session.Clear();

            var fb = new FacebookClient();

            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = "911465178881112",

                redirect_uri = "http://tracker.aceipse.dk/Login/LoginCheck",

                response_type = "code",

                scope = "email" // Add other permissions as needed)
            });

            Response.Redirect(loginUrl.AbsoluteUri);
        }

        public ActionResult LoginCheck()
        {
            if (Request.QueryString["code"] == null)
                return RedirectToAction("Index", "Home");

            //Get new access code
            string accessCode = Request.QueryString["code"].ToString();

            var fb = new FacebookClient();

            dynamic result = fb.Post("oauth/access_token", new
            {
                client_id = "911465178881112",

                client_secret = "5de4ebacafaf9ddd9a168468f72dffbb",

                redirect_uri = "http://tracker.aceipse.dk/Login/LoginCheck",

                code = accessCode
            });
            Session["AccessToken"] = result.access_token;

            var fbInfo = new FacebookClient(result.access_token);
            dynamic me = fbInfo.Get("me?fields=name,email");
            Session["FacebookId"] = me.id;

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            if (Session["AccessToken"] != null)
            {
                var token = Session["AccessToken"].ToString();
                var client = new FacebookClient();

                var logoutUrl = client.GetLogoutUrl(new { access_token = token, next = "http://tracker.aceipse.dk/" });

                Response.Redirect(logoutUrl.AbsoluteUri);
                Session.Clear();
                Session.RemoveAll();
            }

            return RedirectToAction("Index", "Home");
        }
    }
}