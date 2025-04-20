using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MyWebApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyWebApp.Services
{
    public class ContactService
    {
        private readonly IMongoCollection<Contact> _contacts;

        public ContactService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _contacts = database.GetCollection<Contact>("Contacts");
        }

        public async Task<List<Contact>> GetAsync() =>
            await _contacts.Find(_ => true).ToListAsync();

        public async Task<Contact> GetAsync(string id) =>
            await _contacts.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Contact contact)
        {
            try
            {
                // Clear the Id to ensure MongoDB generates a new one
                contact.Id = null;

                await _contacts.InsertOneAsync(contact);
                Console.WriteLine($"Contact created successfully with ID: {contact.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ContactService.CreateAsync: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateAsync(string id, Contact contactIn) =>
            await _contacts.ReplaceOneAsync(x => x.Id == id, contactIn);

        public async Task RemoveAsync(string id) =>
            await _contacts.DeleteOneAsync(x => x.Id == id);
    }
}