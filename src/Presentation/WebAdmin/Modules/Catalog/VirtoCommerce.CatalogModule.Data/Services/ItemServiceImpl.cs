﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using VirtoCommerce.CatalogModule.Data.Converters;
using VirtoCommerce.Foundation;
using VirtoCommerce.Foundation.Frameworks;
using VirtoCommerce.Framework.Web.Settings;
using foundation = VirtoCommerce.Foundation.Catalogs.Model;
using foundationConfig = VirtoCommerce.Foundation.AppConfig.Model;
using module = VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Foundation.Frameworks.Extensions;
using System.Collections.Generic;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Foundation.Data.Infrastructure;

namespace VirtoCommerce.CatalogModule.Data.Services
{
	public class ItemServiceImpl : ServiceBase, IItemService
	{
        #region Constants

        public const string ItemsCacheKey = "I:{0}:rg:{1}";
        public const string CatalogCacheKey = "C:{0}";

        #endregion

		private readonly Func<IFoundationCatalogRepository> _catalogRepositoryFactory;
		private readonly Func<IFoundationAppConfigRepository> _appConfigRepositoryFactory;
        private readonly ISettingsManager _settingsManager;
        private readonly ICacheRepository _cache;

		public ItemServiceImpl(Func<IFoundationCatalogRepository> catalogRepositoryFactory, Func<IFoundationAppConfigRepository> appConfigRepositoryFactory, 
							  ISettingsManager settingsManager, ICacheRepository cache)
		{
			_catalogRepositoryFactory = catalogRepositoryFactory;
			_appConfigRepositoryFactory = appConfigRepositoryFactory;
		    _settingsManager = settingsManager;
		    this._cache = cache;
		}

		#region IItemService Members

		public module.CatalogProduct GetById(string itemId, module.ItemResponseGroup respGroup)
		{
		    var results = this.GetByIds(new[] { itemId }, respGroup);
		    return !results.Any() ? null : results[0];
		}

        /// <summary>
        /// This method is used by both frontend merchandising services and backend.
        /// </summary>
        /// <param name="itemIds"></param>
        /// <param name="respGroup"></param>
        /// <returns></returns>
        public module.CatalogProduct[] GetByIds(string[] itemIds, module.ItemResponseGroup respGroup)
        {
            // TODO: Optimize performance (Sasha)
            // 1. Catalog should be cached and not retrieved every time from the db
            // 2. SEO info can be retrieved for all items at once instead of one by one
            // 3. Optimize how main variation is loaded
            // 4. Associations shouldn't be loaded always and must be optimized as well
            // 5. No need to get properties meta data to just retrieve property ID
            var retVal = new List<module.CatalogProduct>();
            using (var repository = _catalogRepositoryFactory())
            using (var appConfigRepository = _appConfigRepositoryFactory())
            {
                var dbItems = repository.GetItemByIds(itemIds, respGroup);
                foreach (var dbItem in dbItems)
                {
                    var dbCatalog = repository.GetCatalogById(dbItem.CatalogId);

                    // Sasha: products passed here are only products and not variation, espcially in the list
                    //var parentItemRelation = repository.ItemRelations.FirstOrDefault(x => x.ChildItemId == dbItem.ItemId);
                    //var parentItemId = parentItemRelation == null ? null : parentItemRelation.ParentItemId;

                    foundation.Item[] dbVariations = null;
                    if ((respGroup & module.ItemResponseGroup.Variations) == module.ItemResponseGroup.Variations)
                    {
                        // 
                        /* dbVariations = repository.GetAllItemVariations(parentItemId ?? dbItem.ItemId); * */

                        dbVariations = repository.GetAllItemVariations(dbItem.ItemId);
                        //When user load not main product need to include main product in variation list and exclude current
                        //if (parentItemId != null)
                        //{
                        //    var dbMainItem = repository.GetItemByIds(new[] { parentItemId }, respGroup).FirstOrDefault();
                        //    dbVariations = dbVariations.Concat(new[] { dbMainItem }).Where(x => x.ItemId != dbItem.ItemId).ToArray();
                        //}

                        //Need this for add main product to variations list except current  
                        //dbVariations = dbVariations.Concat(new[] { dbItem }).Where(x => x.ItemId != dbItem.ItemId).ToArray();
                    }

                    foundationConfig.SeoUrlKeyword[] seoInfos = null;
                    if ((respGroup & module.ItemResponseGroup.Seo) == module.ItemResponseGroup.Seo)
                    {
                        seoInfos = appConfigRepository.GetAllSeoInformation(dbItem.ItemId);
                    }

                    var associatedProducts = new List<module.CatalogProduct>();
                    if ((respGroup & module.ItemResponseGroup.ItemAssociations)
                        == module.ItemResponseGroup.ItemAssociations)
                    {
                        if (dbItem.AssociationGroups.Any())
                        {
                            foreach (var association in dbItem.AssociationGroups.SelectMany(x => x.Associations))
                            {
                                var associatedProduct = GetById(association.ItemId, module.ItemResponseGroup.ItemAssets);
                                associatedProducts.Add(associatedProduct);
                            }
                        }
                    }

                    var catalog = dbCatalog.ToModuleModel();

                    /*
                    if (dbItem.CategoryItemRelations.Any())
                    {
                        var dbCategory = repository.GetCategoryById(dbItem.CategoryItemRelations.OrderBy(x => x.Priority).First().CategoryId);
                        var dpProperties = repository.GetAllCategoryProperties(dbCategory);
                        var properties = dpProperties.Select(x => x.ToModuleModel(catalog, dbCategory.ToModuleModel(catalog))).ToArray();
                        var category = dbCategory.ToModuleModel(catalog, properties);

                        retVal.Add(dbItem.ToModuleModel(catalog, category, properties, dbVariations, seoInfos, parentItemId, associatedProducts.ToArray()));
                    }
                    else
                    {
                        retVal.Add(dbItem.ToModuleModel(catalog, null, null, dbVariations, seoInfos, parentItemId, associatedProducts.ToArray()));
                    }
                     * */

                    retVal.Add(dbItem.ToModuleModel(catalog, null, null, dbVariations, seoInfos, null, associatedProducts.ToArray()));
                }
            }

            return retVal.ToArray();
        }

