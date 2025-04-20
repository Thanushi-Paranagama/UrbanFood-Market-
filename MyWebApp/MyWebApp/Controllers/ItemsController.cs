using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using MyWebApp.Models;

namespace MyWebApp.Controllers
{
    public class ItemsController : Controller
    {
        private readonly string connectionString;
        private readonly DatabaseService _databaseService;

        public ItemsController(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        public IActionResult Index()
        {
            List<Item> items = _databaseService.GetAllItems();
            return View(items);
        }

        public IActionResult AddItem()
        {
            return View();
        }

        [HttpPost]
        [HttpPost]
        public IActionResult AddItem([FromForm] Item newItem)
        {
            _databaseService.SaveItemToOracle(newItem);
            ViewBag.Message = "Item saved successfully!";
            return View();
        }

        public IActionResult UpdateItem(int id)
        {
            Item item = _databaseService.GetItemById(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        [HttpPost]
        public IActionResult Update(Item updatedItem)
        {
            if (updatedItem == null) return BadRequest("Invalid item data.");
            try
            {
                _databaseService.UpdateItemInOracle(updatedItem);
                TempData["Message"] = "Item updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error updating item: " + ex.Message;
                return View("UpdateItem", updatedItem);
            }
        }

        public IActionResult DeleteItem(int id)
        {
            Item item = _databaseService.GetItemById(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        [HttpPost, ActionName("DeleteItem")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteItemConfirmed(int id)
        {
            try
            {
                _databaseService.DeleteItem(id);
                TempData["Message"] = "Item deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting item: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
