using Haulory.Api.Extensions;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Contracts.Customers;
using Haulory.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;

    public CustomersController(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll([FromQuery] string? search = null)
    {
        var ownerUserId = User.GetOwnerUserId();
        var customers = await _customerRepository.SearchByOwnerAsync(ownerUserId, search);

        return Ok(customers.Select(ToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id)
    {
        var ownerUserId = User.GetOwnerUserId();
        var customer = await _customerRepository.GetByIdAsync(id);

        if (customer == null || customer.OwnerUserId != ownerUserId)
            return NotFound();

        return Ok(ToDto(customer));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CompanyName))
            return BadRequest("Company name is required.");

        if (string.IsNullOrWhiteSpace(request.AddressLine1))
            return BadRequest("Address is required.");

        if (string.IsNullOrWhiteSpace(request.City))
            return BadRequest("City is required.");

        if (string.IsNullOrWhiteSpace(request.Country))
            return BadRequest("Country is required.");

        var ownerUserId = User.GetOwnerUserId();

        var existing = await _customerRepository.FindMatchAsync(
            ownerUserId,
            request.CompanyName,
            request.Email,
            request.AddressLine1,
            request.City,
            request.Country);

        if (existing != null)
            return Ok(ToDto(existing));

        var customer = new Customer(
            Guid.NewGuid(),
            ownerUserId,
            request.CompanyName,
            request.ContactName,
            request.Email,
            request.AddressLine1,
            request.City,
            request.Country);

        await _customerRepository.AddAsync(customer);

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, ToDto(customer));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        var ownerUserId = User.GetOwnerUserId();

        var customer = await _customerRepository.GetByIdForUpdateAsync(id);
        if (customer == null || customer.OwnerUserId != ownerUserId)
            return NotFound();

        customer.UpdateDetails(
            request.CompanyName,
            request.ContactName,
            request.Email,
            request.AddressLine1,
            request.City,
            request.Country);

        await _customerRepository.UpdateAsync(customer);
        return Ok(ToDto(customer));
    }

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