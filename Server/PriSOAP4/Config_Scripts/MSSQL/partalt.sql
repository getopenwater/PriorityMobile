set ANSI_NULLS ON
set QUOTED_IDENTIFIER ON
GO

ALTER FUNCTION [dbo].[PARTALIAS]()
RETURNS 
@SVCCALL_PARTS TABLE 
(
	-- Add the column definitions for the TABLE variable here
	PART INT,
	BARCODE CHAR(32), 
	PARTNAME CHAR(32)
)
AS
BEGIN
	INSERT INTO @SVCCALL_PARTS
	SELECT PART, BARCODE, PARTNAME
	FROM PART
	where BARCODE <> '' 

	INSERT INTO @SVCCALL_PARTS
	SELECT PART, PARTNAME, PARTNAME
	FROM PART
	where BARCODE <> ''
	RETURN
END

SELECT BARCODE, PARTNAME FROM dbo.SVCCALL_PARTS() WHERE BARCODE = '6883670000021'"