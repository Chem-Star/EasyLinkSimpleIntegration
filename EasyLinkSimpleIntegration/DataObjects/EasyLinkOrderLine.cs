using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLinkSimpleIntegration.DataObjects
{
    public class EasyLinkOrderLine
    {
        public string RecordLine_ExternalName           { get; set; }
        public string Product_OwnerPartNumber           { get; set; }
        public string Product_SupplierPartNumber        { get; set; }
        public string Product_RinchemPartNumber         { get; set; }
        public double Quantity                          { get; set; }
        public string LotNumber                         { get; set; }
        public string SerialNumber                      { get; set; }
        public string UnitOfMeasure                     { get; set; }
        public string LinePurchaseOrderNumber           { get; set; }
        public string AdditionalComments                { get; set; }
        public string LineCustomerField1                { get; set; }
        public string LineCustomerField2                { get; set; }
        public string LineCustomerField3                { get; set; }
        public string HoldCode                          { get; set; }
        public string HoldCode_Reason                   { get; set; }
        public string Attributes_Destination            { get; set; }
        public string Attributes_Process                { get; set; }
        public string Attributes_Other                  { get; set; }
        public string Attributes_ComponentStatus        { get; set; }

        public string  WMS_RecordLine_Number                 { get; set; }
        public string  WMS_RecordLine_CreatedDate            { get; set; }
        public string  WMS_RecordLine_LastModifiedDate       { get; set; }
        public string  WMS_RecordLine_ShippedDate            { get; set; }
        public string  WMS_RecordLine_ReceivedDate           { get; set; }
        public string  WMS_Product_RinchemPartNumber         { get; set; }
        public double  WMS_Quantity                          { get; set; }
        public string  WMS_LotNumber                         { get; set; }
        public string  WMS_SerialNumber                      { get; set; }
        public string  WMS_UnitOfMeasure                     { get; set; }
        public string  WMS_LinePurchaseOrderNumber           { get; set; }
        public string  WMS_AdditionalComments                { get; set; }
        public string  WMS_HoldCode                          { get; set; }
        public string  WMS_Attributes_Destination            { get; set; }
        public string  WMS_Attributes_Process                { get; set; }
        public string  WMS_Attributes_Other                  { get; set; }
        public string  WMS_Attributes_ComponentStatus        { get; set; }
    }
}
