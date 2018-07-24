using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using REORGCHART.Data;
using REORGCHART.Models;

using Newtonsoft.Json;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Dynamic;

namespace REORGCHART.Controllers
{
    public class VersionController : Controller
    {
        Models.DBContext db = new Models.DBContext();
        ApplicationUser UserData = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());

        Common comClass = new Common();

        // GET: Version
        public ActionResult Index()
        {
            return View();
        }

        // GET: Version
        public ActionResult Upload()
        {
            return View();
        }

        public string[] CheckFields(string FileName)
        {
            string[] FieldsInf = { "", "" };
            string MissingFields = "", ShowFields = "";
            var UploadExcelFile = (from uef in db.UploadFiles
                                   where uef.CompanyName == UserData.CompanyName
                                   select uef).OrderByDescending(x => x.CreatedDate).FirstOrDefault();

            using (StreamReader reader = new StreamReader(FileName))
            {
                string jsonData = reader.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(jsonData);
                int Index = 0;
                foreach (var item in array.data)
                {
                    if (UploadExcelFile.UseFields != "")
                    {
                        Index++;
                        string[] UF = UploadExcelFile.UseFields.Split(',');
                        foreach (string strUF in UF)
                        {
                            string strField = strUF.Trim().ToUpper().Replace(" ", "_");
                            if (!(strField == "LEVEL_ID" || strField == "PARENT_LEVEL_ID" || strField == "VERSION" ||
                                  strField == "FULL_NAME" || strField == "DATE_UPDATED" || strField == "USER_ID"))
                            {
                                try
                                {
                                    if (item[strUF] == null)
                                        MissingFields += "," + Index.ToString() + "~:~" + strUF;
                                    else
                                    {
                                        if (Index == 1) ShowFields += "," + strUF;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MissingFields += "," + Index.ToString() + "~:~" + strUF;
                                }
                            }
                        }
                    }

                }
            }

            FieldsInf[0] = (MissingFields == "") ? "" : MissingFields.Substring(1);
            FieldsInf[1] = (ShowFields == "") ? "" : ShowFields.Substring(1);

            return FieldsInf;
        }

        [HttpPost]
        public ActionResult UploadFile()
        {
            string[] Fields = { "", "" };
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int Idx = 0; Idx < files.Count; Idx++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[Idx];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }

                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/App_Data/Uploads/"), fname);
                        file.SaveAs(fname);

                        Fields=CheckFields(fname);
                    }

                    // Returns message that successfully uploaded  
                    return Json(new
                    {
                        Success = (Fields[0] == "") ? "Yes" : "No",
                        Message= (Fields[0] != "") ? "Missing Fields" : "",
                        MF= Fields[0],
                        SF = Fields[1]
                    });

                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        Success = "No",
                        Message = ex.Message,
                        MF = "",
                        SF = Fields[1]
                    });
                }
            }
            else
            {
                return Json(new
                {
                    Success = "No",
                    Message = "No files selected.",
                    MF = "",
                    SF = Fields[1]
                });
            }
        }
    }
}