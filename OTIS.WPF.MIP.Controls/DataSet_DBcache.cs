using System.Data;

namespace OTIS.WPF.MIP.Controls
{
    public partial class DataSet_DBcache
    {
        public partial class ORG_FREE_CONTXTDataTable
        {
            /// <summary>
            /// Vrátí počet záznamů v zadaném kontextu, které mají daný kód kategorie.
            /// </summary>
            /// <param name="contxtId">Id zadaného kontextu</param>
            /// <param name="cat_code">kód kategorie</param>
            /// <returns>počet záznamů</returns>
            public int GetCountInContext(decimal contxtId, string cat_code = null)
            {
                string filtr;
                if (cat_code == null)
                    filtr = string.Format("contxt_id = '{0}'", contxtId.ToString());
                else
                    filtr = string.Format("contxt_id = '{0}' and cat_code = '{1}'", contxtId.ToString(), cat_code);

                return (int)this.Compute("count(contxt_id)", filtr);
            }

            /// <summary>
            /// Smaže všechny záznamy pro zadaný kontext.
            /// </summary>
            /// <param name="contxtId">Id zadaného kontextu</param>
            public void DeleteByContext(decimal contxtId)
            {
                DataRow[] rows = this.Select(string.Format("contxt_id = '{0}'", contxtId.ToString()));
                foreach (DataRow r in rows)
                    r.Delete();
                this.AcceptChanges();
            }
        }

        public partial class ORG_STRUCT_DET_CONTXTDataTable
        {
            /// <summary>
            /// Vrátí počet záznamů v zadaném kontextu, které mají daný kód kategorie.
            /// </summary>
            /// <param name="contxtId">Id zadaného kontextu</param>
            /// <param name="cat_code">kód kategorie</param>
            /// <returns>počet záznamů</returns>
            public int GetCountInContext(decimal contxtId, string cat_code = null)
            {
                string filtr;
                if (cat_code == null)
                    filtr = string.Format("contxt_id = '{0}'", contxtId.ToString());
                else
                    filtr = string.Format("contxt_id = '{0}' and OBJ_cat_code = '{1}'", contxtId.ToString(), cat_code);

                return (int)this.Compute("count(contxt_id)", filtr);
            }

            /// <summary>
            /// Smaže všechny záznamy pro zadaný kontext.
            /// </summary>
            /// <param name="contxtId">Id zadaného kontextu</param>
            public void DeleteByContext(decimal contxtId)
            {
                DataRow[] rows = this.Select(string.Format("contxt_id = '{0}'", contxtId.ToString()));
                foreach (DataRow r in rows)
                    r.Delete();
                this.AcceptChanges();
            }
        }


        public partial class CONTXT_ACTDataTable
        {
            /// <summary>
            /// Vrátí počet záznamů v zadaném kontextu, které mají daný kód kategorie.
            /// </summary>
            /// <param name="contxtId">Id zadaného kontextu</param>
            /// <param name="cat_code">kód kategorie</param>
            /// <returns>počet záznamů</returns>
            public int GetCountInContext(decimal contxtId, string cat_code = null)
            {
                string filtr;
                if (cat_code == null)
                    filtr = string.Format("contxt_id = '{0}'", contxtId.ToString());
                else
                    filtr = string.Format("contxt_id = '{0}' and cat_code = '{1}'", contxtId.ToString(), cat_code);

                return (int)this.Compute("count(contxt_id)", filtr);
            }

            /// <summary>
            /// Smaže všechny záznamy pro zadaný kontext.
            /// </summary>
            /// <param name="contxtId">Id zadaného kontextu</param>
            public void DeleteByContext(decimal contxtId)
            {
                DataRow[] rows = this.Select(string.Format("contxt_id = '{0}'", contxtId.ToString()));
                foreach (DataRow r in rows)
                    r.Delete();
                this.AcceptChanges();
            }
        }

        public partial class CONTXT_OBJ_ITEM_FREEDataTable
        {
            /// <summary>
            /// Vrátí počet záznamů v zadaném kontextu, které mají daný kód kategorie.
            /// </summary>
            /// <param name="contxtId">Id zadaného kontextu</param>
            /// <param name="cat_code">kód kategorie</param>
            /// <returns>počet záznamů</returns>
            public int GetCountInContext(decimal contxtId, string cat_code = null)
            {
                string filtr;
                if (cat_code == null)
                    filtr = string.Format("contxt_id = '{0}'", contxtId.ToString());
                else
                    filtr = string.Format("contxt_id = '{0}' and cat_code = '{1}'", contxtId.ToString(), cat_code);

                return (int)this.Compute("count(contxt_id)", filtr);
            }

            /// <summary>
            /// Smaže všechny záznamy pro zadaný kontext.
            /// </summary>
            /// <param name="contxtId">Id zadaného kontextu</param>
            public void DeleteByContext(decimal contxtId)
            {
                DataRow[] rows = this.Select(string.Format("contxt_id = '{0}'", contxtId.ToString()));
                foreach (DataRow r in rows)
                    r.Delete();
                this.AcceptChanges();
            }
        }

        public partial class CONTXT_ORG_STRUCT_DETDataTable
        {
            /// <summary>
            /// Vrátí počet záznamů v zadaném kontextu, které mají daný kód kategorie.
            /// </summary>
            /// <param name="contxtId">Id zadaného kontextu</param>
            /// <param name="cat_code">kód kategorie</param>
            /// <returns>počet záznamů</returns>
            public int GetCountInContext(decimal contxtId, string cat_code = null)
            {
                string filtr;
                if (cat_code == null)
                    filtr = string.Format("contxt_id = '{0}'", contxtId.ToString());
                else
                    filtr = string.Format("contxt_id = '{0}' and OBJ_cat_code = '{1}'", contxtId.ToString(), cat_code);

                return (int)this.Compute("count(contxt_id)", filtr);
            }

            /// <summary>
            /// Smaže všechny záznamy pro zadaný kontext.
            /// </summary>
            /// <param name="contxtId">Id zadaného kontextu</param>
            public void DeleteByContext(decimal contxtId)
            {
                DataRow[] rows = this.Select(string.Format("contxt_id = '{0}'", contxtId.ToString()));
                foreach (DataRow r in rows)
                    r.Delete();
                this.AcceptChanges();
            }
        }
    }
}
