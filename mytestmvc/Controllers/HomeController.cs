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

            //**********************
            //this cannot work
            //RestClient _client = new RestClient(forgeBaseUrl);
            //RestRequest authReq = new RestRequest();
            //authReq.Resource = "authentication/v1/gettoken";
            //authReq.Method = Method.POST;
            //authReq.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            //authReq.AddParameter("code", code);
            //authReq.AddParameter("grant_type", "authorization_code");
            //authReq.AddParameter("redirect_uri", HttpUtility.UrlEncode(callbackUrl));
            ////authReq.AddParameter("redirect_uri", callbackUrl);
            //authReq.AddParameter("client_id", clientId);
            //authReq.AddParameter("client_secret", clientSecret);
            //IRestResponse httpresult = _client.Execute(authReq);
            //**********************

            var client = new RestClient(forgeBaseUrl+ "authentication/v1/gettoken");
            var request = new RestRequest(Method.POST);
            string encodedBody = string.Format("code={0}&grant_type=authorization_code&client_id={1}&client_secret={2}&redirect_uri={3}",
                code, clientId, clientSecret, HttpUtility.UrlEncode(callbackUrl));
            request.AddParameter("application/x-www-form-urlencoded", encodedBody, ParameterType.RequestBody);
            request.AddParameter("Content-Type", "application/x-www-form-urlencoded", ParameterType.HttpHeader);
            var response = client.Execute<gettoken_response>(request);

            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            { 

                string userid = AtMe(response.Data.access_token);
                userid = "9FBPMRCHFRS5";
                string appid = "8838710715095167987";
                if (userid != "error")
                {
                    bool isvaliduser = CheckEntitlement(userid, appid);
                 }
                else
                {

                }
                //result = Content(userid);
            }
            else
            { 
            } 
            //result = Content("<script>window.opener.location.reload(false);window.close();</script>");
            result = Redirect("\\");
            return result;
        }
         
        string AtMe(string access_token)
        {
            var client = new RestClient(forgeBaseUrl + "userprofile/v1/users/@me");
            var request = new RestRequest(Method.GET);           
            request.AddParameter("Authorization", "Bearer " + access_token, ParameterType.HttpHeader);
            var response = client.Execute<atme_response>(request);
             
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            { 
                string _userId = response.Data.userId;
                string _useremail = response.Data.emailId;

                Session["currentuserid"] = _userId;
                Session["currentuseremail"] = _useremail;

                return _userId + " " + _useremail; 
            }
            else
            {
                Session["currentuseremail"] = "please log in";

                return "error";
             }
        }

        bool CheckEntitlement(string userid, string appid)
        {
            var client = new RestClient("https://apps.autodesk.com/" + "webservices/checkentitlement");
            var request = new RestRequest(Method.GET);
            request.AddParameter("userid", userid);
            request.AddParameter("appid", appid);

            var response = client.Execute<entitlement_response>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //((sessionJson)Session[unique_id]).currentuservalid = response.Data.IsValid; 
                  
                return response.Data.IsValid;

            }
            else
            {
                return false;
            } 
        }



        [HttpGet]
        public ActionResult checkuservalid(string unique_id)
        {
            ActionResult result = null;

            if(Session[unique_id] == null)
            {
                Session[unique_id] = new sessionJson();
                
            }

            result = Json(new
            {
                currentuseremail = ((sessionJson)Session[unique_id]).currentuseremail,
                currentuserid = ((sessionJson)Session[unique_id]).currentuserid,
                currentuservalid = ((sessionJson)Session[unique_id]).currentuservalid
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
        
         [HttpPost]
        public ActionResult ForgeLogout()
        { 
            //doing...
            return Content("ok"); 
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
