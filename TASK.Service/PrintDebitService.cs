using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TASK.Data;
using TASK.Settings;
using TASK.Model.ViewModel;
using TASK.Model.DBModel;
using DATAACCESS;


namespace TASK.Service
{
    public class PrintDebitService
    {
        public static List<HermesDebit> _listDebitHermesToInsert;
        public static List<HermesDebitDetail> _listDebitDetailToInsert;
        public static List<DebitCheckQuery> _listKeyDebit;
        public static void GetDataFromHermes()
        {
            _listDebitHermesToInsert = new List<HermesDebit>();
            _listDebitDetailToInsert = new List<HermesDebitDetail>();
            _listKeyDebit = new List<DebitCheckQuery>();
            ProcessData();
            try
            {
                using (TransactionScope scope = AppSetting.GetTransactionScope())
                {
                    if (_listDebitHermesToInsert.Count > 0)
                    {
                        HermesDebit.Insert(_listDebitHermesToInsert);
                    }
                    if (_listDebitDetailToInsert.Count > 0)
                    {
                        HermesDebitDetail.Insert(_listDebitDetailToInsert);
                    }
                    if (_listKeyDebit.Count > 0)
                    {
                        DebitCheckQuery.Insert(_listKeyDebit);
                    }

                    scope.Complete();
                }
            }

            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
            }
        }
        public static void ProcessData()
        {
            List<DebitCheckQuery> debitChecks = DebitCheckQuery.GetDataProcess();
            if (debitChecks.Count > 0)
            {

                foreach (var debitCheck in debitChecks)
                {
                    bool checkDebitExist = true;
                    InvoiceViewModel invoiceViewModel = new PrintAccess().GetAllDebit(debitCheck.InvoiceIsn, ref checkDebitExist, debitCheck);
                    if (!string.IsNullOrEmpty(invoiceViewModel.InvoiceIsn))
                    {
                        #region check dk
                        if (checkDebitExist)
                        {

                            List<HermesDebit> listHermesDebit = HermesDebit.GetByDay();

                            if (!listHermesDebit.Any(c => c.InvoiceIsn == invoiceViewModel.InvoiceIsn))
                            {
                                HermesDebit obj = new HermesDebit();
                                obj.InvoiceIsn = invoiceViewModel.InvoiceIsn;
                                obj.InvoiceRunIsn = invoiceViewModel.InvoiceRunIns;
                                obj.Payment = invoiceViewModel.Payment;
                                obj.InvoiceNumber = invoiceViewModel.No;
                                obj.InvoiceType = invoiceViewModel.InvoiceType;
                                obj.InvoiceDate = DateTime.Now.Date;
                                obj.InvoiceTotalAmount = (decimal)invoiceViewModel.Amount_Total;
                                obj.InvoiceTotalVatAmount = (decimal)invoiceViewModel.Amount_Vat;
                                obj.InvoiceTotalNoVatAmount = (decimal)invoiceViewModel.Amount_No_Vat;
                                obj.AWB_Prefix = invoiceViewModel.Awb_Prefix;
                                obj.AWB_Serial = invoiceViewModel.Awb_No.PadLeft(8, '0');
                                // obj.debitCheckAddtional = true;
                                obj.Email = invoiceViewModel.FullEmail.Trim(';');
                                obj.AWB = invoiceViewModel.Awb_Prefix + "-" + obj.AWB_Serial;
                                obj.Hawb = invoiceViewModel.Hawb;
                                obj.InvoiceDatime = invoiceViewModel.InvoiceDateTime.HasValue ? invoiceViewModel.InvoiceDateTime : DateTime.Now;
                                obj.KundName = invoiceViewModel.Customer_Name;
                                obj.PersonName = invoiceViewModel.PersonName;
                                obj.Address = invoiceViewModel.Street.Replace(", ,", ",").TrimEnd(',');
                                obj.TaxCode = invoiceViewModel.VatID;
                                obj.PaymentDescription = invoiceViewModel.Note;
                                obj.Status = false;
                                obj.Retry = 0;
                                obj.IsCancel = false;
                                obj.ObjectType = invoiceViewModel.ObjType;
                                obj.InvoiceLink = invoiceViewModel.InvoiceLink;
                                if (obj.ObjectType == "EXPORT AWB")
                                {
                                    obj.InvoiceLink = @"\\VM-SHARE\Hermes5Share\HL\Forms\" + DateTime.Now.ToString("yyyyMMdd") + @"\Cash_Invoice_" + obj.InvoiceNumber + ".rtf";
                                }
                                obj.PrinterNetworkName = invoiceViewModel.PrinterNetWork;
                                obj.Created = DateTime.Now;
                                obj.InvoiceStatus = Common.CommonConstants.INVOICEINITIAL;
                                obj.InvoiceDescription = Common.CommonConstants.INITIAL;


                                List<InvoiceDetailViewModel> InvoiceDetailsViewModels = new InvoiceLineAccess().GetAllDebit(obj).OrderByDescending(c => c.TotalAmount).ToList();
                                bool checkVatCorrect = true;
                                if (InvoiceDetailsViewModels.Any(c => c.TotalVatAmount == 0))
                                {
                                    checkVatCorrect = false;
                                }
                                List<InvoiceDetailViewModel> invoiceDetailsViewModelResults = new List<InvoiceDetailViewModel>();
                                List<InvoiceDetailViewModel> invoiceDetailsViewModelCheckRound = new List<InvoiceDetailViewModel>();
                                bool check = true;
                                double result = 0;
                                //Neu la hang KE cua hang xuat thi phai lam tron can
                                #region Xu ly tron can hang KE,CI
                                if (invoiceViewModel.ObjType == "EXPORT AWB" && (obj.AWB_Prefix == "180" || obj.AWB_Prefix == "297" || obj.AWB_Prefix == "539"))
                                {
                                    foreach (var item in InvoiceDetailsViewModels)
                                    {
                                        InvoiceDetailViewModel invoiceDetail = new InvoiceDetailViewModel();
                                        if ((item.Des.Contains("xu ly") || item.Des.Contains("dich vu doi voi")) && !invoiceDetailsViewModelCheckRound.Any(c => c.Des == item.Des))
                                        {
                                            invoiceDetail.Des = item.Des;
                                            if(!string.IsNullOrEmpty(item.Data))
                                                 invoiceDetail.Quantity = double.Parse(item.Data.Split(';')[1].ToString());
                                            invoiceDetailsViewModelCheckRound.Add(invoiceDetail);
                                        }

                                    }
                                    //lay danh sach ca loai hang dang xu ly
                                    foreach (var item in invoiceDetailsViewModelCheckRound)
                                    {
                                        result += item.Quantity;
                                    }
                                    int a = 0;
                                    check = int.TryParse(result.ToString(), out a);
                                }
                                #endregion
                                #region xu ly Invoice Detail
                                try
                                {
                                    foreach (var item in InvoiceDetailsViewModels)
                                    {

                                        InvoiceDetailViewModel itemResult = new InvoiceDetailViewModel();
                                        //neu chua ton tai dich vu thi them moi
                                        #region codeLogic
                                        if (!invoiceDetailsViewModelResults.Any(c => c.Des == item.Des))
                                        {
                                            itemResult.InvoiceIsn = item.InvoiceIsn;
                                            itemResult.GroupIsn = item.GroupIsn;
                                            //itemResult.Des = item.Des;
                                            if (item.Des.Trim().Contains("Xu ly"))
                                            {
                                                itemResult.Des = item.Des.Replace("Xu ly", "Gia dich vu doi voi");
                                            }
                                            else if (item.Des.Trim().Contains("Dich vu xu ly"))
                                            {
                                                itemResult.Des = item.Des.Replace("Dich vu xu ly", "Gia dich vu doi voi");
                                            }
                                            else if (item.Des.Trim().Contains("Luu kho"))
                                            {
                                                itemResult.Des = item.Des.Replace("Luu kho", "Gia bao quan doi voi");
                                            }
                                            else
                                            {
                                                itemResult.Des = item.Des;
                                            }
                                            itemResult.TotalAmount = item.TotalAmount;
                                            itemResult.TotalVatAmount = item.TotalVatAmount;
                                            itemResult.PERCENTAGE = item.PERCENTAGE;
                                            itemResult.Received_Weight = item.Received_Weight;
                                            itemResult.CW_Weight = item.CW_Weight;
                                            //itemResult.StorageTimeLog = 0;
                                            itemResult.MiCharged = item.MiCharged;
                                            itemResult.StorageTimeLog = string.IsNullOrEmpty(item.Remark.Trim()) ? 1 : int.Parse(item.Remark);
                                            try
                                            {
                                                #region xu ly gia
                                                #region Mincharge
                                                if (item.MiCharged)
                                                {
                                                    itemResult.Unit = "AWB";
                                                    itemResult.Quantity = 1;
                                                    itemResult.UnitPrice = item.TotalAmount;
                                                    //luu log storageTime
                                                    //if (item.Title.Contains("STRG_TIME") && obj.ObjectType == "IMPORT AWB")
                                                    //{
                                                    //    int index = Array.FindIndex(title, m => m == "UNITQTY");
                                                    //    double quantityStr = double.Parse(data[index].ToString());
                                                    //    itemResult.StorageTimeLog = quantityStr;
                                                    //}
                                                }
                                                #endregion


                                                #region Cac TH Khac
                                                else
                                                {
                                                    itemResult.Unit = item.Unit;
                                                    itemResult.Quantity = item.Quantity;
                                                    //itemResult.UnitPrice = item.TotalAmount;
                                                }
                                                #endregion
                                                #endregion
                                            }
                                            catch (Exception ex)
                                            {
                                                itemResult.Unit = "AWB";
                                                itemResult.Quantity = 1;
                                                itemResult.UnitPrice = item.TotalAmount;

                                                Log.WriteLog(ex.ToString() + obj.InvoiceIsn, obj.InvoiceIsn);
                                            }

                                            invoiceDetailsViewModelResults.Add(itemResult);
                                        }
                                        else
                                        {
                                            //sum lai gia va VAT
                                            var objResult = invoiceDetailsViewModelResults.FirstOrDefault(p => p.Des == item.Des);
                                            objResult.TotalAmount += item.TotalAmount;
                                            objResult.TotalVatAmount += item.TotalVatAmount;
                                            double unitPriceExtention = Math.Round(item.TotalAmount / objResult.Quantity, 2);
                                            if (obj.ObjectType == "EXPORT AWB" && (unitPriceExtention.ToString().Contains(".01") || unitPriceExtention.ToString().Contains(".99")))
                                            {
                                                unitPriceExtention = Math.Round(unitPriceExtention);
                                            }
                                            objResult.UnitPrice += unitPriceExtention;

                                        }
                                        #endregion
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.WriteLog(ex.ToString() + obj.InvoiceIsn, obj.InvoiceIsn);
                                }
                                #endregion
                                _listDebitHermesToInsert.Add(obj);
                                foreach (var item in invoiceDetailsViewModelResults)
                                {
                                    HermesDebitDetail objDetail = new HermesDebitDetail();
                                    objDetail.InvoiceIns = item.InvoiceIsn;
                                    objDetail.InvoiceLineIns = item.InvoiceLineIsn;
                                    objDetail.Item = item.Des;
                                    objDetail.Unit = "AWB";
                                    objDetail.Amount = (decimal)item.TotalAmount;
                                    objDetail.TaxRate = item.PERCENTAGE.ToString();
                                    objDetail.VAT = (decimal)item.TotalVatAmount;
                                    objDetail.Unit = item.Unit;
                                    objDetail.UnitPrice = (decimal)item.UnitPrice;
                                    objDetail.Quantity = Math.Round(item.Quantity, 3);
                                    objDetail.StorageTimeLog = item.StorageTimeLog;
                                    objDetail.CW = item.CW_Weight;
                                    objDetail.Received_Weight = item.Received_Weight;
                                    objDetail.MinCharged = item.MiCharged;
                                    _listDebitDetailToInsert.Add(objDetail);
                                }
                                debitCheck.Status = true;
                                DebitCheckQuery.Update(debitCheck);
                            }
                        }

                    }


                    #endregion KT check



                }
            }

        }
    }
}
