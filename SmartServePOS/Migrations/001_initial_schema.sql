PRAGMA foreign_keys = ON;

-- =============================================================================
-- SCHEMA VERSION
-- =============================================================================
CREATE TABLE IF NOT EXISTS SchemaVersion (
    Version INTEGER NOT NULL
);

INSERT INTO SchemaVersion (Version)
SELECT 1
WHERE NOT EXISTS (SELECT 1 FROM SchemaVersion);

-- =============================================================================
-- CATEGORIES
-- =============================================================================
CREATE TABLE IF NOT EXISTS categories (
    LocalId INTEGER PRIMARY KEY AUTOINCREMENT,
    Id INTEGER UNIQUE,
    name TEXT UNIQUE NOT NULL,
    is_active INTEGER DEFAULT 1,
    display_order INTEGER DEFAULT 0,
    UpdatedOn TEXT
);

-- =============================================================================
-- BRANDS
-- =============================================================================
CREATE TABLE IF NOT EXISTS brands (
    LocalId INTEGER PRIMARY KEY AUTOINCREMENT,
    Id INTEGER UNIQUE,
    name TEXT UNIQUE NOT NULL,
    is_active INTEGER DEFAULT 1,
    UpdatedOn TEXT
);

-- =============================================================================
-- PRODUCTS
-- =============================================================================
CREATE TABLE IF NOT EXISTS products (
    LocalId INTEGER PRIMARY KEY AUTOINCREMENT,
    Id INTEGER UNIQUE,
    name TEXT NOT NULL,
    category_id INTEGER,
    is_active INTEGER DEFAULT 1,
    display_order INTEGER DEFAULT 0,
    food_type TEXT CHECK (food_type IN ('VEG','NON_VEG')),
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    UpdatedOn TEXT,
    FOREIGN KEY (category_id) REFERENCES categories(Id)
);

-- =============================================================================
-- PRODUCT VARIANTS
-- =============================================================================
CREATE TABLE IF NOT EXISTS product_variants (
    LocalId INTEGER PRIMARY KEY AUTOINCREMENT,
    Id INTEGER UNIQUE,
    product_id INTEGER NOT NULL,
    brand_id INTEGER,
    variant_name TEXT NOT NULL,
    price REAL NOT NULL CHECK (price >= 0),
    is_active INTEGER DEFAULT 1,
    display_order INTEGER DEFAULT 0,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    UpdatedOn TEXT,
    FOREIGN KEY (product_id) REFERENCES products(Id) ON DELETE CASCADE,
    FOREIGN KEY (brand_id) REFERENCES brands(Id),
    UNIQUE (product_id, brand_id, variant_name)
);

CREATE INDEX IF NOT EXISTS idx_variants_product
    ON product_variants(product_id);

-- =============================================================================
-- RESTAURANT TABLES
-- =============================================================================
CREATE TABLE IF NOT EXISTS restaurant_tables (
    LocalId INTEGER PRIMARY KEY AUTOINCREMENT,
    Id INTEGER UNIQUE,
    display_name TEXT NOT NULL,
    is_active INTEGER DEFAULT 1,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    UpdatedOn TEXT
);

-- =============================================================================
-- TABLE STATUS
-- =============================================================================
CREATE TABLE IF NOT EXISTS table_status (
    LocalId INTEGER PRIMARY KEY AUTOINCREMENT,
    Id INTEGER UNIQUE,
    status_code TEXT UNIQUE NOT NULL,
    status_name TEXT,
    color_hex TEXT
);

-- =============================================================================
-- ORDERS
-- =============================================================================
CREATE TABLE IF NOT EXISTS orders (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    order_number TEXT UNIQUE,
    order_type TEXT CHECK (order_type IN ('DINE_IN','DELIVERY','PICKUP')),
    table_id INTEGER,
    status_id INTEGER,
    total_amount REAL DEFAULT 0 CHECK (total_amount >= 0),
    discount_type TEXT CHECK (discount_type IN ('FLAT','PERCENT')),
    discount_value REAL DEFAULT 0 CHECK (discount_value >= 0),
    discount_reason TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    closed_at TEXT,
    IsSynced INTEGER DEFAULT 0,
    SyncedOn TEXT,
    SyncError TEXT,
    FOREIGN KEY (table_id) REFERENCES restaurant_tables(Id),
    FOREIGN KEY (status_id) REFERENCES table_status(Id)
);

-- =============================================================================
-- ORDER ITEMS
-- =============================================================================
CREATE TABLE IF NOT EXISTS order_items (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    order_id INTEGER,
    variant_id INTEGER,
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    price_snapshot REAL NOT NULL CHECK (price_snapshot >= 0),
    discount_amount REAL DEFAULT 0 CHECK (discount_amount >= 0),
    FOREIGN KEY (order_id) REFERENCES orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (variant_id) REFERENCES product_variants(Id)
);

-- =============================================================================
-- PAYMENTS
-- =============================================================================
CREATE TABLE IF NOT EXISTS payments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    order_id INTEGER,
    mode TEXT CHECK (mode IN ('CASH','UPI','CARD')),
    amount REAL CHECK (amount >= 0),
    status TEXT CHECK (status IN ('PAID','FAILED')),
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    IsSynced INTEGER DEFAULT 0,
    FOREIGN KEY (order_id) REFERENCES orders(Id) ON DELETE CASCADE
);
