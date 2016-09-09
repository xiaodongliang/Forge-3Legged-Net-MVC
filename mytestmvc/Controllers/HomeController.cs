using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace mytestmvc.Controllers
{
    public class User1
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Mail { get; set; }
    }

    [Serializable]
    public class gettoken_response
    {
        // This is the mo_file information for the BIM 360 Glue Model
        public string access_token { get; set; }
       
    }

    [Serializable]
    public class atme_response
    {
        // This is the mo_file information for the BIM 360 Glue Model
        public string userId { get; set; }
        public string emailId { get; set; }
        public string userName { get; set; }
    }

    [Serializable]
    public class entitlement_response
    {
        // This is the mo_file information for the BIM 360 Glue Model
        public string UserId { get; set; }
        public string AppId { get; set; }
        public bool IsValid { get; set; }
    }
    public class HomeController : Controller
    {


        private IList<User1> GetUsers()
        {
            var users = new List<User1>();
            users.AddRange(new User1[] {
                new User1() {ID = Guid.NewGuid(),Name = "a",Password ="a",Mail="a@cleversoft.com"},
                new User1() {ID = Guid.NewGuid(),Name = "b",Password ="b",Mail="b@cleversoft.com"},
                new User1() {ID = Guid.NewGuid(),Name = "c",Password ="c",Mail="c@cleversoft.com"},
                new User1() {ID = Guid.NewGuid(),Name = "d",Password ="d",Mail="d@cleversoft.com"},
                new User1() {ID = Guid.NewGuid(),Name = "e",Password ="e",Mail="e@cleversoft.com"}
            });

            return users;
        }



        private string forgeBaseUrl = Properties.Settings.Default.ForgeBaseUrl;
        private string clientId = Properties.Settings.Default.ForgeViewerClientId;
        private string clientSecret = Properties.Settings.Default.ForgeViewerClientSecret;
        private string callbackUrl = Properties.Settings.Default.ForgeCallbackUrl;

         

        [HttpGet]
        public ActionResult ForgeAuthorize()
        {
            ActionResult result = null;
             
            string Output = forgeBaseUrl + "authentication/v1/authorize?" +
                "redirect_uri=" + HttpUtility.UrlEncode(callbackUrl) +
                "&scope=" + HttpUtility.UrlEncode("data:read data:create data:write bucket:read bucket:create") +
                "&client_id=" + clientId +
                "&response_type=code";

            result = Json(new { authorizeurl = Output }, JsonRequestBehavior.AllowGet);
            return result;
        }

        [HttpGet]
        public ActionResult callback(string code)
        {
            ActionResult result = null;
            
            RestClient _client = new RestClient(forgeBaseUrl);

            RestRequest authReq = new RestRequest();
            authReq.Resource = "authentication/v1/gettoken";
            authReq.Method = Method.POST;
            authReq.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            authReq.AddParameter("client_id", clientId);
            authReq.AddParameter("client_secret", clientSecret);
            authReq.AddParameter("grant_type", "authorization_code");
            authReq.AddParameter("redirect_uri", HttpUtility.UrlEncode(callbackUrl));
            authReq.AddParameter("code", HttpUtility.UrlEncode(code));

            IRestResponse httpresult = _client.Execute(authReq);
            if (httpresult.StatusCode == System.Net.HttpStatusCode.OK)
            {                 
                string responseString = httpresult.Content;
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();

                gettoken_response jsonObj = (gettoken_response)json_serializer.DeserializeObject(responseString);

                //string userid = AtMe(jsonObj.access_token);
                //string appid = "";
                //if (userid != "error")
                //{
                //bool isvaliduser = CheckEntitlement(userid, appid);
                // }
                // else
                // {

                // }


            }
            else
            {
                //just for test
                Session["currentuseremail"] = httpresult.Content  + " " + code;
                Session["currentuservalid"] = true;                 
            }
             

            result = Content("<script>window.opener.location.reload(false);window.close();</script>");
            return result;
        }
         
        string AtMe(string access_token)
        {
            RestClient _client = new RestClient(forgeBaseUrl);

            RestRequest authReq = new RestRequest();
            authReq.Resource = "userprofile/v1/users/@me";
            authReq.Method = Method.GET;
            authReq.AddHeader("Authorization", "Bearer " + access_token);

            IRestResponse httpresult = _client.Execute(authReq);
            if (httpresult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string responseString = httpresult.Content;
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();

                atme_response jsonObj = (atme_response)json_serializer.DeserializeObject(responseString);

                string _userId = jsonObj.userId;
                string _useremail = jsonObj.emailId;

                Session["currentuserid"] = _userId;
                Session["currentuseremail"] = _useremail;

                return _userId; 
            }
            else
            {
                return "error";
             }

        }

        bool CheckEntitlement(string userid, string appid)
        {
            RestClient _client = new RestClient("https://apps.autodesk.com/");

            RestRequest authReq = new RestRequest();
            authReq.Resource = "webservices/checkentitlement";
            authReq.Method = Method.GET;
            authReq.AddParameter("userid", userid);
            authReq.AddParameter("appid", appid);

            IRestResponse httpresult = _client.Execute(authReq);
            if (httpresult.StatusCode == System.Net.HttpStatusCode.OK)
            {

                string responseString = httpresult.Content;

                JavaScriptSerializer json_serializer = new JavaScriptSerializer();

                entitlement_response jsonObj = (entitlement_response)json_serializer.DeserializeObject(responseString);

                Session["currentuserid"] = jsonObj.IsValid; 


                return jsonObj.IsValid;
                //result = Json(new { access_token = _accessToken }, JsonRequestBehavior.AllowGet); ;

            }
            else
            {
                return false; ;
                //result = Json(new { error = httpresult.StatusCode.ToString() }, JsonRequestBehavior.AllowGet); ;
            }

        }



        [HttpGet]
        public ActionResult checkuservalid()
        {
            ActionResult result = null;

            result = Json(new
            {
                currentuseremail = Session["currentuseremail"],
                currentuserid = Session["currentuserid"],
                currentuservalid = Session["currentuservalid"]
            },
            JsonRequestBehavior.AllowGet);

            return result;
        }
        [HttpGet]
        public ActionResult forgecallbacktest()
        {
            ActionResult result = null;
             
            result = Json(new { mycode = "forgecallbacktest" }, JsonRequestBehavior.AllowGet);



            return result;
        }
         

        public ActionResult Index()
        {
            //Session["currentuseremail"] = "";
            //Session["currentuserid"] = "";
            //Session["currentuservalid"] = false;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
