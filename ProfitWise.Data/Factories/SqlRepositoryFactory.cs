using System;
using ProfitWise.Data.Repositories;

namespace ProfitWise.Data.Factories
{
    public class SqlRepositoryFactory
    {
        private readonly Func<ProductDataRepository> _productDataRepositoryFactory;
        private readonly Func<VariantDataRepository> _variantDataRepositoryFactory;


        public SqlRepositoryFactory(
            Func<ProductDataRepository> productDataRepositoryFactory,
            Func<VariantDataRepository> variantDataRepositoryFactory)
        {
            _productDataRepositoryFactory = productDataRepositoryFactory;
            _variantDataRepositoryFactory = variantDataRepositoryFactory;
        }

        public virtual ProductDataRepository MakeProductDataRepository(string userId)
        {
            var repository = _productDataRepositoryFactory();
            repository.UserId = userId;
            return repository;
        }

        public virtual VariantDataRepository MakeVariantDataRepository(string userId)
        {
            var repository = _variantDataRepositoryFactory();
            repository.UserId = userId;
            return repository;
        }
    }
}

