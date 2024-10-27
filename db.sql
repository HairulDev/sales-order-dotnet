CREATE TABLE SalesOrder (
    id_order INT PRIMARY KEY IDENTITY(1,1),
    number_order NVARCHAR(50) NOT NULL,
    date DATE NOT NULL,
    customer NVARCHAR(100),
    address NVARCHAR(200)
);

CREATE TABLE ItemOrder (
    id_item INT PRIMARY KEY IDENTITY(1,1),
    id_order INT NOT NULL,
    item_name NVARCHAR(100),
    qty INT NOT NULL,
    price DECIMAL(18, 2) NOT NULL,
    total AS (qty * price) PERSISTED,
    FOREIGN KEY (id_order) REFERENCES SalesOrder(id_order)
);
