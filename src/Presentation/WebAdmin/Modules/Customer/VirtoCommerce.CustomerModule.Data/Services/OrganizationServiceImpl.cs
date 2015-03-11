﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using foundationModel = VirtoCommerce.Foundation.Customers.Model;
using coreModel = VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Foundation.Data.Infrastructure;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Converters;
using System.Data.Entity;

namespace VirtoCommerce.CustomerModule.Data.Services
{
	public class OrganizationServiceImpl : ServiceBase, IOrganizationService
	{
		private readonly Func<IFoundationCustomerRepository> _repositoryFactory;
		public OrganizationServiceImpl(Func<IFoundationCustomerRepository> repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		#region IContactService Members

		public coreModel.Organization GetById(string id)
		{
			coreModel.Organization retVal = null;
			using (var repository = _repositoryFactory())
			{
				var entity = repository.GetOrganizationById(id);
				if (entity != null)
				{
					retVal = entity.ToCoreModel();
				}
			}

			return retVal;
		}

		public coreModel.Organization Create(coreModel.Organization organization)
		{
			var entity = organization.ToFoundation();
			coreModel.Organization retVal = null;
			using (var repository = _repositoryFactory())
			{
				repository.Add(entity);
				CommitChanges(repository);
			}
			retVal = GetById(organization.Id);
			return retVal;
		}

		public void Update(coreModel.Organization[] organizations)
		{
			using (var repository = _repositoryFactory())
			using (var changeTracker = base.GetChangeTracker(repository))
			{
				foreach (var organization in organizations)
				{
					var sourceEntity = organization.ToFoundation();
					var targetEntity = repository.GetOrganizationById(organization.Id);
					if (targetEntity == null)
					{
						throw new NullReferenceException("targetEntity");
					}

					changeTracker.Attach(targetEntity);
					sourceEntity.Patch(targetEntity);
				}
				CommitChanges(repository);
			}
		}

		public void Delete(string[] ids)
		{
			using (var repository = _repositoryFactory())
			{
				foreach (var id in ids)
				{
					var entity = repository.GetOrganizationById(id);
					repository.Remove(entity);
				}
				CommitChanges(repository);
			}
		}
		#endregion
	
	}
}