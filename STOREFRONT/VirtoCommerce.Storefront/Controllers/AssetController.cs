using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Mvc;
using DotLiquid;
using LibSassNetProxy;
using VirtoCommerce.LiquidThemeEngine;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.Storefront.Controllers
{
    [OutputCache(CacheProfile = "AssetsCachingProfile")]
    public class AssetController : Controller
    {
        private readonly ILiquidThemeEngine _themeEngine;
        private readonly WorkContext _workContext;
        public AssetController(ILiquidThemeEngine themeEngine, WorkContext workContext)
        {
            _themeEngine = themeEngine;
            _workContext = workContext;
        }

        #region Public Methods and Operators

        /// <summary>
        /// GET: /themes/assets/{asset}
        /// Need handle all assets requests because it may be liquid and scss files which should be preprocessed
        /// </summary>
        /// <param name="theme"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetAssets(string asset)
        {
            var stream = _themeEngine.GetAssetStream(asset);
            if(stream != null)
            {
                return base.File(stream, MimeMapping.GetMimeMapping(asset));
            }
            throw new HttpException(404, asset);
        }

        /// <summary>
        /// GET: /themes/global/assets/{asset}
        /// </summary>
        [HttpGet]
        public ActionResult GetGlobalAssets(string asset)
        {
           
            var stream = _themeEngine.GetAssetStream(asset, searchInGlobalThemeOnly: true);
            if (stream != null)
            {
                return base.File(stream, MimeMapping.GetMimeMapping(asset));
            }
            throw new HttpException(404, asset);
        }

        [HttpGet]
        public ActionResult Template(string name)
        {
            HttpContext.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            return PartialView($"{name}:{name}_layout");
            //return PartialView($"{name}");
        }

        #endregion

    }
}
