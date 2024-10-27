using CrudApp.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

public class SalesOrderRepository
{
    private readonly DbConnectionFactory _dbConnectionFactory;

    public SalesOrderRepository(DbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    private static void ValidateSalesOrder(SalesOrder order)
    {
        if (string.IsNullOrWhiteSpace(order.Number_Order) ||
            order.Date == DateTime.MinValue ||
            string.IsNullOrWhiteSpace(order.Customer))
        {
            throw new ArgumentException("Please fill in all fields");
        }
    }

    private async Task InsertOrderItems(IDbConnection db, string orderId, List<ItemOrder> items)
    {
        foreach (var item in items)
        {
            item.Id_Item = Guid.NewGuid().ToString();
            item.Id_Order = orderId;

            string itemOrderQuery = @"
                INSERT INTO ItemOrder (id_item, id_order, item_name, qty, price)
                VALUES (@Id_Item, @Id_Order, @Item_Name, @Qty, @Price)";
            await db.ExecuteAsync(itemOrderQuery, item);
        }
    }

    public async Task<bool> CreateSalesOrder(SalesOrder newOrder)
    {
        ValidateSalesOrder(newOrder);

        using var db = _dbConnectionFactory.CreateConnection();
        newOrder.Id_Order = Guid.NewGuid().ToString();

        string salesOrderQuery = @"
            INSERT INTO SalesOrder (id_order, number_order, date, customer, address)
            VALUES (@Id_Order, @Number_Order, @Date, @Customer, @Address)";
        var orderResult = await db.ExecuteAsync(salesOrderQuery, newOrder);

        if (orderResult > 0)
        {
            await InsertOrderItems(db, newOrder.Id_Order, newOrder.Items);
            return true;
        }
        return false;
    }

    public async Task<int> GetSalesOrderCount()
    {
        using var db = _dbConnectionFactory.CreateConnection();
        string countQuery = "SELECT COUNT(*) FROM SalesOrder";
        return await db.ExecuteScalarAsync<int>(countQuery);
    }

    public async Task<IEnumerable<SalesOrder>> GetSalesOrders(int page, int limit)
    {
        using var db = _dbConnectionFactory.CreateConnection();

        string query = @"
            SELECT so.id_order, so.number_order, so.date, so.customer, so.address
            FROM SalesOrder so
            ORDER BY so.date DESC
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

        var parameters = new { Offset = (page - 1) * limit, Limit = limit };
        return (await db.QueryAsync<SalesOrder>(query, parameters)).ToList();
    }

    public async Task<SalesOrder?> GetSalesOrderById(string idOrder)
    {
        using var db = _dbConnectionFactory.CreateConnection();

        string query = @"
            SELECT so.id_order, so.number_order, so.date, so.customer, so.address,
                   io.id_item, io.item_name, io.qty, io.price, io.total
            FROM SalesOrder so
            LEFT JOIN ItemOrder io ON so.id_order = io.id_order
            WHERE so.id_order = @IdOrder";

        var salesOrderDictionary = new Dictionary<string, SalesOrder>();

        await db.QueryAsync<SalesOrder, ItemOrder, SalesOrder>(
            query,
            (order, item) =>
            {
                if (!salesOrderDictionary.TryGetValue(order.Id_Order, out var existingOrder))
                {
                    existingOrder = order;
                    existingOrder.Items = new List<ItemOrder>();
                    salesOrderDictionary[order.Id_Order] = existingOrder;
                }
                existingOrder.Items.Add(item);
                return existingOrder;
            },
            new { IdOrder = idOrder },
            splitOn: "id_item"
        );

        return salesOrderDictionary.Values.FirstOrDefault();
    }

    public async Task<bool> UpdateSalesOrder(string id, SalesOrder updatedOrder)
    {
        ValidateSalesOrder(updatedOrder);

        using var db = _dbConnectionFactory.CreateConnection();
        updatedOrder.Id_Order = id;

        string updateOrderQuery = @"
            UPDATE SalesOrder
            SET number_order = @Number_Order, date = @Date, customer = @Customer, address = @Address
            WHERE id_order = @Id_Order";
        var orderResult = await db.ExecuteAsync(updateOrderQuery, updatedOrder);

        if (orderResult > 0)
        {
            string deleteItemsQuery = "DELETE FROM ItemOrder WHERE id_order = @Id_Order";
            await db.ExecuteAsync(deleteItemsQuery, new { Id_Order = id });

            await InsertOrderItems(db, id, updatedOrder.Items);
            return true;
        }
        return false;
    }

    public async Task<bool> DeleteSalesOrder(string id)
    {
        using var db = _dbConnectionFactory.CreateConnection();

        string deleteItemsQuery = "DELETE FROM ItemOrder WHERE id_order = @Id_Order";
        await db.ExecuteAsync(deleteItemsQuery, new { Id_Order = id });

        string deleteOrderQuery = "DELETE FROM SalesOrder WHERE id_order = @Id_Order";
        return await db.ExecuteAsync(deleteOrderQuery, new { Id_Order = id }) > 0;
    }

    public async Task<int> GetSearchSalesOrderCount(string? keywords, DateTime? date)
    {
        using var db = _dbConnectionFactory.CreateConnection();

        string query = @"
            SELECT COUNT(1) 
            FROM SalesOrder 
            WHERE (@Keywords IS NULL OR (number_order LIKE '%' + @Keywords + '%' OR customer LIKE '%' + @Keywords + '%'))
            AND (@Date IS NULL OR CAST(date AS DATE) = CAST(@Date AS DATE))";

        var parameters = new { Keywords = keywords, Date = date };
        return await db.ExecuteScalarAsync<int>(query, parameters);
    }

    public async Task<IEnumerable<SalesOrder>> SearchSalesOrders(string? keywords, DateTime? date, int page, int limit)
    {
        using var db = _dbConnectionFactory.CreateConnection();

        string query = @"
            SELECT so.id_order, so.number_order, so.date, so.customer, so.address
            FROM SalesOrder so
            WHERE (@Keywords IS NULL OR so.number_order LIKE '%' + @Keywords + '%' OR so.customer LIKE '%' + @Keywords + '%')
            AND (@Date IS NULL OR CAST(so.date AS DATE) = CAST(@Date AS DATE))
            ORDER BY so.date DESC
            OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

        var parameters = new { Keywords = keywords, Date = date, Offset = (page - 1) * limit, Limit = limit };
        return (await db.QueryAsync<SalesOrder>(query, parameters)).ToList();
    }
}
