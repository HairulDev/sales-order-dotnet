CREATE TABLE SalesOrder (
    id_order NVARCHAR(50) PRIMARY KEY,
    number_order NVARCHAR(50) NOT NULL,
    date DATE NOT NULL,
    customer NVARCHAR(100),
    address NVARCHAR(200)
);

CREATE TABLE ItemOrder (
    id_item NVARCHAR(50) NOT NULL,
    id_order NVARCHAR(50) NOT NULL,
    item_name NVARCHAR(100),
    qty INT NOT NULL,
    price DECIMAL(18, 2) NOT NULL,
    total AS (qty * price) PERSISTED,
    FOREIGN KEY (id_order) REFERENCES SalesOrder(id_order)
);
