using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class CustomerController : Controller
{
    private readonly CustomerService _customerService;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(CustomerService customerService, ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    // GET method to display the form - uses the view from Home folder
    [HttpGet]
    public IActionResult Index()
    {
        return View("~/Views/Home/Guest.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> Submit(Customer customer)
    {
        _logger.LogInformation("[DEBUG] Submit method called with Name: {Name}, ContactNumber: {ContactNumber}",
            customer.Name, customer.ContactNumber);

        if (ModelState.IsValid)
        {
            bool success = await _customerService.SaveCustomerAsync(customer);
            if (success)
            {
                _logger.LogInformation("[DEBUG] Data successfully saved in DB.");

                // Fetch the saved customer details
                var savedCustomer = await _customerService.GetCustomerByNameAndNumberAsync(customer.Name, customer.ContactNumber);

                if (savedCustomer != null)
                {
                    // Redirect to PlaceOrder with customer details
                    return RedirectToAction("PlaceOrder", new
                    {
                        id = savedCustomer.ID,
                        name = savedCustomer.Name,
                        contact = savedCustomer.ContactNumber
                    });
                }
            }
            else
            {
                _logger.LogError("[ERROR] Data saving failed.");
                ModelState.AddModelError("", "Error saving customer data.");
            }
        }
        else
        {
            _logger.LogError("[ERROR] ModelState is invalid.");
        }
        return View("~/Views/Home/Guest.cshtml");
    }

    // GET method for the PlaceOrder page - uses the view from Home folder
    [HttpGet]
    public IActionResult PlaceOrder(int id, string name, string contact)
    {
        var customer = new Customer
        {
            ID = id,
            Name = name,
            ContactNumber = contact
        };
        return View("~/Views/Home/PlaceOrder.cshtml", customer);
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomerDetails(string name, string contactNumber)
    {
        var customer = await _customerService.GetCustomerByNameAndNumberAsync(name, contactNumber);
        if (customer != null)
        {
            return Json(new { success = true, customerID = customer.ID, contactNumber = customer.ContactNumber });
        }
        return Json(new { success = false });
    }
}
