using Microsoft.AspNetCore.Mvc;
using MyWebApp.Models;
using MyWebApp.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebApp.Controllers
{
    public class ContactController : Controller
    {
        private readonly ContactService _contactService;

        public ContactController(ContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitForm(Contact contact)
        {
            try
            {
                // Remove any possible ModelState errors for Id field
                ModelState.Remove("Id");

                if (ModelState.IsValid)
                {
                    // Add debugging
                    Console.WriteLine($"Attempting to save contact: {contact.FirstName} {contact.LastName}");

                    await _contactService.CreateAsync(contact);

                    Console.WriteLine("Contact saved successfully");
                    return RedirectToAction("ThankYou");
                }
                else
                {
                    Console.WriteLine("Model state is invalid");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine($"- {error.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error saving contact: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", "An error occurred while saving your contact information.");
            }

            return View("Index", contact);
        }

        [HttpGet]
        public IActionResult ThankYou()
        {
            return View();
        }
    }
}