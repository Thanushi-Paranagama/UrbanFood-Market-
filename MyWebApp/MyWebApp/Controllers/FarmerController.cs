using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebApp.Models;



    [Authorize(Roles = "Farmer")]
    public class FarmerController : Controller
    {
        private readonly DatabaseService1 _databaseService;

        public FarmerController(DatabaseService1 databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Index()
        {
            List<ItemFarmer> farmerItems = _databaseService.GetAllItems();
            return View(farmerItems);
        }

        public IActionResult AddItem()
        {
            return View();
        }

        public IActionResult ViewIndex()
        {
        return View();
        }
    public IActionResult ViewLogs()
    {
        List<FarmerItemLog> logs = _databaseService.GetItemLogs();
        return View(logs);
    }

    [HttpPost]
        public IActionResult AddItem([FromForm] ItemFarmer newItem)
        {
            _databaseService.SaveItemToOracle(newItem);
            ViewBag.Message = "Item saved successfully!";
            return View();
        }

        public IActionResult UpdateItem(int id)
        {
            ItemFarmer itemFarmer = _databaseService.GetItemById(id);
            if (itemFarmer == null)
            {
                return NotFound();
            }
            return View(itemFarmer);
        }

        [HttpPost]
        public IActionResult Update(ItemFarmer updatedItem)
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
    }
