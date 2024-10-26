using CrudApp.Models;
using Dapper;
using System;
using System.Data;
using System.Threading.Tasks;

public class SalesOrderRepository
{
    private readonly DbConnectionFactory _dbConnectionFactory;

    public SalesOrderRepository(DbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> CreateSalesOrder(SalesOrder newOrder)
    {
        using (var db = _dbConnectionFactory.CreateConnection())
        {
            newOrder.Id_Order = Guid.NewGuid().ToString();

            string salesOrderQuery = @"
                INSERT INTO SalesOrder (id_order, number_order, date, customer, address)
                VALUES (@Id_Order, @Number_Order, @Date, @Customer, @Address)";

            var orderResult = await db.ExecuteAsync(salesOrderQuery, newOrder);

            if (orderResult > 0)
            {
                foreach (var item in newOrder.Items)
                {
                    item.Id_Item = Guid.NewGuid().ToString();
                    item.Id_Order = newOrder.Id_Order;

                    string itemOrderQuery = @"
                        INSERT INTO ItemOrder (id_item, id_order, item_name, qty, price)
                        VALUES (@Id_Item, @Id_Order, @Item_Name, @Qty, @Price)";

                    await db.ExecuteAsync(itemOrderQuery, item);
                }

                return true;
            }

            return false;
        }
    }


    public async Task<IEnumerable<SalesOrder>> GetSalesOrders(int page, int limit)
    {
        using (var db = _dbConnectionFactory.CreateConnection())
        {
            string query = @"
        SELECT so.id_order, so.number_order, so.date, so.customer, so.address
        FROM SalesOrder so
        ORDER BY so.date DESC
        OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";

            var parameters = new DynamicParameters();
            parameters.Add("Offset", (page - 1) * limit);
            parameters.Add("Limit", limit);

            var salesOrders = await db.QueryAsync<SalesOrder>(
                query,
                parameters
            );

            return salesOrders.ToList();
        }
    }


    public async Task<SalesOrder?> GetSalesOrderById(string idOrder)
    {
        using (var db = _dbConnectionFactory.CreateConnection())
        {
            string query = @"
            SELECT so.id_order, so.number_order, so.date, so.customer, so.address,
                   io.id_item, io.item_name, io.qty, io.price, io.total
            FROM SalesOrder so
            LEFT JOIN ItemOrder io ON so.id_order = io.id_order
            WHERE so.id_order = @IdOrder";

            var parameters = new DynamicParameters();
            parameters.Add("IdOrder", idOrder);

            var salesOrderDictionary = new Dictionary<string, SalesOrder>();

            var salesOrders = await db.QueryAsync<SalesOrder, ItemOrder, SalesOrder>(
                query,
                (order, item) =>
                {
                    if (!salesOrderDictionary.TryGetValue(order.Id_Order, out var existingOrder))
                    {
                        existingOrder = order;
                        existingOrder.Items = new List<ItemOrder>();
                        salesOrderDictionary[order.Id_Order] = existingOrder;
                    }

                    if (item != null)
                    {
                        existingOrder.Items.Add(item);
                    }

                    return existingOrder;
                },
                parameters,
                splitOn: "id_item"
            );

            // Return the SalesOrder if found, otherwise null
            return salesOrderDictionary.Values.FirstOrDefault();
        }
    }



    public async Task<bool> UpdateSalesOrder(string id, SalesOrder updatedOrder)
    {
        using (var db = _dbConnectionFactory.CreateConnection())
        {
            string updateOrderQuery = @"
            UPDATE SalesOrder
            SET number_order = @Number_Order, date = @Date, customer = @Customer, address = @Address
            WHERE id_order = @Id_Order";

            updatedOrder.Id_Order = id;
            var orderResult = await db.ExecuteAsync(updateOrderQuery, updatedOrder);

            if (orderResult > 0)
            {
                string deleteItemsQuery = "DELETE FROM ItemOrder WHERE id_order = @Id_Order";
                await db.ExecuteAsync(deleteItemsQuery, new { Id_Order = id });

                foreach (var item in updatedOrder.Items)
                {
                    item.Id_Item = Guid.NewGuid().ToString();
                    item.Id_Order = id;

                    string insertItemQuery = @"
                    INSERT INTO ItemOrder (id_item, id_order, item_name, qty, price)
                    VALUES (@Id_Item, @Id_Order, @Item_Name, @Qty, @Price)";

                    await db.ExecuteAsync(insertItemQuery, item);
                }

                return true;
            }

            return false;
        }
    }


    public async Task<bool> DeleteSalesOrder(string id)
    {
        using (var db = _dbConnectionFactory.CreateConnection())
        {
            string deleteItemsQuery = "DELETE FROM ItemOrder WHERE id_order = @Id_Order";
            await db.ExecuteAsync(deleteItemsQuery, new { Id_Order = id });

            string deleteOrderQuery = "DELETE FROM SalesOrder WHERE id_order = @Id_Order";
            var orderResult = await db.ExecuteAsync(deleteOrderQuery, new { Id_Order = id });

            return orderResult > 0;
        }
    }

    public async Task<IEnumerable<SalesOrder>> SearchSalesOrders(string? keywords, DateTime? date, int page, int pageSize)
    {
        if (string.IsNullOrEmpty(keywords) || !date.HasValue)
        {
            return Enumerable.Empty<SalesOrder>();
        }

        using (var db = _dbConnectionFactory.CreateConnection())
        {
            var query = @"
        SELECT so.id_order, so.number_order, so.date, so.customer, so.address
        FROM SalesOrder so
        WHERE (so.number_order = @Keywords OR so.customer = @Keywords)
        AND CAST(so.date AS DATE) = @Date
        ORDER BY so.date DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new DynamicParameters();
            parameters.Add("Keywords", keywords);
            parameters.Add("Date", date.Value.Date);
            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);

            var salesOrders = await db.QueryAsync<SalesOrder>(
                query,
                parameters
            );

            return salesOrders.ToList();
        }
    }





}
