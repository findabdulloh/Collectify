using Collectify.Domain.Entities.Items.Basics;
using Collectify.Service.DTOs.Collections;
using Collectify.Service.DTOs.Paginations;
using Collectify.Service.DTOs.Users;
using Collectify.Service.IServices;
using Collectify.Service.IServices.IItems;
using Collectify.Service.IServices.IUsers;
using Collectify.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
using System.Reflection.Metadata;

namespace Collectify.Web.Controllers;

public class CollectionController : Controller
{
    private readonly IUserService userService;
    private readonly IItemService itemService;
    private readonly IItemFieldService itemFieldService;
    private readonly ICollectionService collectionService;
    private readonly IAuthorizationService authorizationService;

    public CollectionController(IAuthorizationService authorizationService, ICollectionService collectionService, IUserService userService, IItemService itemService, IItemFieldService itemFieldService)
    {
        this.userService = userService;
        this.collectionService = collectionService;
        this.authorizationService = authorizationService;
        this.itemService = itemService;
        this.itemFieldService = itemFieldService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(long collectionsUserId, int page = 1)
    {
        var visitor = await this.authorizationService.AuthorizeAsync();

        var viewModel = new CollectionIndexViewModel
        {
            PageIndex = page,
            Visitor = (await this.userService.GetAsync(visitor.Id)).Result,
            CollectionsUser = (await this.userService.GetAsync(collectionsUserId)).Result,
            Collections = (await this.collectionService
                .GetAllAsync(
                    new PaginationParams { PageIndex = page, PageSize = 4},
                    c => c.UserId == collectionsUserId)).Result
        };

        return View("Index", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> View(long id)
    {
        var collection = (await this.collectionService.GetAsync(id)).Result;

        if (collection is null)
        {
            return View("Error");
        }

        collection.User = (await this.userService.GetAsync(collection.UserId)).Result;

        var items = (await this.itemService.GetAllAsync(new PaginationParams { PageSize = 10, PageIndex = 1 }, i => i.CollectionId == collection.Id)).Result;

        foreach (var item in items.Data)
            item.Fields = (await this.itemFieldService.GetAllAsync(new PaginationParams { PageSize = 10, PageIndex = 1 }, i => i.ItemId == item.Id)).Result.Data;

        var viewModel = new CollectionViewModel
        {
            Visitor = (await this.userService.GetAsync((await this.authorizationService.AuthorizeAsync()).Id)).Result,
            Collection = collection,
            Items = items
        };

        return View("View", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        if (ModelState.IsValid)
        {
            var collection = (await this.collectionService.GetAsync(id)).Result;

            var response = await this.collectionService.RemoveAsync(id);

            if (response.Result)
                TempData["SuccessMessage"] = response.Message;
            else
                TempData["ErrorMessage"] = response.Message;

            return await Index(collection.UserId);
        }

        return await MyCollections();
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("Create", new CollectionCreationDto
        {
            FieldsList = new List<CollectionField>()
        });
    }

    [HttpPost]
    public async Task<IActionResult> PostCreate(CollectionCreationDto dto)
    {
        if (ModelState.IsValid)
        {
            foreach (var field in dto.FieldsList)
                dto.Fields.Add(field.Name, field.Type);

            var response = await this.collectionService.AddAsync(dto);

            if (response.Result is null)
                TempData["ErrorMessage"] = response.Message;
            else
            {
                TempData["SuccessMessage"] = response.Message;
                return await Index(response.Result.UserId);
            }
        }

        return Create();
    }

    [HttpGet]
    public async Task<IActionResult> MyCollections()
    {
        var user = await this.authorizationService.AuthorizeAsync();

        return await Index(user.Id);
    }

    [HttpGet]
    public async Task<IActionResult> Update(CollectionUpdateDto dto)
    {
        var collection = (await this.collectionService.GetAsync(dto.Id)).Result;

        dto.Name = collection.Name;
        dto.Description = collection.Description;
        dto.Category = collection.Category;

        return View("Update", dto);
    }

    [HttpPost]
    public async Task<IActionResult> PostUpdate(CollectionUpdateDto dto)
    {
        if (ModelState.IsValid)
        {
            var response = await this.collectionService.ModifyAsync(dto);

            if (response.Result is null)
                TempData["ErrorMessage"] = response.Message;
            else
            {
                TempData["SuccessMessage"] = response.Message;
                return await Index(response.Result.UserId);
            }
        }

        return await Update(dto);
    }
}