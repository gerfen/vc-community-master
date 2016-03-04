using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Controllers.Api
{
    public class ApiLiquidPartialViewController : StorefrontControllerBase
    {
        // GET: ApiLiquidPartialView
        public ActionResult Index(string name)
        {
            HttpContext.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            return PartialView(name);
        }

        public ApiLiquidPartialViewController(WorkContext context, IStorefrontUrlBuilder urlBuilder) : base(context, urlBuilder)
        {
        }
    }
}