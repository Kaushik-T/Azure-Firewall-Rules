using System;
using System.Collections.Generic;
using System.Data;
//using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Configuration;
using System.Xml;
using DevOps.Web.DAL;
using DevOps.Web.Models;

namespace DevOps.Web.Controllers
{
    [Authorize]
    public class FirewallController : Controller
    {
        // GET: Firewall
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult dbconnect(string Subscribtions_p, string Server, string User_Name_p, string Ip_test_P)
        {
            string XML_Path = ConfigurationManager.AppSettings.Get("XMLPath");
            XDocument doc = XDocument.Load(XML_Path);

            List<string> allDb = new List<string>();
            List<string> allUser = new List<string>();
            List<string> allPassword = new List<string>();

            var Server_Name = Server;
            var Subscribtions = Subscribtions_p;
            var User_Name = User_Name_p;
            var Ip_test = Ip_test_P;


            var Config = doc.Descendants("MasterDB");

            foreach (var e in Config)
            {
                allDb.Add(e.Value);
            }
            String DB_string = (string)allDb[1];




            var U_list = doc.Descendants("Environment").Where(s => (string)s.Element("ServerName") == Server_Name).Select(s => s.Element("MasterDBUserName"));
            foreach (var e in U_list)
            {
                allUser.Add(e.Value);
            }
            String DB_User = (string)allUser[0];

            var P_list = doc.Descendants("Environment").Where(s => (string)s.Element("ServerName") == Server_Name).Select(s => s.Element("MasterDBPassword"));
            foreach (var e in P_list)
            {
                allPassword.Add(e.Value);
            }
            String DB_Pass = (string)allPassword[0];

         
            string sql_add = null;
            string sql_show = null;
         //test   string Ip_test2 = "50.202.178.92";
            string connetionString = "Data Source=" + Server_Name.ToString() + ";Initial Catalog=" + DB_string + ";Integrated Security=False;User Id=" + DB_User + ";Password=" + DB_Pass + ";MultipleActiveResultSets=True";

            sql_show = @"SELECT * FROM sys.firewall_rules WHERE start_ip_address='" + Ip_test + "'";

            sql_add = @"EXECUTE sp_set_firewall_rule @name = N'" + User_Name.ToString() + "', @start_ip_address = '" + Ip_test + "', @end_ip_address = '" + Ip_test + "'";


            
            SqlCommand cmd1 = new SqlCommand();
            cmd1.CommandText = sql_show;
            
            SQLDataProvider objSQLDP = new SQLDataProvider();
            try
            {
                var getValue = objSQLDP.ExecuteQueryWithCustomRetry(cmd1, connetionString, Constants.Scalar);
            
            if (getValue != null)
                {

                    String show_result = getValue.ToString();
                    

                    string result = "Firewall rule already exists for current IP ! check connection!";

                    return new JsonResult() { Data = result };


                }
                else
                {
                    SqlCommand cmd2 = new SqlCommand();
                    cmd2.CommandText = sql_add;
                   
                    SQLDataProvider objSQLDP2 = new SQLDataProvider();
                    var result = objSQLDP2.ExecuteQueryWithCustomRetry(cmd2, connetionString, Constants.NonQuery);

                

                   
                    string result1 = "Firewall rule updated ! check connection in 5 mins";

                    return new JsonResult() { Data = result1 };
                }


            }
            catch (Exception ex)
            {
                string result = "Error in SQL Query - Retry in 5 Mins!  or Contact RM Team for immediate assitance! \n" + ex;

                return new JsonResult() { Data = result };
            }




        }




        public JsonResult server_list(string value)
        {
            string XML_Path = ConfigurationManager.AppSettings.Get("XMLPath");
            XDocument doc = XDocument.Load(XML_Path);


            List<string> server_list = new List<string>();


            var s_list = doc.Descendants("Environment").Where(s => (string)s.Element("ConfigurationDb") == value).Select(s => s.Element("ServerName"));


            foreach (var e in s_list)
            {
                server_list.Add(e.Value);
            }



            return new JsonResult() { Data = server_list };


        }


        public ActionResult FirewallSetup()
        {
            List<string> allConfigurationDb = new List<string>();

            string XML_Path = ConfigurationManager.AppSettings.Get("XMLPath");
            XDocument doc = XDocument.Load(XML_Path);


            var Config = doc.Descendants("ConfigurationDb");

            foreach (var e in Config)
            {
                allConfigurationDb.Add(e.Value);
            }


            ViewBag.Configs = allConfigurationDb;

            return View();
        }
    }
}