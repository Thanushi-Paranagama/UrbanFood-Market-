using Microsoft.AspNetCore.Mvc;
using MyWebApp.Models;
using MyWebApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebApp.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ReviewService _reviewService;

        public ReviewController(ReviewService reviewService)
        {
            Console.WriteLine("ReviewController constructor called");
            _reviewService = reviewService ?? throw new ArgumentNullException(nameof(reviewService));
        }

        public async Task<IActionResult> Index()
        {
            Console.WriteLine("ReviewController.Index method called");
            try
            {
                Console.WriteLine("Attempting to retrieve reviews from database");
                var reviews = await _reviewService.GetReviews();
                Console.WriteLine($"Retrieved {reviews?.Count ?? 0} reviews from database");

                if (reviews != null && reviews.Any())
                {
                    foreach (var review in reviews)
                    {
                        Console.WriteLine($"Review found: {review.Id}, {review.CustomerName}, Rating: {review.Rating}, {review.ReviewText}");
                    }
                }
                else
                {
                    Console.WriteLine("No reviews found in database");
                }

                return View("~/Views/Home/Review.cshtml", reviews ?? new List<Review>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving reviews: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return View("~/Views/Home/Review.cshtml", new List<Review>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddReview(string customerName, string reviewText, int rating)
        {
            Console.WriteLine($"AddReview called with: {customerName}, Rating: {rating}, Review: {reviewText}");

            if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(reviewText))
            {
                return RedirectToAction("Index");
            }

            try
            {
                var review = new Review
                {
                    CustomerName = customerName,
                    ReviewText = reviewText,
                    Rating = rating,
                    Date = DateTime.UtcNow
                };

                await _reviewService.AddReview(review);
                Console.WriteLine($"Added new review from {customerName} with rating {rating}");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding review: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
    }
}