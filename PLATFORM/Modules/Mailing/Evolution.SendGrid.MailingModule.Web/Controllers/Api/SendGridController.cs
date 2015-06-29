using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
//using System.Web.Mvc;
using Evolution.SendGrid.MailingModule.Web.Binders;
using Evolution.SendGrid.MailingModule.Web.Models;
using SendGrid;

namespace Evolution.SendGrid.MailingModule.Web.Controllers.Api
{
    [System.Web.Http.RoutePrefix("api/sendgrid")]
    public class SendGridController : ApiController
    {
        //[System.Web.Http.Route("send")]
        [HttpPost]
        [ActionName("send")]
       // public async Task<IHttpActionResult> Send([ModelBinder(typeof(MailModelBinder))] MailModel model, bool isResend = false, string redirectUrl = null)
        public async Task<IHttpActionResult> Send(FormDataCollection form)
        {
            var username = ConfigurationManager.AppSettings["SendGrid:Username"];
            var password = ConfigurationManager.AppSettings["SendGrid:Password"];

            var model = BuildMailModelFromForm(form);

            var message = new SendGridMessage();

            message.AddTo(ConfigurationManager.AppSettings["SendGrid:ToEmail"]);
            message.From = new MailAddress(model.To, model.FullName);
            message.Subject = model.Subject;
            message.Html = model.FullMailBody;

            var credentials = new NetworkCredential(username, password);
            var transportWeb = new global::SendGrid.Web(credentials);
            await transportWeb.DeliverAsync(message);


            return Ok();
        }

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

        private MailModel BuildMailModelFromForm(FormDataCollection form)
        {
            var retVal = new MailModel();


            var allKeys = form.ReadAsNameValueCollection().AllKeys.ToList();

            CheckAndRemoveKeys(allKeys);

            if (allKeys.Contains("FullName"))
                retVal.FullName = form["FullName"];

            retVal.To = form["To"];
            retVal.Subject = form["Subject"];

            var builder = new StringBuilder();
            foreach (var key in allKeys)
            {
                builder.AppendLine(string.Format("{0}: {1} <br>", key, form[key]));
            }

            retVal.MailBody = builder.ToString();

            builder.AppendLine(string.Format("EmailTo: {0}", form["To"]));
            retVal.FullMailBody = builder.ToString();

            return retVal;
        }
    }
}