-- Initialize databases for GraphOfOrders system
-- This script creates the necessary databases for development

-- Main orders database (if not exists)
SELECT 'CREATE DATABASE ordersdb'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'ordersdb')\gexec

-- Delivery service database (if not exists)
SELECT 'CREATE DATABASE delivery'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'delivery')\gexec

-- Optional: Create development user with appropriate permissions
-- (Comment out if you prefer to use the default postgres user)
-- DO
-- $do$
-- BEGIN
--    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'graphoforders_dev') THEN
--       CREATE ROLE graphoforders_dev LOGIN PASSWORD 'dev_password';
--       GRANT ALL PRIVILEGES ON DATABASE ordersdb TO graphoforders_dev;
--       GRANT ALL PRIVILEGES ON DATABASE delivery TO graphoforders_dev;
--    END IF;
-- END
-- $do$;

-- Log successful initialization
\echo 'Database initialization completed:'
\echo '- ordersdb: Main orders application database'
\echo '- delivery: Delivery service database'
\echo 'Both databases are ready for development use'