		public module.CatalogProduct Create(module.CatalogProduct item)
		{
			var dbItem = item.ToFoundation();

			using (var repository = _catalogRepositoryFactory())
			using (var appConfigRepository = _appConfigRepositoryFactory())
			{
				if (item.CategoryId != null)
				{
					//Category relation
					var dbCategory = repository.GetCategoryById(item.CategoryId);
					if (dbCategory == null)
					{
						throw new NullReferenceException("dbCategory");
					}
					repository.SetItemCategoryRelation(dbItem, dbCategory);
				}

				//Variation relation
				if (item.MainProductId != null)
				{
					repository.SetVariationRelation(dbItem, item.MainProductId);
				}

				//Need add seo separately
				if (item.SeoInfos != null)
				{
					foreach (var seoInfo in item.SeoInfos)
					{
						var dbSeoInfo = seoInfo.ToFoundation(item);
						appConfigRepository.Add(dbSeoInfo);
					}
				}

				repository.Add(dbItem);

				CommitChanges(repository);
				CommitChanges(appConfigRepository);
			}

			var retVal = GetById(dbItem.ItemId, module.ItemResponseGroup.ItemLarge);
			return retVal;
		}

		public void Update(module.CatalogProduct[] items)
		{
			using (var repository = _catalogRepositoryFactory())
			using (var appConfigRepository = _appConfigRepositoryFactory())
			using (var changeTracker = base.GetChangeTracker(repository))
			{
				var dbItems = repository.GetItemByIds(items.Select(x => x.Id).ToArray(), module.ItemResponseGroup.ItemLarge);
				foreach (var dbItem in dbItems)
				{
					var item = items.FirstOrDefault(x => x.Id == dbItem.ItemId);
					if (item != null)
					{
						changeTracker.Attach(dbItem);

						var dbItemChanged = item.ToFoundation();
						dbItemChanged.Patch(dbItem);

						//Variation relation update
						if (item.MainProductId != null)
						{
							repository.SetVariationRelation(dbItem, item.MainProductId);
						}
						//else
						//{
						//	//Switch item like a  main product
						//	repository.SwitchProductToMain(dbItem);
						//}
					}

					//Patch seoInfo
					if (item.SeoInfos != null)
					{
						var dbSeoInfos = new ObservableCollection<foundationConfig.SeoUrlKeyword>(appConfigRepository.GetAllSeoInformation(item.Id));
						var changedSeoInfos = item.SeoInfos.Select(x => x.ToFoundation(item)).ToList();
						dbSeoInfos.ObserveCollection(x => appConfigRepository.Add(x), x => appConfigRepository.Remove(x));

						changedSeoInfos.Patch(dbSeoInfos, new SeoInfoComparer(), (source, target) => source.Patch(target));
					}
				}
				CommitChanges(repository);
				CommitChanges(appConfigRepository);
			}

		}

		public void Delete(string[] itemIds)
		{
			using (var repository = _catalogRepositoryFactory())
			{
				repository.RemoveItems(itemIds);
				CommitChanges(repository);
			}
		}

		#endregion
    }
}
