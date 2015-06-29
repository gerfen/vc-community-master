using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SendGrid;
using VirtoCommerce.Web.Binders;
using VirtoCommerce.Web.Models;

namespace VirtoCommerce.Web.Controllers
{
    [RoutePrefix("mail")]
    public class MailController : Controller
    {
        [Route("send")]
        public async Task<ActionResult> Send([ModelBinder(typeof (MailModelBinder))] MailModel model, bool isResend,
            string redirectUrl)
        {
            var username = ConfigurationManager.AppSettings["SendGrid:Username"];
            var password = ConfigurationManager.AppSettings["SendGrid:Password"];
          
            var message = new SendGridMessage();

            message.AddTo(ConfigurationManager.AppSettings["SendGrid:ToEmail"]);
            message.From = new MailAddress(model.To, model.FullName);
            message.Subject = model.Subject;
            message.Html = model.FullMailBody;

            var credentials = new NetworkCredential(username, password);
            var transportWeb = new SendGrid.Web(credentials);
            await transportWeb.DeliverAsync(message);

            /*
			if (isResend)
			{
				message = new SendGridMessage();
				message.AddTo(model.To);
				message.From = new MailAddress(ConfigurationManager.AppSettings["FromEmail"]);
				message.Subject = model.Subject;
				if (string.IsNullOrEmpty(model.MailBody))
					message.Html = "Thank you";
				else
					message.Html = model.MailBody;
				transportWeb.Deliver(message);
			}
			 * */

            return Redirect(redirectUrl);
        }
    }
}