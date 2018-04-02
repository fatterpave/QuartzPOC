CREATE LOGIN quartz WITH PASSWORD = 'quartz123';
CREATE USER quartz FOR LOGIN quartz;
exec sp_addrolemember 'db_owner', 'quartz'