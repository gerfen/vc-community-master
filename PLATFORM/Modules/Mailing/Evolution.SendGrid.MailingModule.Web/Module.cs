using Evolution.SendGrid.MailingModule.Web.Controllers.Api;
using Microsoft.Practices.Unity;
using VirtoCommerce.Platform.Core.Modularity;

namespace Evolution.SendGrid.MailingModule.Web
{
    public class Module : IModule
    {
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public void SetupDatabase(SampleDataLevel sampleDataLevel)
        {
        }

        public void Initialize()
        {
            _container.RegisterType<SendGridController>();
        }

        public void PostInitialize()
        {
        }

        #endregion
    }
}