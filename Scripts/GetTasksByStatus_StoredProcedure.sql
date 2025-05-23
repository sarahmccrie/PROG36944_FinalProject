USE [FinalProject_ERMS]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetTasksByStatus]
    @Status NVARCHAR(50)
AS
BEGIN
    SELECT 
        t.TaskId, 
        t.Name, 
        t.Description, 
        t.Status, 
        t.Priority, 
        p.Name AS ProjectName, 
        e.Name AS AssignedEmployee
    FROM dbo.TaskItems t
    LEFT JOIN dbo.Projects p ON t.ProjectId = p.ProjectId
    LEFT JOIN dbo.Employees e ON t.AssignedEmployeeId = e.EmployeeId
    WHERE t.Status = @Status
END
GO
