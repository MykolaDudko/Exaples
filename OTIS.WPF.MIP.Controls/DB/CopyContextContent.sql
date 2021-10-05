-- Author:		DUDKO
-- Create date: 25.7.2016
-- Description:	Kopirovanie context - content


USE [ST]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

--- slou×Ý k p°ekopÝrovßnÝ obsahu kontextu do novÚho kontextu
CREATE PROCEDURE [dbo].[spCore_CopyContextContent]
		@source_contxt_id [contxt_id],
		@dest_contxt_id [contxt_id] OUTPUT,
		@name_txt VARCHAR(80) = NULL,
		@cat_code [contxt_cat_code] = NULL,
		@security_clsfc_id [security_clsfc_id] = NULL,
		@obj_contxt_id [contxt_id] = NULL,
		@assoc_cat_code [contxt_assoc_cat_code] = 'ISPART'
AS
BEGIN
	SET NOCOUNT ON;

	EXEC spMaxID_CONTXT @dest_contxt_id OUTPUT

	if @name_txt IS NULL
		SELECT @name_txt = name_txt FROM CONTXT WHERE contxt_id = @source_contxt_id

	if @cat_code IS NULL
		SELECT @cat_code = cat_code FROM CONTXT WHERE contxt_id = @source_contxt_id

	if @security_clsfc_id IS NULL
		SELECT @security_clsfc_id = security_clsfc_id FROM CONTXT WHERE contxt_id = @source_contxt_id

	INSERT INTO CONTXT (contxt_id, name_txt, cat_code, security_clsfc_id)
VALUES (@dest_contxt_id, @name_txt, @cat_code, @security_clsfc_id)

    INSERT INTO CONTXT_ELMT (contxt_id, contxt_elmt_ix, rptd_id)
	SELECT @dest_contxt_id as contxt_id, contxt_elmt_ix, rptd_id
	FROM CONTXT_ELMT WHERE contxt_id = @source_contxt_id

    INSERT INTO CONTXT_ELMT_STAT(contxt_id, contxt_elmt_ix, contxt_elmt_stat_ix, cat_code, effctv_dttm, estblng_org_id)
	SELECT @dest_contxt_id as contxt_id, contxt_elmt_ix, contxt_elmt_stat_ix, cat_code, effctv_dttm, estblng_org_id
	FROM CONTXT_ELMT_STAT WHERE contxt_id = @source_contxt_id

	INSERT INTO CONTXT_OI_ASSOC(contxt_id, obj_item_id, cat_code)
	SELECT @dest_contxt_id as contxt_id, obj_item_id, cat_code
	FROM CONTXT_OI_ASSOC WHERE contxt_id = @source_contxt_id

	INSERT INTO CONTXT_OI_ASSOC_STAT(contxt_id, obj_item_id, contxt_oi_assoc_stat_ix, cat_code, effctv_dttm, estblng_org_id)
	SELECT @dest_contxt_id as contxt_id, obj_item_id, contxt_oi_assoc_stat_ix, cat_code, effctv_dttm, estblng_org_id
	FROM CONTXT_OI_ASSOC_STAT WHERE contxt_id = @source_contxt_id

	INSERT INTO CONTXT_RPTD_ASSOC(contxt_id, rptd_id, cat_code)
	SELECT @dest_contxt_id as contxt_id, rptd_id, cat_code
	FROM CONTXT_RPTD_ASSOC WHERE contxt_id = @source_contxt_id

	INSERT INTO ACT_CONTXT(contxt_id, act_id, act_contxt_ix, cat_code)
	SELECT @dest_contxt_id as contxt_id, act_id, act_contxt_ix, cat_code
	FROM ACT_CONTXT WHERE contxt_id = @source_contxt_id

	INSERT INTO ACT_CONTXT_STAT(contxt_id, act_id, act_contxt_ix, act_contxt_stat_ix, cat_code, effctv_dttm, estblng_org_id)
	SELECT @dest_contxt_id as contxt_id, act_id, act_contxt_ix, act_contxt_stat_ix, cat_code, effctv_dttm, estblng_org_id
	FROM ACT_CONTXT_STAT WHERE contxt_id = @source_contxt_id

	IF @obj_contxt_id IS NOT NULL
BEGIN
    INSERT INTO CONTXT_ASSOC (subj_contxt_id, obj_contxt_id, cat_code)
    VALUES (@dest_contxt_id, @obj_contxt_id, @assoc_cat_code)
    
    DECLARE @contxt_assoc_stat_ix [ix]
    DECLARE @effctv_dttm [dttm]
    DECLARE @estblng_org_id [numeric](20, 0)
    DECLARE @assoc_stat_cat_code [assoc_stat_cat_code]
    
    EXEC spCore_GetNextIX @contxt_assoc_stat_ix OUTPUT
    EXEC spCore_GetDttmUtc @effctv_dttm OUTPUT
    SET @assoc_stat_cat_code = 'START'
    SELECT @estblng_org_id = HODNOTA FROM Z_CONFIG_TAB WHERE KLIC = 'CreatorID'
    
    INSERT INTO CONTXT_ASSOC_STAT (subj_contxt_id, obj_contxt_id, contxt_assoc_stat_ix, cat_code, effctv_dttm, estblng_org_id)
    VALUES (@dest_contxt_id, @obj_contxt_id, @contxt_assoc_stat_ix, @assoc_stat_cat_code, @effctv_dttm, @estblng_org_id)

-- asociácia vlastnika databazy do vytvoreného kontextu
-- neni to nutne, ale uľahčuje to vykonanie niektorých mtrs testu (treba SLT3 DP 1412)
    EXEC spCore_Add_OI_CTX_ASSOC @obj_item_id = @estblng_org_id, @contxt_id = @dest_contxt_id /*, @source_context_id = @obj_contxt_id - je riziko zo v parent kontexte vlastnik nebude */
END

END
GO

