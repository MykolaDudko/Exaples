using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Windows;

namespace OTR.WPF.DIP.Controls
{
    public class DBcache
    {
        private static DBcache singletonInstance = null;

        public static DBcache GetData_DBcache()
        {
            if (singletonInstance == null)
            {
                singletonInstance = new DBcache();
            }

            return singletonInstance;
        }


        private string connectionString_ST;


        public string ConnectionString_ST
        {
            get
            {
                if (string.IsNullOrWhiteSpace(connectionString_ST))
                    connectionString_ST = OTR.Environment.GlobalParams.STconnString;

                return connectionString_ST;
            }

            set { connectionString_ST = value; }
        }



        public DataSet_DBcache dataSet_DBcache;


        public DBcache()
        {
            this.dataSet_DBcache = new DataSet_DBcache();
        }


        #region Context -----------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Poslední hodnota sekvence změny v kontextech, které jsou zohledněny.
        /// </summary>
        public int last_context_update_seqnr = 0;

        /// <summary>
        /// Nastavení aktuální hodnoty sekvence změny v kontextech.
        /// </summary>
        public void SetLastContextUpdateSeqnr()
        {
            SqlConnection conn = new SqlConnection(this.ConnectionString_ST);
            conn.Open();
            SqlCommand cmd = new SqlCommand("", conn);
            cmd.CommandText =
                @"DECLARE @OBJ_LCH int = (SELECT MAX(OBJ_LCH) FROM (SELECT (SELECT TOP 1 update_seqnr FROM CONTXT_OI_ASSOC_STAT WHERE CONTXT_OI_ASSOC_STAT.contxt_id = CONTXT.contxt_id  ORDER BY update_seqnr DESC) AS OBJ_LCH FROM CONTXT) last_update_seqnr);
                DECLARE @OBJST_LCH int = (SELECT MAX(OBJST_LCH) FROM (SELECT (SELECT TOP 1 update_seqnr FROM CONTXT_ELMT_STAT WHERE CONTXT_ELMT_STAT.contxt_id = CONTXT.contxt_id  ORDER BY update_seqnr DESC) AS OBJST_LCH FROM CONTXT) last_update_seqnr)
                DECLARE @ACT_LCH int = (SELECT MAX(ACT_LCH) FROM (SELECT (SELECT TOP 1 update_seqnr FROM ACT_CONTXT_STAT WHERE ACT_CONTXT_STAT.contxt_id = CONTXT.contxt_id  ORDER BY update_seqnr DESC) AS ACT_LCH FROM CONTXT) last_update_seqnr)
                SELECT MAX(v) AS last_context_update_seqnr
                FROM (VALUES (@OBJ_LCH), (@OBJST_LCH), (@ACT_LCH)) AS value(v);";

            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.Read())
                {
                    int.TryParse(rdr["last_context_update_seqnr"].ToString(), out last_context_update_seqnr);
                }
            }
        }

        /// <summary>
        /// Načíta z ST dáta pre zobrazenie kontextov.
        /// </summary>
        /// <param name="refresh">určuje či obnoviť dáta kontextov </ param>
        public void ReadContextData(bool refresh)
        {
            // Nejde o refresh
            if (!refresh)
                SetLastContextUpdateSeqnr();

            if (this.dataSet_DBcache.Context.Count > 0 && !refresh) return;     //nejaké dáta sú a nechce refresh

            // Pokiaľ sa jedná o refresh, tak sa vytvorí druhá DataSet na porovnanie zmien
            DataSet_DBcache dataSet_for_merge = refresh ? new DataSet_DBcache() : dataSet_DBcache;

            try
            {
                
                OTR.Environment.MIP.GlobalDbObjects.GetOwnContextId(OTR.Environment.MIP.ContextType.UserFolders, OTR.Environment.MIP.NotFoundHandling.Create);  // hľadá v ST ContextType.UserFolders, pokiaľ nie je, vytvorí ho.
                //OTR.Environment.MIP.GlobalDbObjects.GetOwnContextId(OTR.Environment.MIP.ContextType.UserFoldersForeign, OTR.Environment.MIP.NotFoundHandling.Create);
                //OTR.Environment.MIP.GlobalDbObjects.GetOwnContextId(OTR.Environment.MIP.ContextType.ActualSituation, OTR.Environment.MIP.NotFoundHandling.Create);
                //OTR.Environment.MIP.GlobalDbObjects.GetOwnContextId(OTR.Environment.MIP.ContextType.Events, OTR.Environment.MIP.NotFoundHandling.Create);
                //OTR.Environment.MIP.GlobalDbObjects.GetOwnContextId(OTR.Environment.MIP.ContextType.Holdings, OTR.Environment.MIP.NotFoundHandling.Create);
                //OTR.Environment.MIP.GlobalDbObjects.GetOwnContextId(OTR.Environment.MIP.ContextType.Missions, OTR.Environment.MIP.NotFoundHandling.Create);
                //OTR.Environment.MIP.GlobalDbObjects.GetOwnContextId(OTR.Environment.MIP.ContextType.PlansAndOrders, OTR.Environment.MIP.NotFoundHandling.Create);
                //OTR.Environment.MIP.GlobalDbObjects.GetOwnContextId(OTR.Environment.MIP.ContextType.PSPWorkingFolder, OTR.Environment.MIP.NotFoundHandling.Create);
                //OTR.Environment.MIP.GlobalDbObjects.GetOwnContextId(OTR.Environment.MIP.ContextType.Structures, OTR.Environment.MIP.NotFoundHandling.Create);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Chyba čtení globálních kontextů:\n\n{0}\n\n- {1}", ex.Message, ex.StackTrace), string.Format("DBCache - {0}", ex.Source), MessageBoxButton.OK, MessageBoxImage.Information);
            }

            string cmdTxt_Context =
                @"SELECT
					CONTXT.contxt_id,
					CONTXT.cat_code,
					CONTXT.name_txt,
					CONTXT.security_clsfc_id,
					Seznam_OIG.type AS oig_cat_code,
					Seznam_OIG.is_own AS oig_is_own,
					CONTXT.creator_id,
					Seznam_OIG.resp_org_id,
					Seznam_OIG.resp AS resp_org_name, 
					fce_KODY_JAZYK.NAZEV_KODU AS oig_cat_code_NAZEV
				FROM
					CONTXT LEFT JOIN
					Seznam_OIG ON CONTXT.contxt_id = Seznam_OIG.oig_id LEFT OUTER JOIN
					fce_KODY_JAZYK(@TwoLetterLang) AS fce_KODY_JAZYK ON fce_KODY_JAZYK.KOD = Seznam_OIG.type AND fce_KODY_JAZYK.dom_id = 100004248";

            string cmdTxt_ContextAssoc =
                @"SELECT
					MostRecent_CONTXT_ASSOC.subj_contxt_id,
					MostRecent_CONTXT_ASSOC.obj_contxt_id,
					MostRecent_CONTXT_ASSOC.assoc_cat_code, 
					MostRecent_CONTXT_ASSOC.obj_contxt_name_txt,
					MostRecent_CONTXT_ASSOC.subj_contxt_name_txt,
					MostRecent_CONTXT_ASSOC.effctv_dttm, 
					MostRecent_CONTXT_ASSOC.obj_contxt_cat_code,
					MostRecent_CONTXT_ASSOC.subj_contxt_cat_code, 
					fce_KODY_JAZYK.NAZEV_KODU AS subj_contxt_cat_code_NAZEV
				FROM MostRecent_CONTXT_ASSOC INNER JOIN
					fce_KODY_JAZYK(@TwoLetterLang) AS fce_KODY_JAZYK ON MostRecent_CONTXT_ASSOC.subj_contxt_cat_code = fce_KODY_JAZYK.KOD AND fce_KODY_JAZYK.dom_id = 100004244";

            string cmdTxt_Z_TRASH =
                @"SELECT DISTINCT id as contxt_id
				FROM Z_TRASH
				WHERE cat_code = 'CONTXT' AND (user_name = '*' OR user_name = '" + System.Environment.UserName + "')";

            SqlConnection sqlConnection = new SqlConnection(this.ConnectionString_ST);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("", sqlConnection);

            string tableName = dataSet_for_merge.Z_TRASH.TableName;
            try
            {
                // fill Z_TRASH table
                dataSet_for_merge.Z_TRASH.Clear();           //kontexty možno vyhodiť do koša a vrátiť z koša späť
                sqlDataAdapter.SelectCommand.CommandText = cmdTxt_Z_TRASH;
                sqlDataAdapter.Fill(dataSet_for_merge.Z_TRASH);

                string twoLetterLang = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
                sqlDataAdapter.SelectCommand.Parameters.Add("@TwoLetterLang", SqlDbType.Char, 2).Value = twoLetterLang;

                // fill Context table
                tableName = dataSet_for_merge.Context.TableName;
                sqlDataAdapter.SelectCommand.CommandText = cmdTxt_Context;
                sqlDataAdapter.Fill(dataSet_for_merge.Context);

                // fill ContextAssoc table
                tableName = dataSet_for_merge.ContextAssoc.TableName;
                sqlDataAdapter.SelectCommand.CommandText = cmdTxt_ContextAssoc;
                sqlDataAdapter.Fill(dataSet_for_merge.ContextAssoc);

                if (refresh)
                {
                    Environment.Utils.DataSetEx.MergeWithDelete(this.dataSet_DBcache, dataSet_for_merge, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Chyba čtení dat do DBCache.{0}:\n\n{1}\n\n- {2}", tableName, ex.Message, ex.StackTrace), string.Format("DBCache - {0}", ex.Source), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Vráti kontexty, ktoré vyhovujú podmienke danej parametrom filter.
        /// </summary>
        /// <param name = "filter"> filter pre vyhľadanie kontextov </ param>
        /// <returns> záznamy s hľadanými kontextami (DataRow []) </ returns>
        public DataRow[] GetContexts(string filter)
        {
            return this.dataSet_DBcache.Context.Select(filter);
        }

        /// <summary>
        /// Vráti podriadené kontexty.
        /// </ summary>
        /// <param name = "parentId"> Id nadriadeného kontexte </ param>
        /// <returns> záznamy s podriadenými kontextami (DataRow []) </ returns>
        public DataRow[] GetSubContexts(decimal parentId)
        {
            return this.dataSet_DBcache.ContextAssoc.Select(string.Format("obj_contxt_id = '{0}'", parentId.ToString()));
        }

        /// <summary>
        /// Vráti kontexty, ktoré sú v koši (Trash).
        /// </ summary>
        /// <returns> záznamy s kontextami v koši (DataRow []) </ returns>
        public DataRow[] GetContextsInTrash()
        {
            return this.dataSet_DBcache.Z_TRASH.Select();
        }

        #endregion Context --------------------------------------------------------------------------------------------------------------------------

        #region Structure ---------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Ak pre daný kontext nie sú načítané organizácie, načíta ich alebo obnoví v závislosti na parametri refresh.
        /// </ summary>
        /// <param name = "contxtId"> Id daného kontextu </ param>
        /// <param name = "structAssoc"> určuje aké väzby budú v štruktúre zobrazené (false = všetky väzby, true = len štrukturálne) </ param>
        /// <param name = "refresh"> určuje či obnoviť dáta pre daný kontext </ param>
        /// <returns> true - dáta úspešne načítaná / false - data neprevzatých </ returns>
        public bool ReadOrganisationsInContext(decimal contxtId, bool structAssoc, bool refresh)
        {
            if (refresh)
            {
                this.dataSet_DBcache.ORG_FREE_CONTXT.DeleteByContext(contxtId);
                this.dataSet_DBcache.ORG_STRUCT_DET_CONTXT.DeleteByContext(contxtId);
            }
            else
            {
                int pocObj = this.dataSet_DBcache.ORG_FREE_CONTXT.GetCountInContext(contxtId);
                int pocOrg = this.dataSet_DBcache.ORG_STRUCT_DET_CONTXT.GetCountInContext(contxtId);

                if (pocObj + pocOrg > 0) return true; 
            }

            string cmdTxtORG =
                @"SELECT CtxMostRecent_IncludedOI.contxt_id,
	                     CtxMostRecent_IncludedOI.obj_item_id,
	                     CtxMostRecent_IncludedOI.cat_code, 
                         CtxMostRecent_IncludedOI.name_txt,
	                     CtxMostRecent_OIHSTS.code AS hstly_code,
                         dbo.fceEos_isMilitaryPost(CtxMostRecent_IncludedOI.contxt_id, CtxMostRecent_IncludedOI.obj_item_id) AS is_mil_post
                  FROM CtxMostRecent_IncludedOI INNER JOIN
                       CtxMostRecent_OIHSTS ON CtxMostRecent_IncludedOI.contxt_id = CtxMostRecent_OIHSTS.contxt_id AND CtxMostRecent_IncludedOI.obj_item_id = CtxMostRecent_OIHSTS.obj_item_id
                  WHERE CtxMostRecent_IncludedOI.cat_code = 'OR' and CtxMostRecent_IncludedOI.contxt_id = @contxt_id";

            string cmdTxtSTRUCT =
               @"SELECT @contxt_id as contxt_id,
                         OrgStrCntxt.org_id,
                         OrgStrCntxt.org_ix,
                         OrgStrCntxt.subj_id,
                         OrgStrCntxt.obj_id,
                         OrgStrCntxt.obj_name,
                         OrgStrCntxt.obj_sname,
                         OrgStrCntxt.obj_cat_code,
                         OrgStrCntxt.hstly_code,
                         OrgStrCntxt.cat_code,
                         OrgStrCntxt.subcat_code,
                         OrgStrCntxt.assoc_type,
                         OrgStrCntxt.lvl,
                         OrgStrCntxt.org_name,
                         fce_KJ_1.NAZEV_KODU AS cat_code_NAZEV, 
                         fce_KJ_2.NAZEV_KODU AS subcat_code_NAZEV
                  FROM fce_SelectOrgStructuresFromContext(@contxt_id, @StructAssoc) AS OrgStrCntxt LEFT OUTER JOIN
                       fce_KODY_JAZYK(@TwoLetterLang) AS fce_KJ_1 ON OrgStrCntxt.cat_code = fce_KJ_1.KOD AND (fce_KJ_1.dom_id = 100004142 OR OrgStrCntxt.cat_code IS NULL) LEFT OUTER JOIN
                       fce_KODY_JAZYK(@TwoLetterLang) AS fce_KJ_2 ON OrgStrCntxt.subcat_code = fce_KJ_2.KOD AND (fce_KJ_2.dom_id = 100000297 OR OrgStrCntxt.subcat_code IS NULL)";

            string tabName = this.dataSet_DBcache.ORG_FREE_CONTXT.TableName;
            try
            {
                SqlConnection sqlConnection = new SqlConnection(this.ConnectionString_ST);

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmdTxtORG, sqlConnection);
                sqlDataAdapter.SelectCommand.Parameters.Add("@contxt_id", SqlDbType.Decimal).Value = contxtId;
                sqlDataAdapter.Fill(this.dataSet_DBcache.ORG_FREE_CONTXT);

                string twoLetterLang = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;

                tabName = this.dataSet_DBcache.ORG_STRUCT_DET_CONTXT.TableName;
                sqlDataAdapter.SelectCommand.CommandText = cmdTxtSTRUCT;
                sqlDataAdapter.SelectCommand.Parameters.Add("@StructAssoc", SqlDbType.Bit, 1).Value = ((structAssoc) ? 1 : 0);  
                sqlDataAdapter.SelectCommand.Parameters.Add("@TwoLetterLang", SqlDbType.Char, 2).Value = twoLetterLang;
                sqlDataAdapter.Fill(this.dataSet_DBcache.ORG_STRUCT_DET_CONTXT);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Chyba čtení dat do DBCache.{0}:\n{1}\n\n- {2}", tabName, ex.Message, ex.StackTrace), string.Format("DBCache - {0}", ex.Source), MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            DataSet_DBcache.ORG_FREE_CONTXTRow rowToDelete;
            foreach (DataSet_DBcache.ORG_STRUCT_DET_CONTXTRow row in this.dataSet_DBcache.ORG_STRUCT_DET_CONTXT.Rows)
            {
                rowToDelete = this.dataSet_DBcache.ORG_FREE_CONTXT.FindBycontxt_idobj_item_id(row.contxt_id, row.obj_id);
                if (rowToDelete != null) rowToDelete.Delete();

                if (row.Issubj_idNull()) continue;
                rowToDelete = this.dataSet_DBcache.ORG_FREE_CONTXT.FindBycontxt_idobj_item_id(row.contxt_id, row.subj_id);
                if (rowToDelete != null) rowToDelete.Delete();
            }
            this.dataSet_DBcache.ORG_FREE_CONTXT.AcceptChanges();

            return true;
        }

        /// <summary>
        /// Vráti záznamy organizačnej štruktúry v danom kontexte.
        /// </ summary>
        /// <param name = "contxtId"> Id daného kontextu </ param>
        /// <returns> záznamy organizačnej štruktúry (DataRow []) </ returns>
        public DataRow[] GetOrgStructDetInContext(decimal contxtId)
        {
            return this.dataSet_DBcache.ORG_STRUCT_DET_CONTXT.Select(string.Format("contxt_id = '{0}'", contxtId.ToString()));
        }

        /// <summary>
        /// Vráti záznamy s voľnými organizáciami v danom kontexte.
        /// </ summary>
        /// <param name = "contxtId"> Id daného kontextu </ param>
        /// <returns> záznamy voľných organizácií (DataRow []) </ returns>
        public DataRow[] GetFreeOrgInContext(decimal contxtId)
        {
            return this.dataSet_DBcache.ORG_FREE_CONTXT.Select(string.Format("contxt_id = '{0}'", contxtId.ToString()), "name_txt");
        }

        #endregion Structure ------------------------------------------------------------------------------------------------------------------------

        #region Object - objekty v kontextu ---------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Ak pre daný kontext nie sú načítané objekty (ACT a OBJ_ITEM), načíta ich alebo obnoví v závislosti na parametri refresh.
        /// </ summary>
        /// <param name = "contxtId"> Id daného kontextu </ param>
        /// <param name = "refresh"> určuje či obnoviť dáta pre daný kontext </ param>
        /// <returns> true - dáta úspešne načítaná / false - data neprevzatých </ returns>
        public bool ReadObjectsInContext(decimal contxtId, bool refresh)
        {
            DataSet_DBcache dataSet_for_merge = refresh ? new DataSet_DBcache() : dataSet_DBcache;

            if (refresh)
            {
                //this.dataSet_DBcache.CONTXT_ACT.DeleteByContext(contxtId);
                //this.dataSet_DBcache.CONTXT_OBJ_ITEM_FREE.DeleteByContext(contxtId);
                //this.dataSet_DBcache.CONTXT_ORG_STRUCT_DET.DeleteByContext(contxtId);
            }
            else
            {
              int pocAct = this.dataSet_DBcache.CONTXT_ACT.GetCountInContext(contxtId);
              int pocObj = this.dataSet_DBcache.CONTXT_OBJ_ITEM_FREE.GetCountInContext(contxtId);
              int pocOrg = this.dataSet_DBcache.CONTXT_ORG_STRUCT_DET.GetCountInContext(contxtId);

              if (pocAct + pocObj + pocOrg > 0) return true; 
            }

            string cmdTxtOBJ =
                @"SELECT CtxMostRecent_IncludedOI.contxt_id,
	                     CtxMostRecent_IncludedOI.obj_item_id,
	                     CtxMostRecent_IncludedOI.cat_code, 
                         CtxMostRecent_IncludedOI.name_txt,
	                     CtxMostRecent_OIHSTS.code AS hstly_code,
	                     CtxMostRecent_OITYPE.name_txt AS popis, 
                         CtxMostRecent_OITYPE.obj_type_id,
                         dbo.fceEos_isMilitaryPost(CtxMostRecent_IncludedOI.contxt_id, CtxMostRecent_IncludedOI.obj_item_id) AS is_mil_post
                  FROM CtxMostRecent_IncludedOI INNER JOIN
                       CtxMostRecent_OIHSTS ON CtxMostRecent_IncludedOI.contxt_id = CtxMostRecent_OIHSTS.contxt_id AND 
                       CtxMostRecent_IncludedOI.obj_item_id = CtxMostRecent_OIHSTS.obj_item_id INNER JOIN
                       CtxMostRecent_OITYPE ON CtxMostRecent_IncludedOI.contxt_id = CtxMostRecent_OITYPE.contxt_id AND 
                       CtxMostRecent_IncludedOI.obj_item_id = CtxMostRecent_OITYPE.obj_item_id
                  WHERE CtxMostRecent_IncludedOI.contxt_id = @contxt_id";

            string cmdTxtACT =
               @"SELECT CtxMostRecent_IncludedACT.contxt_id,
                        ACT.act_id,
                        ACT.cat_code,
                        ACT.name_txt,
                        dbo.fce_ActHostility(ACT.act_id, CtxMostRecent_IncludedACT.contxt_id) AS hstly_code,
                        ISNULL(ACT_EVENT.NAZEV_KODU, ACT_TASK.NAZEV_KODU) AS popis
                 FROM   ACT INNER JOIN
                        CtxMostRecent_IncludedACT ON ACT.act_id = CtxMostRecent_IncludedACT.act_id LEFT OUTER JOIN
                        (SELECT ACT_TASK.act_task_id,
                                fce_KODY_JAZYK.NAZEV_KODU
                         FROM ACT_TASK INNER JOIN
                              fce_KODY_JAZYK(@TwoLetterLang) AS fce_KODY_JAZYK ON ACT_TASK.actv_code = fce_KODY_JAZYK.KOD
                         WHERE fce_KODY_JAZYK.dom_id = 100000280) AS ACT_TASK ON ACT.act_id = ACT_TASK.act_task_id LEFT OUTER JOIN
                        (SELECT ACT_EVENT.act_event_id,
                                fce_KODY_JAZYK.NAZEV_KODU
                         FROM ACT_EVENT INNER JOIN
                              fce_KODY_JAZYK(@TwoLetterLang) AS fce_KODY_JAZYK ON ACT_EVENT.cat_code = fce_KODY_JAZYK.KOD
                         WHERE fce_KODY_JAZYK.dom_id = 100000282) AS ACT_EVENT ON ACT.act_id = ACT_EVENT.act_event_id
                 WHERE CtxMostRecent_IncludedACT.contxt_id = @contxt_id";

            string cmdTxtSTRUCT =
                @"SELECT @contxt_id as contxt_id,
                         OrgStrCntxt.org_id,
                         OrgStrCntxt.org_ix,
                         OrgStrCntxt.subj_id,
                         OrgStrCntxt.obj_id,
                         OrgStrCntxt.obj_name,
                         OrgStrCntxt.obj_sname,
                         OrgStrCntxt.obj_cat_code,
                         OrgStrCntxt.hstly_code,
                         OrgStrCntxt.cat_code,
                         OrgStrCntxt.subcat_code,
                         OrgStrCntxt.assoc_type,
                         OrgStrCntxt.lvl,
                         OrgStrCntxt.org_name,
                         fce_KJ_1.NAZEV_KODU AS cat_code_NAZEV, 
                         fce_KJ_2.NAZEV_KODU AS subcat_code_NAZEV
                  FROM fce_SelectOrgStructuresFromContext(@contxt_id, @StructAssoc) AS OrgStrCntxt LEFT OUTER JOIN
                       fce_KODY_JAZYK(@TwoLetterLang) AS fce_KJ_1 ON OrgStrCntxt.cat_code = fce_KJ_1.KOD AND (fce_KJ_1.dom_id = 100004142 OR OrgStrCntxt.cat_code IS NULL) LEFT OUTER JOIN
                       fce_KODY_JAZYK(@TwoLetterLang) AS fce_KJ_2 ON OrgStrCntxt.subcat_code = fce_KJ_2.KOD AND (fce_KJ_2.dom_id = 100000297 OR OrgStrCntxt.subcat_code IS NULL)";

            string tabName = dataSet_for_merge.CONTXT_OBJ_ITEM_FREE.TableName;
            try
            {
                SqlConnection sqlConnection = new SqlConnection(this.ConnectionString_ST);

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmdTxtOBJ, sqlConnection);
                sqlDataAdapter.SelectCommand.Parameters.Add("@contxt_id", SqlDbType.Decimal).Value = contxtId;
                sqlDataAdapter.Fill(dataSet_for_merge.CONTXT_OBJ_ITEM_FREE);

                string twoLetterLang = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;

                tabName = dataSet_for_merge.CONTXT_ACT.TableName;
                sqlDataAdapter.SelectCommand.CommandText = cmdTxtACT;
                sqlDataAdapter.SelectCommand.Parameters.Add("@TwoLetterLang", SqlDbType.Char, 2).Value = twoLetterLang;
                sqlDataAdapter.Fill(dataSet_for_merge.CONTXT_ACT);

                tabName = dataSet_for_merge.CONTXT_ORG_STRUCT_DET.TableName;
                sqlDataAdapter.SelectCommand.CommandText = cmdTxtSTRUCT;
                sqlDataAdapter.SelectCommand.Parameters.Add("@StructAssoc", SqlDbType.Bit, 1).Value = 1;                
                sqlDataAdapter.Fill(dataSet_for_merge.CONTXT_ORG_STRUCT_DET);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Chyba čtení dat do DBCache.{0}:\n{1}\n\n- {2}", tabName, ex.Message, ex.StackTrace), string.Format("DBCache - {0}", ex.Source), MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            DataSet_DBcache.CONTXT_OBJ_ITEM_FREERow rowToDelete;
            foreach (DataSet_DBcache.CONTXT_ORG_STRUCT_DETRow row in dataSet_for_merge.CONTXT_ORG_STRUCT_DET.Rows)
            {
                rowToDelete = dataSet_for_merge.CONTXT_OBJ_ITEM_FREE.FindBycontxt_idobj_item_id(row.contxt_id, row.obj_id);
                if (rowToDelete != null) rowToDelete.Delete();

                if (row.Issubj_idNull()) continue;
                rowToDelete = dataSet_for_merge.CONTXT_OBJ_ITEM_FREE.FindBycontxt_idobj_item_id(row.contxt_id, row.subj_id);
                if (rowToDelete != null) rowToDelete.Delete();
            }

            dataSet_for_merge.CONTXT_OBJ_ITEM_FREE.AcceptChanges();

            if (refresh)
            {
                // merge the temporary dataset with the current one
                Environment.Utils.DataTableEx.MergeWithDelete(this.dataSet_DBcache.CONTXT_ACT, dataSet_for_merge.CONTXT_ACT, true);
                Environment.Utils.DataTableEx.MergeWithDelete(this.dataSet_DBcache.CONTXT_OBJ_ITEM_FREE, dataSet_for_merge.CONTXT_OBJ_ITEM_FREE, true);
                Environment.Utils.DataTableEx.MergeWithDelete(this.dataSet_DBcache.CONTXT_ORG_STRUCT_DET, dataSet_for_merge.CONTXT_ORG_STRUCT_DET, true);
            }

            return true;
        }

        /// <summary>
        /// Vráti počet záznamov v zadanom kontexte, ktoré majú daný kód kategórie.
        /// </ summary>
        /// <param name = "contxtId"> Id zadaného kontexte </ param>
        /// <param name = "cat_code"> kód kategória </ param>
        /// <param name = "structDet"> urcuje či hľadať počet záznamov v štruktúre (CONTXT_ORG_STRUCT_DET) </ param>
        /// <returns> počet záznamov </ returns>
        public int GetCountInContext(decimal contxtId, string cat_code, bool structDet = false)
        {
            if (cat_code == "ACTEV" || cat_code == "ACTTA")
                return this.dataSet_DBcache.CONTXT_ACT.GetCountInContext(contxtId, cat_code);
            else if (structDet)
                return this.dataSet_DBcache.CONTXT_ORG_STRUCT_DET.GetCountInContext(contxtId, cat_code);
            else
                return this.dataSet_DBcache.CONTXT_OBJ_ITEM_FREE.GetCountInContext(contxtId, cat_code);
        }

        /// <summary>
        /// Vráti záznamy s "Actions" v danom kontexte.
        /// </ summary>
        /// <param name = "contxtId"> Id daného kontextu </ param>
        /// <param name = "cat_code"> kód kategória danej "Action" </ param>
        /// <param name = "sort"> reťazec špecifikujúci stĺpce a smer zoradenia výslednej množiny záznamov </ param>
        /// <returns> záznamy hľadaných "Actions" (DataRow []) </ returns>
        public DataRow[] GetActionsInContext(decimal contxtId, string cat_code, string sort)
        {
            string filtr = string.Format("contxt_id = '{0}' and cat_code = '{1}'", contxtId.ToString(), cat_code);

            return this.dataSet_DBcache.CONTXT_ACT.Select(filtr, sort);
        }

        /// <summary>
        /// Vráti záznamy s voľnými (nezaradenými) objekty v danom kontexte.
        /// </ summary>
        /// <param name = "contxtId"> Id daného kontextu </ param>
        /// <param name = "cat_code"> kód kategória danej "Action" </ param>
        /// <param name = "sort"> reťazec špecifikujúci stĺpce a smer zoradenia výslednej množiny záznamov </ param>
        /// <returns> záznamy hľadaných voľných objektov (DataRow []) </ returns>
        public DataRow[] GetFreeObjectsInContext(decimal contxtId, string cat_code, string sort)
        {
            string filtr = string.Format("contxt_id = '{0}' and cat_code = '{1}'", contxtId.ToString(), cat_code);

            return this.dataSet_DBcache.CONTXT_OBJ_ITEM_FREE.Select(filtr, sort);
        }

        /// <summary>
        /// Vrá záznamy organizačnej Štruktúry v danú KONTEX.
        /// </ summary>
        /// <param name = "contxtId"> Id daného kontextu </ param>
        /// <returns> záznamy organizačnej štruktúre (DataRow []) </ returns>
        public DataRow[] GetOrgStructInContext(decimal contxtId)
        {
            return this.dataSet_DBcache.CONTXT_ORG_STRUCT_DET.Select(string.Format("contxt_id = '{0}'", contxtId.ToString()));
        }

        #endregion Object - objekty v kontextu ------------------------------------------------------------------------------------------------------
    }
}