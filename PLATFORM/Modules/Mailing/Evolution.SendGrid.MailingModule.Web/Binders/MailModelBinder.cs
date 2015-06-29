using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Net.Http;
//using System.Web.Mvc;
using Evolution.SendGrid.MailingModule.Web.Models;

namespace Evolution.SendGrid.MailingModule.Web.Binders
{
    public class MailModelBinder : IModelBinder
    {
      

        //public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        //{
        //    if (controllerContext == null)
        //        throw new ArgumentNullException("controllerContext");

        //    if (bindingContext == null)
        //        throw new ArgumentNullException("bindingContext");

        //    var retVal = new MailModel();

        //    var form = controllerContext.RequestContext.HttpContext.Request.Form;
        //    var allKeys = form.AllKeys.ToList();

        //    CheckAndRemoveKeys(allKeys);

        //    if (allKeys.Contains("FullName"))
        //        retVal.FullName = form["FullName"];

        //    retVal.To = form["To"];
        //    retVal.Subject = form["Subject"];

        //    var builder = new StringBuilder();
        //    foreach (var key in allKeys)
        //    {
        //        builder.AppendLine(string.Format("{0}: {1} <br>", key, form[key]));
        //    }

        //    retVal.MailBody = builder.ToString();

        //    builder.AppendLine(string.Format("EmailTo: {0}", form["To"]));
        //    retVal.FullMailBody = builder.ToString();

        //    return retVal;
        //}

        private string[] RemovedKeys = new[] { "To", "Subject", "IsResend", "RedirectUrl" };
        public void CheckAndRemoveKeys(List<string> allKeys)
        {
            foreach (var removedKey in RemovedKeys)
            {
                if (!allKeys.Contains(removedKey))
                    throw new NullReferenceException(string.Format("No {0} email", removedKey));
                else
                    allKeys.Remove(removedKey);
            }
        }

        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (actionContext == null)
                throw new ArgumentNullException("actionContext");

            if (bindingContext == null)
                throw new ArgumentNullException("bindingContext");

            var retVal = new MailModel();

           var form = actionContext.ActionArguments["formData"] as FormDataCollection;

            var request = actionContext.ControllerContext.Request;
            var content = request.Content;

           // var collection = content.ReadAsAync<FormDataCollection>().Result;
          //  var allKeys = form.

          //  CheckAndRemoveKeys(new List<string>());

            //if (allKeys.Contains("FullName"))
            //    retVal.FullName = form["FullName"];

            //retVal.To = form["To"];
            //retVal.Subject = form["Subject"];

            var builder = new StringBuilder();
            //foreach (var key in allKeys)
            //{
            //    builder.AppendLine(string.Format("{0}: {1} <br>", key, form[key]));
            //}

            retVal.MailBody = builder.ToString();

            builder.AppendLine(string.Format("EmailTo: {0}", form["To"]));
            retVal.FullMailBody = builder.ToString();

            bindingContext.Model = retVal;

            return true;
        }
    }
}
