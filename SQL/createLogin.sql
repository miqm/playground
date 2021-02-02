IF NOT EXISTS (SELECT 1 FROM [sys].[sql_logins] WHERE [name] = '$(sqlDbUser)') 
 CREATE USER $(sqlDbUser) WITH PASSWORD='$(sqlDbPassword)';
ELSE
  ALTER LOGIN $(sqlDbUser) WITH PASSWORD='$(sqlDbPassword)';
