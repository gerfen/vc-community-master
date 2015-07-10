using Evolution.SendGrid.MailingModule.Web.Controllers.Api;
using Microsoft.Practices.Unity;
using VirtoCommerce.Platform.Core.Modularity;

namespace Evolution.SendGrid.MailingModule.Web
{
    public class Module : ModuleBase
    {
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

      

        public override void Initialize()
        {
            _container.RegisterType<SendGridController>();
        }

       

        

        #endregion
    }
}