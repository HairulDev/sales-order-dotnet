using CrudApp.Models;
using Dapper;
using System;
using System.Data;
using System.Threading.Tasks;

public class SalesOrderService
{
    private readonly DbConnectionFactory _dbConnectionFactory;

    public SalesOrderService(DbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> CreateSalesOrder(SalesOrder newOrder)
    {
        using (var db = _dbConnectionFactory.CreateConnection())
        {
            // Generate unique ID for SalesOrder
            newOrder.Id_Order = Guid.NewGuid().ToString();

            string salesOrderQuery = @"
                INSERT INTO SalesOrder (id_order, number_order, date, customer, address)
                VALUES (@Id_Order, @Number_Order, @Date, @Customer, @Address)";

            // Insert SalesOrder
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



    public async Task<IEnumerable<SalesOrder>> GetSalesOrders()
    {
        using (var db = _dbConnectionFactory.CreateConnection())
        {
            // SQL query to retrieve all sales orders with their items
            string query = @"
                SELECT so.id_order, so.number_order, so.date, so.customer, so.address,
                       io.id_item, io.item_name, io.qty, io.price, io.total
                FROM SalesOrder so
                LEFT JOIN ItemOrder io ON so.id_order = io.id_order
                ORDER BY so.date DESC;
            ";

            // Execute query and process results
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

                    // If there is an item associated, add it to the order's item list
                    if (item != null)
                    {
                        existingOrder.Items.Add(item);
                    }

                    return existingOrder;
                },
                splitOn: "id_item"
            );

            return salesOrderDictionary.Values;
        }
    }

    public async Task<bool> UpdateSalesOrder(string id, SalesOrder updatedOrder)
    {
        using (var db = _dbConnectionFactory.CreateConnection())
        {
            // Update SalesOrder data
            string updateOrderQuery = @"
            UPDATE SalesOrder
            SET number_order = @Number_Order, date = @Date, customer = @Customer, address = @Address
            WHERE id_order = @Id_Order";

            updatedOrder.Id_Order = id; // Ensure the order ID is set
            var orderResult = await db.ExecuteAsync(updateOrderQuery, updatedOrder);

            if (orderResult > 0)
            {
                // Delete existing items associated with the order to refresh them
                string deleteItemsQuery = "DELETE FROM ItemOrder WHERE id_order = @Id_Order";
                await db.ExecuteAsync(deleteItemsQuery, new { Id_Order = id });

                // Insert updated items
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
            // Delete items associated with the sales order
            string deleteItemsQuery = "DELETE FROM ItemOrder WHERE id_order = @Id_Order";
            await db.ExecuteAsync(deleteItemsQuery, new { Id_Order = id });

            // Delete the sales order
            string deleteOrderQuery = "DELETE FROM SalesOrder WHERE id_order = @Id_Order";
            var orderResult = await db.ExecuteAsync(deleteOrderQuery, new { Id_Order = id });

            return orderResult > 0;
        }
    }

    public async Task<IEnumerable<SalesOrder>> SearchSalesOrders(string? keywords, DateTime? date)
    {
        using (var db = _dbConnectionFactory.CreateConnection())
        {
            var query = @"
            SELECT so.id_order, so.number_order, so.date, so.customer, so.address,
                   io.id_item, io.item_name, io.qty, io.price, io.total
            FROM SalesOrder so
            LEFT JOIN ItemOrder io ON so.id_order = io.id_order
            WHERE 1 = 1"; // Base query

            // Dynamic filtering
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(keywords))
            {
                query += " AND (so.number_order = @Keywords OR so.customer = @Keywords)";
                parameters.Add("Keywords", keywords);
            }

            if (date.HasValue)
            {
                query += " AND CAST(so.date AS DATE) = @Date";
                parameters.Add("Date", date.Value.Date);
            }

            // Execute the query and process results
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

            return salesOrderDictionary.Values;
        }
    }




}
