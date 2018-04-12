using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLinkSimpleIntegration.DataObjects
{
    public class EasyLinkOrderLine
    {
        public string Record_LineNumber                 { get; set; }
        public string Product_OwnerPartNumber           { get; set; }
        public string Product_SupplierPartNumber        { get; set; }
        public string Product_RinchemPartNumber         { get; set; }
        public int Quantity                             { get; set; }
        public string LotNumber                         { get; set; }
        public string SerialNumber                      { get; set; }
        public string UnitOfMeasure                     { get; set; }
        public string PurchaseOrderNumber               { get; set; }
        public string AdditionalComments                { get; set; }
        public string Status                            { get; set; }
        public string Status_Reason                     { get; set; }
        public string Attributes_Destination            { get; set; }
        public string Attributes_Process                { get; set; }
        public string Attributes_Other                  { get; set; }
        public string Attributes_ComponentStatus        { get; set; }
    }
}
