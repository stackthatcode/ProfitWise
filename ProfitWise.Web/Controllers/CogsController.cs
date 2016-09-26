﻿using System;
using System.Linq;
using System.Web.Mvc;
using ProfitWise.Data.Factories;
using ProfitWise.Data.Model;
using ProfitWise.Data.Repositories;
using ProfitWise.Data.Services;
using ProfitWise.Web.Attributes;
using ProfitWise.Web.Models;
using Push.Foundation.Web.Json;

namespace ProfitWise.Web.Controllers
{
    [Authorize(Roles = "ADMIN, USER")]
    public class CogsController : Controller
    {
        private readonly MultitenantFactory _factory;
        private readonly CurrencyService _currencyService;

        public CogsController(MultitenantFactory factory, CurrencyService currencyService)
        {
            _factory = factory;
            _currencyService = currencyService;
        }

        [HttpGet]
        public ActionResult Vendors()
        {
            var userBrief = HttpContext.PullUserBriefFromContext();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.Shop);
            var vendors = cogsRepository.RetrieveVendors();

            return new JsonNetResult(vendors);
        }


        [HttpPost]
        public ActionResult Search(CogsSearchParameters parameters)
        {
            var userBrief = HttpContext.PullUserBriefFromContext();
            var cogsRepository = _factory.MakeCogsRepository(userBrief.Shop);

            using (var transaction = cogsRepository.InitiateTransaction())
            {
                var terms =
                    (parameters.Text ?? "")
                        .Split(' ')
                        .Select(x => x.Trim())
                        .Where(x => x != "")
                        .ToList();
                var recordCount = cogsRepository.InsertPickList(terms);

                var products =
                    cogsRepository.RetrieveMasterProducts(
                        parameters.PageNumber, parameters.PageSize, parameters.SortByColumn, parameters.SortByDirectionDown);

                var variants =
                    cogsRepository
                        .RetrieveMasterVariants(products.Select(x => x.PwMasterProductId).ToList());

                foreach (var variant in variants)
                {
                    if (variant.CogsAmount != null && variant.CogsCurrencyId != null)
                    {
                        variant.NormalizedCogsAmount =
                            _currencyService.Convert(
                                variant.CogsAmount.Value,
                                variant.CogsCurrencyId.Value,
                                userBrief.Shop.CurrencyId,
                                DateTime.Now);
                    }
                }

                products.PopulateVariants(variants);

                var model = products.ToCogsGridModel(userBrief.Shop.CurrencyId);

                return new JsonNetResult(new {products = model, totalRecords = recordCount});
            }
        }
    }
}

