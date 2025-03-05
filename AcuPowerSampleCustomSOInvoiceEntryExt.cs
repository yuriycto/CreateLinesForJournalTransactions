/*
https://acupowererp.com sample for Create lines for Journal Transaction from Invoice screen

*/
using AcuPowerSampleCustom.AR.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AcuPowerSampleCustom.SO
{
    public class AcuPowerSampleSOInvoiceEntryExt : PXGraphExtension<SOInvoiceEntry>
    {
        public static bool IsActive() => true;

        [PXOverride]
        public void ReleaseInvoiceProc(List<PX.Objects.AR.ARRegister> list, bool isMassProcess, Action<List<PX.Objects.AR.ARRegister>, bool> baseMethod)
        {
            PXGraph.InstanceCreated.AddHandler<JournalEntry>(graph => graph.RowInserted.AddHandler<Batch>(args =>
            {
                var allowance = SelectFrom<INItemClass>.Where<INItemClass.itemClassCD.IsEqual<@P.AsString>>.View.Select(graph, "ALLOWANCES").TopFirst;
                foreach (var arRegister in list)
                {
                    var invoiceDetails = SelectFrom<ARTran>.Where<ARTran.refNbr.IsEqual<@P.AsString>>.View.Select(graph, arRegister.RefNbr).FirstTableItems.ToList();

                    var allowanceDetails = invoiceDetails.Where(line =>
                    {
                        InventoryItem inventoryItem = SelectFrom<InventoryItem>.Where<InventoryItem.inventoryID.IsEqual<@P.AsInt>>.View.Select(graph, line.InventoryID).TopFirst;

                        if (inventoryItem != null)
                        {
                            return inventoryItem.ItemClassID == allowance.ItemClassID;
                        }
                        return false;
                    }).ToList();

                    List<ARTran> matchingAllowanceDetails = new List<ARTran>();

                    if (allowanceDetails.Any())
                    {
                        var customerAllowances = SelectFrom<AcuPowerSampleCustomerAllowance>
                            .Where<AcuPowerSampleCustomerAllowance.customerID.IsEqual<@P.AsInt>
                            .And<AcuPowerSampleCustomerAllowance.orderType.IsEqual<@P.AsString>>>
                            .View.Select(graph, arRegister.CustomerID, "SO").FirstTableItems.ToList();
                        matchingAllowanceDetails = allowanceDetails.Where(line =>
                        {
                            int? inventoryID = line.InventoryID;

                            return customerAllowances.Any(ca => ca.InventoryID == inventoryID);
                        }).ToList();
                    }

                    if (matchingAllowanceDetails.Any())
                    {
                        decimal totalInvoiceAmount = matchingAllowanceDetails.Sum(line => line.CuryExtPrice) ?? 0m;

                        decimal? percentage = 0m;
                        foreach (var item in matchingAllowanceDetails)
                        {
                            var allowanceItem = SelectFrom<AcuPowerSampleCustomerAllowance>
                                .Where<AcuPowerSampleCustomerAllowance.customerID.IsEqual<@P.AsInt>
                                .And<AcuPowerSampleCustomerAllowance.inventoryID.IsEqual<@P.AsInt>>>.View.Select(graph, arRegister.CustomerID, item.InventoryID).TopFirst;
                            percentage += allowanceItem.AllowancePct;
                        }
                        var inventoryItem = SelectFrom<InventoryItem>.Where<InventoryItem.inventoryID.IsEqual<@P.AsInt>>.View.Select(graph, matchingAllowanceDetails.FirstOrDefault().InventoryID).TopFirst;
                        var accrualAccountID = inventoryItem.POAccrualAcctID;
                        var expenseAccountID = inventoryItem.InvtAcctID;

                        GLTran debitEntry = new GLTran
                        {
                            BranchID = arRegister.BranchID,
                            AccountID = expenseAccountID,
                            //DebitAmt = totalInvoiceAmount * percentage / 100,
                            CuryDebitAmt = totalInvoiceAmount * percentage / 100,
                            RefNbr = arRegister.RefNbr,
                            TranDate = DateTime.Now,
                            Qty = matchingAllowanceDetails.Sum(line => line.Qty)
                        };

                        graph.GLTranModuleBatNbr.Insert(debitEntry);

                        GLTran creditEntry = new GLTran
                        {
                            BranchID = arRegister.BranchID,
                            AccountID = accrualAccountID,
                            //CreditAmt = totalInvoiceAmount * percentage / 100,
                            CuryCreditAmt = totalInvoiceAmount * percentage / 100,
                            RefNbr = arRegister.RefNbr,
                            TranDate = DateTime.Now,
                            Qty = matchingAllowanceDetails.Sum(line => line.Qty)
                        };
                        graph.GLTranModuleBatNbr.Insert(creditEntry);
                    }
                }
            }));

            baseMethod(list, isMassProcess);
        }
    }
}
