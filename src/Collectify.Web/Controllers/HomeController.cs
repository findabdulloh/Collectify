using Collectify.Service.IServices.IItems;
using Collectify.Service.IServices;
using Microsoft.AspNetCore.Mvc;
using Collectify.Service.IServices.IUsers;
using Collectify.Service.DTOs.Paginations;
using Collectify.Web.Models;

public class HomeController : Controller
{
    private readonly IItemService _itemService;
    private readonly ICollectionService _collectionService;
    private readonly IAuthorizationService _authorizationService;

    public HomeController(ICollectionService collectionService, IItemService itemService, IAuthorizationService authorizationService)
    {
        _itemService = itemService;
        _collectionService = collectionService;
        _authorizationService = authorizationService;
    }

    public async Task<IActionResult> Index(HomeIndexViewModel viewModel)
    {
        var largestCollections = (await _collectionService
            .GetAllAsync(new PaginationParams { PageIndex = 1, PageSize = 5 })).Result.Data;

        var latestItems = (await _itemService
            .GetAllAsync(new PaginationParams { PageIndex = 1, PageSize = 5 })).Result;

        var tags = new List<string> { "sla", "sa", "okok"};

        viewModel.TopCollections = largestCollections;
        viewModel.LatestItems = latestItems;
        viewModel.Tags = tags;

        return View(viewModel);
    }
}
