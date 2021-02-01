IF NOT EXISTS (SELECT 1 FROM [sys].[database_principals] WHERE [name] = '$(sqlDbUser)') 
 CREATE USER $(sqlDbUser) WITH PASSWORD='$(sqlDbPassword)';
ELSE
  ALTER LOGIN $(sqlDbUser) WITH PASSWORD='$(sqlDbPassword)';
