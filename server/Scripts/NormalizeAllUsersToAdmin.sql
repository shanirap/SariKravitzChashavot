-- Product policy: every account is Admin. Run after backup on existing databases.
-- Idempotent: safe to run multiple times.

UPDATE [Users]
SET [Role] = N'Admin'
WHERE [Role] <> N'Admin';
