using CrudApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class SalesOrderController : ControllerBase
{
    private readonly SalesOrderRepository _salesOrderRepository;

    public SalesOrderController(SalesOrderRepository salesOrderRepository)
    {
        _salesOrderRepository = salesOrderRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSalesOrder([FromBody] SalesOrder salesOrder)
    {
        try
        {
            var result = await _salesOrderRepository.CreateSalesOrder(salesOrder);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Sales order creation failed",
                    status = false
                });
            }

            return Ok(new
            {
                message = "Sales order created successfully",
                status = true
            });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new
            {
                message = ex.Message,
                status = false
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetSalesOrders([FromQuery] int page = 1, [FromQuery] int limit = 5)
    {
        try
        {
            // Fetch sales orders data
            var salesOrders = await _salesOrderRepository.GetSalesOrders(page, limit);

            // Get total count of sales orders
            var totalCount = await _salesOrderRepository.GetSalesOrderCount();

            // Calculate total pages
            var totalPage = (int)Math.Ceiling((double)totalCount / limit);

            return Ok(new
            {
                message = "Sales orders retrieved successfully",
                status = true,
                data = salesOrders,
                currentPage = page,
                totalPage = totalPage,
                limit = limit,
                count= totalCount
            });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new
            {
                message = ex.Message,
                status = false
            });
        }
    }


    [HttpGet("{idOrder}")]
    public async Task<IActionResult> GetSalesOrderById(string idOrder)
    {
        try
        {
            var salesOrder = await _salesOrderRepository.GetSalesOrderById(idOrder);

            if (salesOrder == null)
            {
                return NotFound(new
                {
                    message = "Sales order not found",
                    status = false
                });
            }

            return Ok(new
            {
                message = "Sales order retrieved successfully",
                status = true,
                data = salesOrder
            });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new
            {
                message = ex.Message,
                status = false
            });
        }
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSalesOrder(string id, [FromBody] SalesOrder updatedOrder)
    {
        try
        {
            var result = await _salesOrderRepository.UpdateSalesOrder(id, updatedOrder);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Sales order update failed",
                    status = false
                });
            }

            return Ok(new
            {
                message = "Sales order updated successfully",
                status = true
            });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new
            {
                message = ex.Message,
                status = false
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSalesOrder(string id)
    {
        try
        {
            var result = await _salesOrderRepository.DeleteSalesOrder(id);

            if (!result)
            {
                return NotFound(new
                {
                    message = "Sales order not found or delete failed",
                    status = false
                });
            }

            return Ok(new
            {
                message = "Sales order deleted successfully",
                status = true
            });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new
            {
                message = ex.Message,
                status = false
            });
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchSalesOrders(
    [FromQuery] string? keywords,
    [FromQuery] DateTime? date,
    [FromQuery] int page = 1,
    [FromQuery] int limit = 5)
    {
        try
        {
            // Fetch filtered sales orders based on search criteria
            var salesOrders = await _salesOrderRepository.SearchSalesOrders(keywords, date, page, limit);

            // Get total count of filtered sales orders based on the search criteria
            var totalCount = await _salesOrderRepository.GetSearchSalesOrderCount(keywords, date);

            // Calculate total pages
            var totalPage = (int)Math.Ceiling((double)totalCount / limit);

            return Ok(new
            {
                message = "Sales orders retrieved successfully",
                status = true,
                data = salesOrders,
                currentPage = page,
                totalPage = totalPage,
                limit = limit,
                count = totalCount
            });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new
            {
                message = ex.Message,
                status = false
            });
        }
    }


}
