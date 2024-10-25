using CrudApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class SalesOrderController : ControllerBase
{
    private readonly SalesOrderService _salesOrderService;

    public SalesOrderController(SalesOrderService salesOrderService)
    {
        _salesOrderService = salesOrderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSalesOrder([FromBody] SalesOrder salesOrder)
    {
        try
        {
            var result = await _salesOrderService.CreateSalesOrder(salesOrder);

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
    public async Task<IActionResult> GetSalesOrders()
    {
        try
        {
            var salesOrders = await _salesOrderService.GetSalesOrders();
            return Ok(new
            {
                message = "Sales orders retrieved successfully",
                status = true,
                data = salesOrders
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
            var result = await _salesOrderService.UpdateSalesOrder(id, updatedOrder);

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
            var result = await _salesOrderService.DeleteSalesOrder(id);

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
    [FromQuery] DateTime? date)
    {
        try
        {
            var salesOrders = await _salesOrderService.SearchSalesOrders(keywords, date);

            return Ok(new
            {
                message = "Sales orders retrieved successfully",
                status = true,
                data = salesOrders
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
