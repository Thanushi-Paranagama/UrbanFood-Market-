using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MyWebApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyWebApp.Services
{
    public class ReviewService
    {
        private readonly IMongoCollection<Review> _reviews;
        private readonly string _connectionString;
        private readonly string _databaseName;

        public ReviewService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            _connectionString = mongoDBSettings.Value.ConnectionString;
            _databaseName = mongoDBSettings.Value.DatabaseName;

            Console.WriteLine($"ReviewService initializing with connection: {_connectionString}, DB: {_databaseName}");

            try
            {
                var client = new MongoClient(_connectionString);
                var database = client.GetDatabase(_databaseName);
                _reviews = database.GetCollection<Review>("Reviews");
                Console.WriteLine("MongoDB connection established successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to MongoDB: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Review>> GetReviews()
        {
            try
            {
                Console.WriteLine("ReviewService.GetReviews method called");
                // First check if we can connect to the collection
                var count = await _reviews.CountDocumentsAsync(FilterDefinition<Review>.Empty);
                Console.WriteLine($"Total document count in collection: {count}");

                // Then get all reviews
                var reviews = await _reviews.Find(_ => true).ToListAsync();
                Console.WriteLine($"Found {reviews.Count} reviews in database");

                // Debug output all reviews
                foreach (var review in reviews)
                {
                    Console.WriteLine($"Review in DB: {review.Id}, {review.CustomerName}, {review.ReviewText}, {review.Date}");
                }

                return reviews;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReviewService.GetReviews: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task AddReview(Review review)
        {
            try
            {
                Console.WriteLine($"Adding review: {review.CustomerName}, {review.ReviewText}");
                await _reviews.InsertOneAsync(review);
                Console.WriteLine("Review added successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding review: {ex.Message}");
                throw;
            }
        }
    }
}