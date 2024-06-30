CREATE TABLE [dbo].[user]
(
  [id] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
  [key] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
  [email] NVARCHAR(1024) NOT NULL,
  [tenant_id] INT NOT NULL,
  [created_at] DATETIME NOT NULL,
  [updated_at] DATETIME NOT NULL,
  [is_enabled] BIT NOT NULL DEFAULT 1,
  CONSTRAINT FK_user_tenant FOREIGN KEY (tenant_id) REFERENCES tenant(id)
)
