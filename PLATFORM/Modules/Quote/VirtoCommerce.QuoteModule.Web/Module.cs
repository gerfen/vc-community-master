﻿using System;
using System.IO;
using Microsoft.Practices.Unity;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Domain.Quote.Events;
using VirtoCommerce.Domain.Quote.Services;
using VirtoCommerce.Domain.Store.Model;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.Platform.Data.Repositories;
using VirtoCommerce.QuoteModule.Data.Observers;
using VirtoCommerce.QuoteModule.Data.Repositories;
using VirtoCommerce.QuoteModule.Data.Services;
using VirtoCommerce.QuoteModule.Web.ExportImport;
using dataModel = VirtoCommerce.QuoteModule.Data.Model;

namespace VirtoCommerce.QuoteModule.Web
{
    public class Module : ModuleBase, ISupportExportImportModule
    {
        private const string _connectionStringName = "VirtoCommerce";
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        #region IModule Members

        public override void SetupDatabase()
        {
            using (var context = new QuoteRepositoryImpl(_connectionStringName, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<QuoteRepositoryImpl, Data.Migrations.Configuration>();
                initializer.InitializeDatabase(context);
            }
        }

        public override void Initialize()
        {
            var extensionManager = new DefaultPricingExtensionManagerImpl();
            _container.RegisterInstance<IPricingExtensionManager>(extensionManager);

            _container.RegisterType<IQuoteTotalsCalculator, DefaultQuoteTotalsCalculator>();

            _container.RegisterType<IQuoteRepository>(new InjectionFactory(c => new QuoteRepositoryImpl(_connectionStringName, new EntityPrimaryKeyGeneratorInterceptor(), _container.Resolve<AuditableInterceptor>())));
            _container.RegisterType<IQuoteRequestService, QuoteRequestServiceImpl>();

            _container.RegisterType<IEventPublisher<QuoteRequestChangeEvent>, EventPublisher<QuoteRequestChangeEvent>>();
            //Log quote request changes
            _container.RegisterType<IObserver<QuoteRequestChangeEvent>, LogQuoteChangesObserver>("LogQuoteChangesObserver");
        }


        public override void PostInitialize()
        {
            base.PostInitialize();
            //Create EnableQuote dynamic propertiy for  Store 
            var dynamicPropertyService = _container.Resolve<IDynamicPropertyService>();
            var enableQuotesProperty = new DynamicProperty
            {
                Id = "Quote_Enable_Property",
                Name = "EnableQuotes",
                ObjectType = typeof(Store).FullName,
                ValueType = DynamicPropertyValueType.Boolean,
                CreatedBy = "Auto",
            };

            dynamicPropertyService.SaveProperties(new[] { enableQuotesProperty });
        }
        #endregion

        #region ISupportExportImportModule Members

        public string ExportDescription
        {
            get
            {
                var settingManager = _container.Resolve<ISettingsManager>();
                return settingManager.GetValue("Quotes.ExportImport.Description", String.Empty);
            }
        }

        public void DoExport(Stream outStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var job = _container.Resolve<QuoteExportImport>();
            job.DoExport(outStream, progressCallback);
        }

        public void DoImport(Stream inputStream, PlatformExportManifest manifest, Action<ExportImportProgressInfo> progressCallback)
        {
            var job = _container.Resolve<QuoteExportImport>();
            job.DoImport(inputStream, progressCallback);
        }

        #endregion

    }
}
