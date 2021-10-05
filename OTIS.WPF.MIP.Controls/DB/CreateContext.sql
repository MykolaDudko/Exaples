-- Author:		DUDKO
-- Create date: 23.7.2016
-- Description: Vytvorí nový kontext (defaultne s cate_code "NOS").
--Ak dostane naplnené parameter @contxt_id, pouzije ho, ak dostane NULL
--vytvorí nové ID a vrati ho (je to OUTPUT parameter).
--Ak dostane naplnené parameter @obj_contxt_id, priraïte novovytvorený kontext
--do tohto kontextu (defaultne s cat_code asociácie "Isparta")

USE [ST]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[spCore_CreateContext]
(
    @contxt_id [contxt_id] = NULL OUTPUT,
    @name_txt VARCHAR(80) = 'Context',
    @cat_code [contxt_cat_code] = 'NOS',
    @security_clsfc_id [security_clsfc_id] = NULL,
    @obj_contxt_id [contxt_id] = NULL,
    @assoc_cat_code [contxt_assoc_cat_code] = 'ISPART'
)
AS
SET NOCOUNT ON

IF @contxt_id IS NULL
    EXEC spMaxID_CONTXT @contxt_id OUTPUT

INSERT INTO CONTXT (contxt_id, name_txt, cat_code, security_clsfc_id)
VALUES (@contxt_id, @name_txt, @cat_code, @security_clsfc_id)

IF @obj_contxt_id IS NOT NULL
BEGIN
    INSERT INTO CONTXT_ASSOC (subj_contxt_id, obj_contxt_id, cat_code)
    VALUES (@contxt_id, @obj_contxt_id, @assoc_cat_code)
    
    DECLARE @contxt_assoc_stat_ix [ix]
    DECLARE @effctv_dttm [dttm]
    DECLARE @estblng_org_id [numeric](20, 0)
    DECLARE @assoc_stat_cat_code [assoc_stat_cat_code]
    
    EXEC spCore_GetNextIX @contxt_assoc_stat_ix OUTPUT
    EXEC spCore_GetDttmUtc @effctv_dttm OUTPUT
    SET @assoc_stat_cat_code = 'START'
    SELECT @estblng_org_id = HODNOTA FROM Z_CONFIG_TAB WHERE KLIC = 'CreatorID'
    
    INSERT INTO CONTXT_ASSOC_STAT (subj_contxt_id, obj_contxt_id, contxt_assoc_stat_ix, cat_code, effctv_dttm, estblng_org_id)
    VALUES (@contxt_id, @obj_contxt_id, @contxt_assoc_stat_ix, @assoc_stat_cat_code, @effctv_dttm, @estblng_org_id)

-- asociácia vlastnika databazy do vytvoreného kontextu
-- neni to nutne, ale u¾ahèuje to vykonanie niektorých mtrs testu (treba SLT3 DP 1412)
    EXEC spCore_Add_OI_CTX_ASSOC @obj_item_id = @estblng_org_id, @contxt_id = @contxt_id 
END

RETURN

GO

