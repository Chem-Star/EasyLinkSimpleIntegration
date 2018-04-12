using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLinkSimpleIntegration.DataObjects
{
    public class EasyLinkOrder
    {
        public enum OrderType{ Inbound, Outbound }

        public string Record_Name                                       { get; set; }
        public string Record_ExternalName                               { get; set; }
        public string OwnerCode                                         { get; set; }
        public string SupplierCode                                      { get; set; }
        public string EstimatedShipDate                                 { get; set; }
        public string DesiredDeliveryDate                               { get; set; }
        public string PurchaseOrderNumber                               { get; set; }
        public string AdditionalOrderNumber                             { get; set; }
        public string AdditionalShipmentComments                        { get; set; }
        public string Requester_Email                                   { get; set; }
        public string Requester_Phone                                   { get; set; }
        public string SecondaryContact_FirstName                        { get; set; }
        public string SecondaryContact_LastName                         { get; set; }
        public string SecondaryContact_Company                          { get; set; }
        public string SecondaryContact_Phone                            { get; set; }
        public string SecondaryContact_Email                            { get; set; }
        public string Freight_CarrierService                            { get; set; }
        public string Freight_CarrierService_AccountNumber              { get; set; }
        public string Freight_CarrierService_TrackingNumber             { get; set; }
        public string Freight_BillTo_Type                               { get; set; }
        public string Freight_BillTo_Name                               { get; set; }
        public string Freight_BillTo_Company                            { get; set; }
        public string Freight_BillTo_Street1                            { get; set; }
        public string Freight_BillTo_Street2                            { get; set; }
        public string Freight_BillTo_Street3                            { get; set; }
        public string Freight_BillTo_City                               { get; set; }
        public string Freight_BillTo_State                              { get; set; }
        public string Freight_BillTo_PostalCode                         { get; set; }
        public string Freight_BillTo_Country                            { get; set; }
        public bool Freight_IsInternationalShipment                     { get; set; }
        public string Freight_InternationalShipment_ImporterOfRecord    { get; set; }
        public string Freight_MethodOfTransport                         { get; set; }
        public string ShipFrom_WarehouseCode                            { get; set; }
        public string ShipTo_WarehouseCode                              { get; set; }
        public string ShipFrom_Name                                     { get; set; }
        public string ShipFrom_Company                                  { get; set; }
        public string ShipFrom_Street1                                  { get; set; }
        public string ShipFrom_Street2                                  { get; set; }
        public string ShipFrom_Street3                                  { get; set; }
        public string ShipFrom_City                                     { get; set; }
        public string ShipFrom_State                                    { get; set; }
        public string ShipFrom_PostalCode                               { get; set; }
        public string ShipFrom_Country                                  { get; set; }
        public string ShipTo_Name                                       { get; set; }
        public string ShipTo_Company                                    { get; set; }
        public string ShipTo_Street1                                    { get; set; }
        public string ShipTo_Street2                                    { get; set; }
        public string ShipTo_Street3                                    { get; set; }
        public string ShipTo_City                                       { get; set; }
        public string ShipTo_State                                      { get; set; }
        public string ShipTo_PostalCode                                 { get; set; }
        public string ShipTo_Country                                    { get; set; }

        public List<EasyLinkOrderLine> LineItems;
    }
}
