using CSP.Common;
using CSP.Common.Attributes;
using CSP.Plugins.Core;
using CSP.Plugins.Darecky.ActionExtensions;
using CSP.Plugins.Darecky.Models;
using CSP.Plugins.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace CSP.Plugins.Darceky
{
    [Plugin("Maco", "Maco - Par")]
    public sealed class Maco : PluginBase
    {
        #region Variables
        /// <summary>
        ///
        /// </summary>
        private string targetEmail;
        /// <summary>
        ///
        /// </summary>
        private string targetEmailBc;
        /// <summary>
        ///
        /// </summary>
        private string targetEmailSubject;
        /// <summary>
        ///
        /// </summary>
        private bool enableNoIncompleteOrdersNotification;
        /// <summary>
        ///
        /// </summary>
        private string checkHashFilePath;
        /// <summary>
        /// 
        /// </summary>
        private string MacoShoptetExportPath;
        /// <summary>
        /// 
        /// </summary>
        private string MacoShoptetExportFileName;
        #endregion

        #region Constructor
        /// <summary>
        ///
        /// </summary>
        public Maco()
            : base()
        {
        }
        #endregion

        #region API - MacoIncompleteNotification
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private OutParams MacoIncompleteOrdersNotification()
        {
            OutParams exportResult = Params.InitOutParams();

            #region Ziskani dat
            OutParams dataOut = base.CallAction("MacoIncompleteOrdersGet");
            if (!MainAct.IsOk(dataOut))
                return dataOut;
            #endregion

            DataTable incOrders = dataOut.Data.Tables[0];
            DataTable alternativesTable = dataOut.Data.Tables[1];

            if (incOrders.Rows.Count == 0)
            {
                string message = "Všechno objednané zboží je skladem.";
                if (this.enableNoIncompleteOrdersNotification)
                    this.SendEmailNotification(message);
                return PluginUtils.GetOkOutParams(message, false);
            }

            List<MacoNotificationAlternativeProduct> alternatives = new List<MacoNotificationAlternativeProduct>(0);
            List<MacoNotification> notifRecords = new List<MacoNotification>(0);

            #region Zpracovanie vystupu
            if (alternativesTable.Rows.Count > 0)
            {
                for (int i = 0; i <= alternativesTable.Rows.Count - 1; i++)
                {
                    DataRow current = alternativesTable.Rows[i];
                    MacoNotificationAlternativeProduct altProd = new MacoNotificationAlternativeProduct();
                    altProd.SourceProductId = Convert.ToInt32(current[0]);
                    altProd.Id = Convert.ToInt32(current[1]);
                    altProd.ProductCode = Convert.ToString(current[2]);
                    altProd.StockQuantity = Convert.ToInt32(current[3]);
                    altProd.ProductName = Convert.ToString(current[4]);
                    alternatives.Add(altProd);
                }
            }

            for (int i = 0; i <= incOrders.Rows.Count - 1; i++)
            {
                DataRow current = incOrders.Rows[i];
                MacoNotification item = new MacoNotification();
                item.Id = Convert.ToInt32(current[0]);
                item.ProductCode = Convert.ToString(current[1]);
                item.StockQuantity = Convert.ToInt32(current[2]);
                item.OrderedQuantity = Convert.ToInt32(current[3]);
                item.Difference = Convert.ToInt32(current[4]);
                item.ProductName = Convert.ToString(current[5]);
                item.PairCode = (current[6] == DBNull.Value ? null : (int?)Convert.ToInt32(current[6]));
                item.OrderId = Convert.ToInt32(current[7]);

                var foundAlternatives = alternatives.Where(w => w.SourceProductId == item.Id);
                if (foundAlternatives != null)
                    item.Alternatives.AddRange(foundAlternatives);

                notifRecords.Add(item);
            }
            #endregion

            var sameProductCodes = (from c in notifRecords
                                    group c by c.ProductCode into g
                                    select new
                                    {
                                        ProductCode = g.Key,
                                        ProductName = g.First().ProductName,
                                        Difference = g.First().Difference,
                                        Alternatives = g.First().Alternatives,
                                        Orders = g.Select(w => w.OrderId)
                                    }).ToList();

            StringBuilder content = new StringBuilder(notifRecords.Count);
            foreach (var item in sameProductCodes)
            {
                string orders = string.Join(",", item.Orders.ToArray());
                string message = $"IN: {orders} - chybí {Math.Abs((int)item.Difference)} ks {item.ProductCode} {item.ProductName}";

                int alternativeProductsCount = item.Alternatives.Count;
                if (item.Alternatives.Count > 0)
                {
                    message = string.Format("{0}, ale můžete nabídnout vartianty", message);
                    for (int i = 0; i <= alternativeProductsCount - 1; i++)
                    {
                        message = string.Format("{0} {1} {2}ks {3}", message,
                                                                     item.Alternatives[i].ProductCode,
                                                                     item.Alternatives[i].StockQuantity,
                                                                     i == alternativeProductsCount - 1 ? string.Empty : ",");
                    }
                }
                content.AppendLine(message);
            }

            string lastCheckContent = string.Empty;
            if (!File.Exists(this.checkHashFilePath))
                File.WriteAllText(this.checkHashFilePath, string.Empty);
            else
                lastCheckContent = File.ReadAllText(this.checkHashFilePath);

            string notificationContent = content.ToString();
            string hash = CreateMD5(notificationContent);

            if (lastCheckContent == hash)
            {
                StringBuilder builder = new StringBuilder(1);
                builder.AppendLine("Aktuální nekompletní objednávky jsou stejné jako v případé minulého běhu.");
                builder.AppendLine(notificationContent);
                exportResult.SetResult(ResultCodes.RC_OK, builder.ToString());
            }
            else
            {
                OutParams sendOut = this.SendEmailNotification(notificationContent);
                if (!MainAct.IsOk(sendOut))
                    return sendOut;
                exportResult.SetResult(ResultCodes.RC_OK, notificationContent);

                File.WriteAllText(this.checkHashFilePath, hash);
            }

            return exportResult;
        }
        #endregion


        #region SendEmailNotification
        /// <summary>
        ///
        /// </summary>
        /// <param name="content"></param>
        private OutParams SendEmailNotification(string content)
        {
            EmailAddresses addresses = new EmailAddresses();
            addresses.EmailFrom = ConfigurationManager.AppSettings["notif_email_from"];
            addresses.EmailsTo = this.targetEmail;
            addresses.EmailsToCc = this.targetEmailBc;

            OutParams sendOut = base.MainAction.EmailSend(addresses, this.targetEmailSubject, content);
            return sendOut;
        }
        #endregion

    }
}
