using Haulory.Api.Extensions;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Contracts.Customers;
using Haulory.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

// Marks this class as an API controller with automatic model binding and validation behavior.
[ApiController]

// Sets the base route for all endpoints in this controller.
[Route("api/customers")]

// Requires an authenticated user for all customer endpoints.
[Authorize]
public sealed class CustomersController : ControllerBase
{
    // Repository used to read and write customer data.
    private readonly ICustomerRepository _customerRepository;

    // Inject the customer repository.
    public CustomersController(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    // Returns all customers for the current owner, with optional search filtering.
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll([FromQuery] string? search = null)
    {
        // Read the current authenticated owner's ID from the user claims.
        var ownerUserId = User.GetOwnerUserId();

        // Search customers that belong only to the current owner.
        var customers = await _customerRepository.SearchByOwnerAsync(ownerUserId, search);

        // Map domain entities to DTOs before returning them to the client.
        return Ok(customers.Select(ToDto).ToList());
    }

    // Returns a single customer by ID if it belongs to the current owner.
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id)
    {
        // Read the current authenticated owner's ID from the user claims.
        var ownerUserId = User.GetOwnerUserId();

        // Load the customer by its unique identifier.
        var customer = await _customerRepository.GetByIdAsync(id);

        // Return 404 if the customer does not exist or does not belong to this owner.
        if (customer == null || customer.OwnerUserId != ownerUserId)
            return NotFound();

        // Return the customer mapped to a DTO.
        return Ok(ToDto(customer));
    }

    // Creates a new customer record for the current owner.
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerRequest request)
    {
        // Validate required fields before attempting to create a customer.
        if (string.IsNullOrWhiteSpace(request.CompanyName))
            return BadRequest("Company name is required.");

        if (string.IsNullOrWhiteSpace(request.AddressLine1))
            return BadRequest("Address is required.");

        if (string.IsNullOrWhiteSpace(request.City))
            return BadRequest("City is required.");

        if (string.IsNullOrWhiteSpace(request.Country))
            return BadRequest("Country is required.");

        // Read the current authenticated owner's ID from the user claims.
        var ownerUserId = User.GetOwnerUserId();

        // Check whether a matching customer already exists for this owner.
        var existing = await _customerRepository.FindMatchAsync(
            ownerUserId,
            request.CompanyName,
            request.Email,
            request.AddressLine1,
            request.City,
            request.Country);

        // If a matching customer already exists, return the existing record instead of creating a duplicate.
        if (existing != null)
            return Ok(ToDto(existing));

        // Create a new customer entity.
        var customer = new Customer(
            Guid.NewGuid(),
            ownerUserId,
            request.CompanyName,
            request.ContactName,
            request.Email,
            request.AddressLine1,
            request.City,
            request.Country);

        // Persist the new customer.
        await _customerRepository.AddAsync(customer);

        // Return 201 Created with a route to fetch the new resource.
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, ToDto(customer));
    }

    // Updates an existing customer record if it belongs to the current owner.
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        // Read the current authenticated owner's ID from the user claims.
        var ownerUserId = User.GetOwnerUserId();

        // Load the customer for update.
        var customer = await _customerRepository.GetByIdForUpdateAsync(id);

        // Return 404 if the customer does not exist or does not belong to this owner.
        if (customer == null || customer.OwnerUserId != ownerUserId)
            return NotFound();

        // Update the editable customer details.
        customer.UpdateDetails(
            request.CompanyName,
            request.ContactName,
            request.Email,
            request.AddressLine1,
            request.City,
            request.Country);

        // Save the updated customer.
        await _customerRepository.UpdateAsync(customer);

        // Return the updated customer mapped to a DTO.
        return Ok(ToDto(customer));
    }

    // Maps a Customer domain entity to a CustomerDto for API responses.
    private static CustomerDto ToDto(Customer customer)
        => new()
        {
            Id = customer.Id,
            OwnerUserId = customer.OwnerUserId,
            CompanyName = customer.CompanyName,
            ContactName = customer.ContactName,
            Email = customer.Email,
            AddressLine1 = customer.AddressLine1,
            City = customer.City,
            Country = customer.Country
        };
}