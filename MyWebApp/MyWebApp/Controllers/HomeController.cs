using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyWebApp.Models;
using MyWebApp.Services;

namespace MyWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ReviewService _reviewService;

        // Single constructor that takes both dependencies
        public HomeController(ILogger<HomeController> logger, ReviewService reviewService)
        {
            _logger = logger;
            _reviewService = reviewService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        // Remove the simple Review() method and keep only the async version
        public async Task<IActionResult> Review()
        {
            _logger.LogInformation("HomeController.Review method called");
            try
            {
                _logger.LogInformation("Fetching reviews from database in HomeController");
                var reviews = await _reviewService.GetReviews();
                _logger.LogInformation($"Retrieved {reviews?.Count ?? 0} reviews from database in HomeController");

                return View("Review", reviews ?? new List<Review>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews in HomeController");
                return View("Review", new List<Review>());
            }
        }

        public IActionResult OrderConfirmation()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}