-- Author:		DUDKO
-- Create date: 27.7.2016
-- Description: Procedúra kopíruje objekt z jedného kontextu do druhého. Spolu s objektom prenáša základné
-- vlastnosti objektu v závislosti od nastavenia blokovania.


USE [ST]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO



CREATE PROCEDURE [dbo].[spCore_CopyObjFromContextToContext]
	@SCID numeric(20,0), -- identifikátor zdrojového kontext
	@DCID numeric(20,0), -- identifikátor cieľového kontext
	@OID  numeric(20,0), -- identifikátor objektu
	@BLoc     boolean = 0,                  -- Lokácia
	@BCmt     boolean = 0,                  -- Poznámka
	@BAddr    boolean = 0,                  -- Adresy
	@BCapab   boolean = 0,                  -- Schopnosti (Capability)
	@BAffil   boolean = 0,                  -- Priradenie (Affiliation)
	@BHold    boolean = 0,                  -- Počty
AS
BEGIN
	SET NOCOUNT ON

		EXEC [dbo].[spCore_CopyObjFromContextToContext_Base] @SCID, @DCID, @OID, @BAss
		IF (@BLoc = 1) EXEC [dbo].[spCore_CopyObjFromContextToContext_Loc] @SCID, @DCID, @OID
		IF (@BCmt = 1) EXEC [dbo].[spCore_CopyObjFromContextToContext_Cmt] @SCID, @DCID, @OID
		IF (@BAddr = 1) EXEC [dbo].[spCore_CopyObjFromContextToContext_Address] @SCID, @DCID, @OID
		IF (@BCapab = 1) EXEC [dbo].[spCore_CopyObjFromContextToContext_Capab] @SCID, @DCID, @OID
		IF (@BAffil = 1) EXEC [dbo].[spCore_CopyObjFromContextToContext_Affil] @SCID, @DCID, @OID

END
GO

