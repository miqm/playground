IF NOT EXISTS (SELECT 1 FROM [sys].[database_principals] WHERE [name] = '$(sqlDbUser)') 
 CREATE USER $(sqlDbUser);

EXEC sp_addrolemember 'db_owner', $(sqlDbUser)
