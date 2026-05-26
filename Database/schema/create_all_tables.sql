DROP TABLE IF EXISTS notifications CASCADE;
DROP TABLE IF EXISTS transactions CASCADE;
DROP TABLE IF EXISTS bookings CASCADE;
DROP TABLE IF EXISTS apartments CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- 1. Пользователи (Арендаторы + Админ)
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(256) NOT NULL,
    full_name VARCHAR(150) NOT NULL,
    phone VARCHAR(15) UNIQUE NOT NULL,
    role VARCHAR(10) NOT NULL CHECK (role IN ('User', 'Admin')),
    telegram_id BIGINT unique,
    is_blocked BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 2. Квартиры
CREATE TABLE apartments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    unit_number VARCHAR(20) UNIQUE NOT NULL,
    entrance INT NOT NULL,
    floor INT NOT NULL,
    capacity INT NOT NULL CHECK (capacity BETWEEN 1 AND 10),
    base_price_per_night DECIMAL(10,2) NOT NULL CHECK (base_price_per_night >= 0),
    description TEXT,
    status VARCHAR(20) NOT NULL CHECK (status IN ('Available', 'Maintenance', 'Occupied')),
    created_at TIMESTAMPTZ DEFAULT NOW()
);
--CREATE INDEX idx_apartments_filter ON apartments(floor, entrance, capacity, status);

-- 3. Бронирования (Заявки)
CREATE TABLE bookings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    apartment_id UUID NOT NULL REFERENCES apartments(id) ON DELETE RESTRICT,
    check_in_date TIMESTAMPTZ NOT NULL,
    check_out_date TIMESTAMPTZ NOT NULL CHECK (check_out_date > check_in_date),
    status VARCHAR(20) NOT NULL CHECK (status IN ('Pending', 'Approved', 'Rejected', 'Cancelled', 'Completed')),
    total_price DECIMAL(10,2) NOT NULL CHECK (total_price >= 0),
    deposit DECIMAL(10,2) NOT NULL DEFAULT 0 CHECK (deposit >= 0),
    cancellation_reason VARCHAR(500),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ
);

-- CREATE INDEX idx_bookings_search ON bookings(apartment_id, check_in_date, check_out_date);
-- CREATE INDEX idx_bookings_admin ON bookings(status, created_at DESC);

-- 4. Транзакции
CREATE TABLE transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    booking_id UUID NOT NULL REFERENCES bookings(id) ON DELETE CASCADE,
    amount DECIMAL(10,2) NOT NULL CHECK (amount > 0),
    type VARCHAR(20) NOT NULL CHECK (type IN ('Payment', 'Deposit', 'Refund', 'Penalty')),
    status VARCHAR(20) NOT NULL CHECK (status IN ('Pending', 'Completed', 'Failed')),
    processed_at TIMESTAMPTZ
);

-- 5. Журнал уведомлений
CREATE TABLE notifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    type VARCHAR(30) NOT NULL CHECK (type IN ('Confirmation', 'Reminder', 'Emergency', 'StatusUpdate')),
    channel VARCHAR(20) NOT NULL CHECK (channel IN ('WinForms', 'Telegram', 'Email')),
    subject VARCHAR(200),
    body TEXT,
    status VARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (status IN ('Pending', 'Sent', 'Failed')),
    created_at TIMESTAMPTZ DEFAULT NOW()
);