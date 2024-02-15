CREATE TABLE departments
(
    id SERIAL PRIMARY KEY,
    title VARCHAR(120) NOT NULL,
    parent_id INTEGER DEFAULT NULL
);
CREATE TABLE employees
(
    id SERIAL PRIMARY KEY,
    name VARCHAR(120) NOT NULL,
    surname VARCHAR(120) NOT NULL,
    post VARCHAR(120) NOT NULL,
    department_id INTEGER NOT NULL,
    phone BIGINT DEFAULT 0
);