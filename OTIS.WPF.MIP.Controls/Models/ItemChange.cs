using System;

namespace OTR.WPF.DIP.Controls.Models
{
    /// <summary>
    /// Objekt, reprezentujúci zmeny, ktoré na objekte prebehli.
    /// </summary>
    public class ItemChange
    {
        private decimal id;
        /// <summary>
        /// Vracia identifikátor objektu v kontexte.
        /// </summary>
        public decimal Id
        {
            get
            {
                return id;
            }
        }

        private string catCode;
        /// <summary>
        /// Vracia kategórii zmeny, ktorá sa stala.
        /// </summary>
        public string CatCode
        {
            get
            {
                return catCode;
            }
        }

        private DateTime dataTime;
        /// <summary>
        /// Vracia dátum a čas, kedy k zmene došlo.
        /// </summary>
        public DateTime DateTime
        {
            get
            {
                return dataTime;
            }
        }

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="id">Identifikátor objektu</param>
        /// <param name="catCode">Kategórie zmeny, ktorá sa stala</param>
        /// <param name="dateTime">Dátum a čas, kedy k zmene došlo</param>
        public ItemChange(decimal id, string catCode, DateTime dateTime)
        {
            this.id = id;
            this.catCode = catCode;
            this.dataTime = dateTime;
        }
    }
}