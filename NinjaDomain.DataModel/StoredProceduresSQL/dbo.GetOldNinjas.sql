CREATE PROCEDURE [dbo].[GetOldNinjas]
AS
	SELECT * FROM Ninjas WHERE DateOfBirth <= '1/1/1980'
