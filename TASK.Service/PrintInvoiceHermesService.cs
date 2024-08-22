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
    public class PrintInvoiceHermesService
    {
        public static List<HermesInvoice> _listInvoieHermesToInsert;
        public static List<HermesInvoiceDetail> _listInvoiceDetailToInsert;
        public static List<InvoiceCheckQuery> _listKeyInvoice;
        public static void GetDataFromHermes()
        {
            _listInvoieHermesToInsert = new List<HermesInvoice>();
            _listInvoiceDetailToInsert = new List<HermesInvoiceDetail>();
            _listKeyInvoice = new List<InvoiceCheckQuery>();
            ProcessData();
            try
            {
                using (TransactionScope scope = AppSetting.GetTransactionScope())
                {
                    if (_listInvoieHermesToInsert.Count > 0)
                    {
                        HermesInvoice.Insert(_listInvoieHermesToInsert);
                    }
                    if (_listInvoiceDetailToInsert.Count > 0)
                    {
                        HermesInvoiceDetail.Insert(_listInvoiceDetailToInsert);
                    }
                    if (_listKeyInvoice.Count > 0)
                    {
                        InvoiceCheckQuery.Insert(_listKeyInvoice);
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
            List<InvoiceCheckQuery> invoiceChecks = InvoiceCheckQuery.GetDataProcess();
            if (invoiceChecks.Count > 0)
            {

                foreach (var invoiceCheck in invoiceChecks)
                {
                    bool chechInvoiceExist = true;
                    InvoiceViewModel invoiceViewModel = new PrintAccess().GetAllInvoice(invoiceCheck.InvoiceIsn.ToString(), ref chechInvoiceExist, invoiceCheck);
                    if(!string.IsNullOrEmpty(invoiceViewModel.InvoiceIsn))
                    {
                        #region check dk
                        if (chechInvoiceExist)
                        {

                            List<HermesInvoice> listHermesInvoice = HermesInvoice.GetByDay();

                            if (!listHermesInvoice.Any(c => c.InvoiceIsn == invoiceViewModel.InvoiceIsn))
                            {
                                HermesInvoice obj = new HermesInvoice();
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
                                obj.InvoiceCheckAddtional = true;
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


                                List<InvoiceDetailViewModel> InvoiceDetailsViewModels = new InvoiceLineAccess().GetAllInvoice(obj).OrderByDescending(c => c.TotalAmount).ToList();
                                bool checkVatCorrect = true;
                                if(InvoiceDetailsViewModels.Any(c=>c.TotalVatAmount==0))
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
                                            itemResult.StorageTimeLog = 0;
                                            string[] data = item.Data.Split(';');
                                            string[] title = item.Title.Split(';');
                                            try
                                            {
                                                #region xu ly gia
                                                #region Mincharge
                                                if (item.Title.Contains("MINCHARGED"))
                                                {
                                                    itemResult.Unit = "AWB";
                                                    itemResult.Quantity = 1;
                                                    itemResult.UnitPrice = item.TotalAmount;
                                                    //luu log storageTime
                                                    if (item.Title.Contains("STRG_TIME") && obj.ObjectType == "IMPORT AWB")
                                                    {
                                                        int index = Array.FindIndex(title, m => m == "UNITQTY");
                                                        double quantityStr = double.Parse(data[index].ToString());
                                                        itemResult.StorageTimeLog = quantityStr;
                                                    }
                                                }
                                                #endregion
                                                #region Xu ly hang le
                                                else if (item.Title == "MEASURE;UNITQTY;RATE")
                                                {
                                                    if (!check)
                                                    {
                                                        itemResult.Unit = "KG";
                                                        itemResult.Quantity = Math.Ceiling(double.Parse(item.Data.Split(';')[1].ToString()));
                                                        itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                                    }
                                                    else
                                                    {
                                                        itemResult.Unit = "KG";
                                                        itemResult.Quantity = double.Parse(item.Data.Split(';')[1].ToString());
                                                        itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                                        if (obj.ObjectType == "EXPORT AWB" && (itemResult.UnitPrice.ToString().Contains(".01") || itemResult.UnitPrice.ToString().Contains(".99")))
                                                        {
                                                            itemResult.UnitPrice = Math.Round(itemResult.UnitPrice);
                                                        }
                                                    }
                                                }
                                                #endregion
                                                #region Xu ly hang le 2
                                                else if (item.Title == "MEASURE;UNITQTY")
                                                {
                                                    if (!check)
                                                    {
                                                        itemResult.Unit = "KG";
                                                        itemResult.Quantity = Math.Ceiling(double.Parse(item.Data.Split(';')[1].ToString()));
                                                        itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                                    }
                                                    else
                                                    {
                                                        itemResult.Unit = "KG";
                                                        itemResult.Quantity = double.Parse(item.Data.Split(';')[1].ToString());
                                                        itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                                        if (obj.ObjectType == "EXPORT AWB" && (itemResult.UnitPrice.ToString().Contains(".01") || itemResult.UnitPrice.ToString().Contains(".99")))
                                                        {
                                                            itemResult.UnitPrice = Math.Round(itemResult.UnitPrice);
                                                        }
                                                    }
                                                }
                                                #endregion
                                                #region handling StorageTime
                                                else if (item.Des.Contains("Luu kho") || item.Des.Contains("bao quan"))
                                                {
                                                    #region luu kho hang nhap
                                                    if (obj.ObjectType == "IMPORT AWB")
                                                    {
                                                        string storeTime = data[3].ToString();
                                                        if (item.Data.Contains("KG*HOURS"))
                                                        {
                                                            itemResult.Unit = "KG x gio";
                                                            int index = Array.FindIndex(title, m => m == "UNITQTY");
                                                            double quantityStr = double.Parse(data[index].ToString());
                                                            itemResult.Quantity = quantityStr;
                                                            itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                                            itemResult.StorageTimeLog = quantityStr;
                                                            List<HermesInvoice> invoices = HermesInvoice.CheckExistListAwb(obj.InvoiceIsn, obj.AWB, obj.Hawb);
                                                            List<HermesInvoiceDetail> listinvoiceDetail = new List<HermesInvoiceDetail>();
                                                            if (invoices.Count > 0)
                                                            {
                                                                foreach (var invoice in invoices)
                                                                {
                                                                    listinvoiceDetail.AddRange(HermesInvoiceDetail.GetByInvoiceIsn(invoice.InvoiceIsn));
                                                                }
                                                                double quantityInvoiceDetails = listinvoiceDetail.Where(c => c.Item == item.Des).Sum(c => c.StorageTimeLog).Value;
                                                                if (quantityInvoiceDetails > 0)
                                                                {
                                                                    itemResult.Quantity = itemResult.Quantity - quantityInvoiceDetails;
                                                                    if (itemResult.Quantity <= 0)
                                                                    {
                                                                        itemResult.Quantity = quantityStr;
                                                                    }
                                                                    itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                                                    itemResult.StorageTimeLog = itemResult.Quantity;
                                                                }
                                                            }

                                                        }
                                                        else
                                                        {
                                                            itemResult.Unit = "KG x ngay";
                                                            int index = Array.FindIndex(title, m => m == "UNITQTY");
                                                            double quantityStr = double.Parse(data[index].ToString());
                                                            if (quantityStr == 0)
                                                                quantityStr = 1;
                                                            itemResult.Quantity = quantityStr;

                                                            itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                                            itemResult.StorageTimeLog = quantityStr;
                                                            //tiep tuc kiem tra xem lo hang co phai la hang nhap va duoc luu kho truoc do hay ko
                                                            List<HermesInvoice> invoices = HermesInvoice.CheckExistListAwb(obj.InvoiceIsn, obj.AWB, obj.Hawb);
                                                            List<HermesInvoiceDetail> listinvoiceDetail = new List<HermesInvoiceDetail>();
                                                            if (invoices.Count > 0)
                                                            {
                                                                foreach (var invoice in invoices)
                                                                {
                                                                    listinvoiceDetail.AddRange(HermesInvoiceDetail.GetByInvoiceIsn(invoice.InvoiceIsn));
                                                                }
                                                                double quantityInvoiceDetails = listinvoiceDetail.Where(c => c.Item == item.Des).Sum(c => c.StorageTimeLog).Value;
                                                                if (quantityInvoiceDetails > 0)
                                                                {
                                                                    itemResult.Quantity = itemResult.Quantity - quantityInvoiceDetails;
                                                                    if (itemResult.Quantity <= 0)
                                                                    {
                                                                        itemResult.Quantity = quantityStr;
                                                                    }
                                                                    itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                                                    itemResult.StorageTimeLog = itemResult.Quantity;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                    #region luu kho hang xuat
                                                    else
                                                    {
                                                        if (item.TotalAmount == 90000)
                                                        {
                                                            itemResult.Unit = "AWB";
                                                            itemResult.Quantity = 1;
                                                            itemResult.UnitPrice = item.TotalAmount;
                                                        }
                                                        else if (itemResult.Des.Trim() == "Gia bao quan doi voi hang nguy hiem ngoai tru vu khi, chat no")
                                                        {
                                                            if (item.TotalAmount == 159000)
                                                            {
                                                                itemResult.Unit = "AWB";
                                                                itemResult.Quantity = 1;
                                                                itemResult.UnitPrice = item.TotalAmount;
                                                            }
                                                            else
                                                            {
                                                                itemResult.Unit = "KG x ngay";
                                                                int index = Array.FindIndex(title, m => m == "WEIGHT001");
                                                                double quantityStr = double.Parse(data[index].ToString());
                                                                if (!check)
                                                                {
                                                                    quantityStr = Math.Ceiling(quantityStr);
                                                                }
                                                                int storeDay = string.IsNullOrEmpty(item.Remark.Trim()) ? 1 : int.Parse(item.Remark);
                                                                double quantity = quantityStr * storeDay;
                                                                itemResult.Quantity = quantity;
                                                                itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                                            }

                                                        }
                                                        else if (itemResult.Des.Trim() == "Gia dich vu bao quan doi voi hang thong thuong (2)" || itemResult.Des.Trim() == "Gia dich vu bao quan doi voi hang thong thuong (3)"
                                                              || itemResult.Des.Trim() == "Gia dich vu bao quan doi voi hang kho bao quan"
                                                              || itemResult.Des.Trim() == "Gia dich vu bao quan doi voi hang bao quan lanh")
                                                        {
                                                            if (item.Data.Contains("KG*HOURS"))
                                                            {
                                                                itemResult.Unit = "KG x gio";
                                                            }
                                                            else
                                                            {
                                                                itemResult.Unit = "KG x ngay";
                                                            }
                                                            double weight = 1;
                                                            if (!check)
                                                            {
                                                                weight = Math.Ceiling(item.Weight);
                                                            }
                                                            else
                                                            {
                                                                weight = item.Weight;
                                                            }
                                                            int storeDay = string.IsNullOrEmpty(item.Remark.Trim()) ? 1 : int.Parse(item.Remark);
                                                            double quantity = weight * storeDay;
                                                            itemResult.Quantity = quantity;
                                                            itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                                        }
                                                        else
                                                        {
                                                            string storeTime = data[3].ToString();
                                                            itemResult.Unit = "KG x ngay";
                                                            int index = Array.FindIndex(title, m => m == "UNITQTY");
                                                            double quantityStr = double.Parse(data[index].ToString());
                                                            if (!check)
                                                            {
                                                                quantityStr = Math.Ceiling(quantityStr);
                                                            }
                                                            itemResult.Quantity = quantityStr;
                                                            itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                #endregion
                                                #region Huy Hoa Don
                                                else if (item.Data.Split(';')[0].ToString() == "NOT APPLICABLE")
                                                {
                                                    itemResult.Unit = "AWB";
                                                    if (item.Des.Trim() == "Dich vu xu ly bo sung theo yeu cau tu khach")
                                                    {
                                                        itemResult.UnitPrice = 276000;
                                                        itemResult.Quantity = Math.Round(item.TotalAmount / itemResult.UnitPrice, 2);
                                                    }
                                                    else
                                                    {
                                                        itemResult.Quantity = 1;
                                                        itemResult.UnitPrice = item.TotalAmount;
                                                    }
                                                }
                                                #endregion
                                                #region Dich vu phat sinh
                                                else if (item.Title.Contains("PROMPTED"))
                                                {
                                                    double unit = double.Parse(data[data.Length - 1].ToString());
                                                    itemResult.Unit = "don vi";
                                                    itemResult.Quantity = unit;
                                                    itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                                }
                                                else if (item.Title == "WEIGHT;MEASURE;UNITQTY")
                                                {
                                                    if (item.Des.Trim() == "Phi goi Khong van don thu cap dien tu")
                                                    {
                                                        itemResult.Unit = "AWB";
                                                        itemResult.UnitPrice = 76000;
                                                        itemResult.Quantity = Math.Round(item.TotalAmount / itemResult.UnitPrice, 2);
                                                    }
                                                    else if (item.Des.Trim() == "phi goi khong van don dien tu")
                                                    {
                                                        itemResult.Unit = "AWB";
                                                        itemResult.UnitPrice = 100000;
                                                        itemResult.Quantity = Math.Round(item.TotalAmount / itemResult.UnitPrice, 2);
                                                    }
                                                    else
                                                    {
                                                        itemResult.Unit = "AWB";
                                                        itemResult.Quantity = 1;
                                                        itemResult.UnitPrice = item.TotalAmount;
                                                    }
                                                }
                                                else if (item.Title.Contains("MEASURE;UNITQTY;STRG_TIME"))
                                                {
                                                    itemResult.Unit = "KG";
                                                    int index = Array.FindIndex(title, m => m == "UNITQTY");
                                                    double quantityStr = double.Parse(data[index].ToString());
                                                    itemResult.Quantity = quantityStr;
                                                    itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                                }
                                                #endregion
                                                #region Cac TH Khac
                                                else
                                                {
                                                    itemResult.Unit = "AWB";
                                                    itemResult.Quantity = 1;
                                                    itemResult.UnitPrice = item.TotalAmount;
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
                                #region xu ly thieu dich vu
                                if (true)
                                {
                                    if (((decimal)invoiceDetailsViewModelResults.Sum(c => c.TotalAmount) == obj.InvoiceTotalNoVatAmount) && checkVatCorrect)
                                    {
                                        //obj.Retry = 4;
                                        //obj.InvoiceCheckAddtional = false;
                                        //obj.Note = "Thiếu dịch vụ hoặc lệch tiền";
                                        _listInvoieHermesToInsert.Add(obj);
                                        foreach (var item in invoiceDetailsViewModelResults)
                                        {
                                            HermesInvoiceDetail objDetail = new HermesInvoiceDetail();
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
                                            _listInvoiceDetailToInsert.Add(objDetail);
                                        }
                                        invoiceCheck.Status = true;
                                        InvoiceCheckQuery.Update(invoiceCheck);
                                    }
                                    else if (((decimal)invoiceDetailsViewModelResults.Sum(c => c.TotalAmount) != obj.InvoiceTotalNoVatAmount) || !checkVatCorrect || invoiceCheck.Retry > 5)
                                    {

                                        obj.Retry = 4;
                                        obj.InvoiceCheckAddtional = false;
                                        obj.Note = "Thiếu dịch vụ hoặc lệch tiền";
                                        _listInvoieHermesToInsert.Add(obj);
                                        foreach (var item in invoiceDetailsViewModelResults)
                                        {
                                            HermesInvoiceDetail objDetail = new HermesInvoiceDetail();
                                            objDetail.InvoiceIns = item.InvoiceIsn;
                                            objDetail.InvoiceLineIns = item.InvoiceLineIsn;
                                            objDetail.Item = item.Des;
                                            objDetail.Unit = "AWB";
                                            objDetail.Amount = (decimal)item.TotalAmount;
                                            objDetail.TaxRate = item.PERCENTAGE.ToString();
                                            objDetail.VAT = (decimal)item.TotalVatAmount;
                                            objDetail.Unit = item.Unit;
                                            objDetail.UnitPrice = (decimal)item.UnitPrice;
                                            objDetail.Quantity = Math.Round(item.Quantity,3);
                                            objDetail.StorageTimeLog = item.StorageTimeLog;
                                            _listInvoiceDetailToInsert.Add(objDetail);
                                        }
                                        invoiceCheck.Status = true;
                                        InvoiceCheckQuery.Update(invoiceCheck);
                                    }
                                    else
                                    {
                                        Log.WriteLog("Xay ra loi thieu dich vu" + obj.InvoiceIsn, obj.InvoiceIsn);
                                        invoiceCheck.Status = false;
                                        invoiceCheck.Retry = invoiceCheck.Retry + 1;
                                        InvoiceCheckQuery.Update(invoiceCheck);
                                    }
                                }
                                else
                                {
                                    //if ((decimal)invoiceDetailsViewModelResults.Sum(c => c.TotalVatAmount) == obj.InvoiceTotalVatAmount)
                                    //{
                                    //    obj.Retry = 4;
                                    //    obj.InvoiceCheckAddtional = false;
                                    //    obj.Note = "Lệch thông tin VAT";
                                    //}
                                    _listInvoieHermesToInsert.Add(obj);
                                    foreach (var item in invoiceDetailsViewModelResults)
                                    {
                                        HermesInvoiceDetail objDetail = new HermesInvoiceDetail();
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
                                        _listInvoiceDetailToInsert.Add(objDetail);
                                    }
                                    invoiceCheck.Status = true;
                                    InvoiceCheckQuery.Update(invoiceCheck);
                                }
                                #endregion
                            }

                        }
                       
                        else
                        {
                            if (invoiceCheck.Retry < 20)
                            {
                                invoiceCheck.Status = false;
                            invoiceCheck.Retry = invoiceCheck.Retry + 1;
                           
                                InvoiceCheckQuery.Update(invoiceCheck);
                            }
                            else
                            {
                                invoiceCheck.Status = true;
                                invoiceCheck.Retry = invoiceCheck.Retry + 1;

                                InvoiceCheckQuery.Update(invoiceCheck);
                            }

                        }
                        #endregion KT check
                    }
                    else
                    {
                        if (invoiceCheck.Retry < 20)
                        {
                            invoiceCheck.Status = false;
                        invoiceCheck.Retry = invoiceCheck.Retry + 1;
                      
                            InvoiceCheckQuery.Update(invoiceCheck);
                        }
                        else
                        {
                            invoiceCheck.Status = true;
                            invoiceCheck.Retry = invoiceCheck.Retry + 1;

                            InvoiceCheckQuery.Update(invoiceCheck);
                        }
                    }

                }
            }

        }
        public static void ProcessDataTest()
        {
            //DateTime dt = new DateTime(2020, 10, 29, 0, 0, 0);
            //List<InvoiceCheckQuery> invoiceChecks = InvoiceCheckQuery.GetDataCancel(dt);
            string invoiceIsn = "30048332331817";
            InvoiceCheckQuery invoiceCheck = new InvoiceCheckQuery();
            bool chechInvoiceExist = true;
            InvoiceViewModel invoiceViewModel = new PrintAccess().GetAllInvoice(invoiceIsn, ref chechInvoiceExist, invoiceCheck);
       //     InvoiceViewModel invoiceViewModel = new PrintAccess().GetAllInvoiceTest(invoiceIsn);
            HermesInvoice obj = new HermesInvoice();
            if(!string.IsNullOrEmpty(invoiceViewModel.InvoiceIsn))
            {
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
                obj.AWB_Serial = invoiceViewModel.Awb_No;
                obj.Email = invoiceViewModel.FullEmail.TrimEnd(';');
                obj.AWB = invoiceViewModel.Awb_Prefix + "-" + obj.AWB_Serial;
                obj.Hawb = invoiceViewModel.Hawb;
                obj.InvoiceDatime = invoiceViewModel.InvoiceDateTime.HasValue ? invoiceViewModel.InvoiceDateTime : DateTime.Now;
                obj.KundName = invoiceViewModel.Customer_Name;
                obj.PersonName = invoiceViewModel.PersonName;
                obj.Address = invoiceViewModel.Street.TrimEnd(',').Replace(", ,", ",");
                obj.TaxCode = invoiceViewModel.VatID;
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
                _listInvoieHermesToInsert.Add(obj);

                List<InvoiceDetailViewModel> InvoiceDetailsViewModels = new InvoiceLineAccess().GetAllInvoice(obj).OrderByDescending(c => c.TotalAmount).ToList();
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
                        if (item.Des.Contains("xu ly") && !invoiceDetailsViewModelCheckRound.Any(c => c.Des == item.Des))
                        {
                            invoiceDetail.Des = item.Des;
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
                            itemResult.Des = item.Des;
                            itemResult.TotalAmount = item.TotalAmount;
                            itemResult.TotalVatAmount = item.TotalVatAmount;
                            itemResult.PERCENTAGE = item.PERCENTAGE;
                            itemResult.StorageTimeLog = 0;
                            string[] data = item.Data.Split(';');
                            string[] title = item.Title.Split(';');
                            try
                            {
                                #region xu ly gia
                                #region Mincharge
                                if (item.Title.Contains("MINCHARGED"))
                                {
                                    itemResult.Unit = "AWB";
                                    itemResult.Quantity = 1;
                                    itemResult.UnitPrice = item.TotalAmount;
                                    //luu log storageTime
                                    if (item.Title.Contains("STRG_TIME") && obj.ObjectType == "IMPORT AWB")
                                    {
                                        int index = Array.FindIndex(title, m => m == "UNITQTY");
                                        double quantityStr = double.Parse(data[index].ToString());
                                        itemResult.StorageTimeLog = quantityStr;
                                    }
                                }
                                #endregion
                                #region Xu ly hang le
                                else if (item.Title == "MEASURE;UNITQTY;RATE")
                                {
                                    if (!check)
                                    {
                                        itemResult.Unit = "KG";
                                        itemResult.Quantity = Math.Ceiling(double.Parse(item.Data.Split(';')[1].ToString()));
                                        itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                    }
                                    else
                                    {
                                        itemResult.Unit = "KG";
                                        itemResult.Quantity = double.Parse(item.Data.Split(';')[1].ToString());
                                        itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                        if (obj.ObjectType == "EXPORT AWB" && (itemResult.UnitPrice.ToString().Contains(".01") || itemResult.UnitPrice.ToString().Contains(".99")))
                                        {
                                            itemResult.UnitPrice = Math.Round(itemResult.UnitPrice);
                                        }
                                    }
                                }
                                #endregion
                                #region Xu ly hang le 2
                                else if (item.Title == "MEASURE;UNITQTY")
                                {
                                    if (!check)
                                    {
                                        itemResult.Unit = "KG";
                                        itemResult.Quantity = Math.Ceiling(double.Parse(item.Data.Split(';')[1].ToString()));
                                        itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                    }
                                    else
                                    {
                                        itemResult.Unit = "KG";
                                        itemResult.Quantity = double.Parse(item.Data.Split(';')[1].ToString());
                                        itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                        if (obj.ObjectType == "EXPORT AWB" && (itemResult.UnitPrice.ToString().Contains(".01") || itemResult.UnitPrice.ToString().Contains(".99")))
                                        {
                                            itemResult.UnitPrice = Math.Round(itemResult.UnitPrice);
                                        }
                                    }
                                }
                                #endregion

                                #region handling StorageTime
                                else if (item.Des.Contains("Luu kho"))
                                {
                                    #region luu kho hang nhap
                                    if (obj.ObjectType == "IMPORT AWB")
                                    {
                                        string storeTime = data[3].ToString();
                                        if (item.Data.Contains("KG*HOURS"))
                                        {
                                            itemResult.Unit = "KG x gio";
                                            int index = Array.FindIndex(title, m => m == "UNITQTY");
                                            double quantityStr = double.Parse(data[index].ToString());
                                            itemResult.Quantity = quantityStr;
                                            itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                            itemResult.StorageTimeLog = quantityStr;
                                            List<HermesInvoice> invoices = HermesInvoice.CheckExistListAwb(obj.InvoiceIsn, obj.AWB, obj.Hawb);
                                            List<HermesInvoiceDetail> listinvoiceDetail = new List<HermesInvoiceDetail>();
                                            if (invoices.Count > 0)
                                            {
                                                foreach (var invoice in invoices)
                                                {
                                                    listinvoiceDetail.AddRange(HermesInvoiceDetail.GetByInvoiceIsn(invoice.InvoiceIsn));
                                                }
                                                double quantityInvoiceDetails = listinvoiceDetail.Where(c => c.Item == item.Des).Sum(c => c.StorageTimeLog).Value;
                                                if (quantityInvoiceDetails > 0)
                                                {
                                                    itemResult.Quantity = itemResult.Quantity - quantityInvoiceDetails;
                                                    if (itemResult.Quantity <= 0)
                                                    {
                                                        itemResult.Quantity = quantityStr;
                                                    }
                                                    itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                                    itemResult.StorageTimeLog = itemResult.Quantity;
                                                }
                                            }

                                        }
                                        else
                                        {
                                            itemResult.Unit = "KG x ngay";
                                            int index = Array.FindIndex(title, m => m == "UNITQTY");
                                            double quantityStr = double.Parse(data[index].ToString());
                                            if (quantityStr == 0)
                                                quantityStr = 1;
                                            itemResult.Quantity = quantityStr;

                                            itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                            itemResult.StorageTimeLog = quantityStr;
                                            //tiep tuc kiem tra xem lo hang co phai la hang nhap va duoc luu kho truoc do hay ko
                                            List<HermesInvoice> invoices = HermesInvoice.CheckExistListAwb(obj.InvoiceIsn, obj.AWB, obj.Hawb);
                                            List<HermesInvoiceDetail> listinvoiceDetail = new List<HermesInvoiceDetail>();
                                            if (invoices.Count > 0)
                                            {
                                                foreach (var invoice in invoices)
                                                {
                                                    listinvoiceDetail.AddRange(HermesInvoiceDetail.GetByInvoiceIsn(invoice.InvoiceIsn));
                                                }
                                                double quantityInvoiceDetails = listinvoiceDetail.Where(c => c.Item == item.Des).Sum(c => c.StorageTimeLog).Value;
                                                if (quantityInvoiceDetails > 0)
                                                {
                                                    itemResult.Quantity = itemResult.Quantity - quantityInvoiceDetails;
                                                    if (itemResult.Quantity <= 0)
                                                    {
                                                        itemResult.Quantity = quantityStr;
                                                    }
                                                    itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                                    itemResult.StorageTimeLog = itemResult.Quantity;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    #region luu kho hang xuat
                                    else
                                    {
                                        if (itemResult.Des.Trim() == "Luu kho hang nguy hiem ngoai tru vu khi, chat no")
                                        {
                                            if (item.TotalAmount == 140000)
                                            {
                                                itemResult.Unit = "AWB";
                                                itemResult.Quantity = 1;
                                                itemResult.UnitPrice = item.TotalAmount;
                                            }
                                            else
                                            {
                                                itemResult.Unit = "KG x ngay";
                                                int index = Array.FindIndex(title, m => m == "WEIGHT001");
                                                double quantityStr = double.Parse(data[index].ToString());
                                                if (!check)
                                                {
                                                    quantityStr = Math.Ceiling(quantityStr);
                                                }
                                                //double weight = 1;
                                                //if (!check)
                                                //{
                                                //    weight = Math.Ceiling(item.Weight);
                                                //}
                                                //else
                                                //{
                                                //    weight = item.Weight;
                                                //}
                                                int storeDay = string.IsNullOrEmpty(item.Remark.Trim()) ? 1 : int.Parse(item.Remark);
                                                double quantity = quantityStr * storeDay;
                                                itemResult.Quantity = quantity;
                                                itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                            }

                                        }
                                        else if (itemResult.Des.Trim() == "Luu kho hang thong thuong (2)" || itemResult.Des.Trim() == "Luu kho hang thong thuong (3)"
                                            || itemResult.Des.Trim() == "Luu kho hang kho bao quan"
                                            || itemResult.Des.Trim() == "Luu kho hang bao quan lanh")
                                        {
                                            if (item.Data.Contains("KG*HOURS"))
                                            {
                                                itemResult.Unit = "KG x gio";
                                            }
                                            else
                                            {
                                                itemResult.Unit = "KG x ngay";
                                            }
                                            double weight = 1;
                                            if (!check)
                                            {
                                                weight = Math.Ceiling(item.Weight);
                                            }
                                            else
                                            {
                                                weight = item.Weight;
                                            }
                                            int storeDay = string.IsNullOrEmpty(item.Remark.Trim()) ? 1 : int.Parse(item.Remark);
                                            double quantity = weight * storeDay;
                                            itemResult.Quantity = quantity;
                                            itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                        }
                                        else
                                        {
                                            string storeTime = data[3].ToString();
                                            itemResult.Unit = "KG x ngay";
                                            int index = Array.FindIndex(title, m => m == "UNITQTY");
                                            double quantityStr = double.Parse(data[index].ToString());
                                            if (!check)
                                            {
                                                quantityStr = Math.Ceiling(quantityStr);
                                            }
                                            itemResult.Quantity = quantityStr;
                                            itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                            //if (itemResult.Des.Contains("hang nguy hiem") && obj.AWB_Prefix == "180")
                                            //{
                                            //    int storageDay = int.Parse(data[4]);
                                            //    if (storageDay > 1)
                                            //    {
                                            //        storageDay = storageDay - 1;
                                            //        double weight = double.Parse(data[13].ToString());
                                            //        itemResult.Quantity = weight * storageDay;
                                            //        itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                            //    }
                                            //}

                                        }
                                    }
                                    #endregion
                                }
                                #endregion
                                #region Huy Hoa Don
                                else if (item.Data.Split(';')[0].ToString() == "NOT APPLICABLE")
                                {
                                    itemResult.Unit = "AWB";
                                    if (item.Des.Trim() == "Dich vu xu ly bo sung theo yeu cau tu khach")
                                    {
                                        itemResult.UnitPrice = 250000;
                                        itemResult.Quantity = Math.Round(item.TotalAmount / itemResult.UnitPrice, 2);
                                    }
                                    else
                                    {
                                        itemResult.Quantity = 1;
                                        itemResult.UnitPrice = item.TotalAmount;
                                    }
                                }
                                #endregion
                                #region Dich vu phat sinh
                                else if (item.Title.Contains("PROMPTED"))
                                {
                                    double unit = double.Parse(data[data.Length - 1].ToString());
                                    itemResult.Unit = "don vi";
                                    itemResult.Quantity = unit;
                                    itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                }
                                else if (item.Title == "WEIGHT;MEASURE;UNITQTY")
                                {
                                    if (item.Des.Trim() == "Phi goi Khong van don thu cap dien tu")
                                    {
                                        itemResult.Unit = "AWB";
                                        itemResult.UnitPrice = 66000;
                                        itemResult.Quantity = Math.Round(item.TotalAmount / itemResult.UnitPrice, 2);
                                    }
                                    else if (item.Des.Trim() == "phi goi khong van don dien tu")
                                    {
                                        itemResult.Unit = "AWB";
                                        itemResult.UnitPrice = 88000;
                                        itemResult.Quantity = Math.Round(item.TotalAmount / itemResult.UnitPrice, 2);
                                    }
                                    else
                                    {
                                        itemResult.Unit = "AWB";
                                        itemResult.Quantity = 1;
                                        itemResult.UnitPrice = item.TotalAmount;
                                    }
                                }
                                else if (item.Title.Contains("MEASURE;UNITQTY;STRG_TIME"))
                                {
                                    itemResult.Unit = "KG";
                                    int index = Array.FindIndex(title, m => m == "UNITQTY");
                                    double quantityStr = double.Parse(data[index].ToString());
                                    itemResult.Quantity = quantityStr;
                                    itemResult.UnitPrice = Math.Round(item.TotalAmount / itemResult.Quantity, 2);
                                }
                                #endregion
                                #region Cac TH Khac
                                else
                                {
                                    itemResult.Unit = "AWB";
                                    itemResult.Quantity = 1;
                                    itemResult.UnitPrice = item.TotalAmount;
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
                #region xu ly thieu dich vu
                if (obj.ObjectType == "IMPORT AWB")
                {
                    if (((decimal)invoiceDetailsViewModelResults.Sum(c => c.TotalAmount) == obj.InvoiceTotalNoVatAmount))
                    {
                        //obj.Retry = 4;
                        //obj.InvoiceCheckAddtional = false;
                        //obj.Note = "Thiếu dịch vụ hoặc lệch tiền";
                        //_listInvoieHermesToInsert.Add(obj);
                        foreach (var item in invoiceDetailsViewModelResults)
                        {
                            HermesInvoiceDetail objDetail = new HermesInvoiceDetail();
                            objDetail.InvoiceIns = item.InvoiceIsn;
                            objDetail.InvoiceLineIns = item.InvoiceLineIsn;
                            objDetail.Item = item.Des;
                            objDetail.Unit = "AWB";
                            objDetail.Amount = (decimal)item.TotalAmount;
                            objDetail.TaxRate = item.PERCENTAGE.ToString();
                            objDetail.VAT = (decimal)item.TotalVatAmount;
                            objDetail.Unit = item.Unit;
                            objDetail.UnitPrice = (decimal)item.UnitPrice;
                            objDetail.Quantity = item.Quantity;
                            objDetail.StorageTimeLog = item.StorageTimeLog;
                            _listInvoiceDetailToInsert.Add(objDetail);
                        }
                        //invoiceCheck.Status = true;
                        //InvoiceCheckQuery.Update(invoiceCheck);
                    }
                    else if (((decimal)invoiceDetailsViewModelResults.Sum(c => c.TotalAmount) != obj.InvoiceTotalNoVatAmount))
                    {
                        obj.Retry = 4;
                        obj.InvoiceCheckAddtional = false;
                        obj.Note = "Thiếu dịch vụ hoặc lệch tiền";
                        _listInvoieHermesToInsert.Add(obj);
                        foreach (var item in invoiceDetailsViewModelResults)
                        {
                            HermesInvoiceDetail objDetail = new HermesInvoiceDetail();
                            objDetail.InvoiceIns = item.InvoiceIsn;
                            objDetail.InvoiceLineIns = item.InvoiceLineIsn;
                            objDetail.Item = item.Des;
                            objDetail.Unit = "AWB";
                            objDetail.Amount = (decimal)item.TotalAmount;
                            objDetail.TaxRate = item.PERCENTAGE.ToString();
                            objDetail.VAT = (decimal)item.TotalVatAmount;
                            objDetail.Unit = item.Unit;
                            objDetail.UnitPrice = (decimal)item.UnitPrice;
                            objDetail.Quantity = item.Quantity;
                            objDetail.StorageTimeLog = item.StorageTimeLog;
                            _listInvoiceDetailToInsert.Add(objDetail);
                        }
                        //invoiceCheck.Status = true;
                        //InvoiceCheckQuery.Update(invoiceCheck);
                    }
                    else
                    {
                        Log.WriteLog("Xay ra loi thieu dich vu" + obj.InvoiceIsn, obj.InvoiceIsn);
                        //invoiceCheck.Status = false;
                        //invoiceCheck.Retry = invoiceCheck.Retry + 1;
                        //InvoiceCheckQuery.Update(invoiceCheck);
                    }
                }
                else
                {
                    //if ((decimal)invoiceDetailsViewModelResults.Sum(c => c.TotalVatAmount) == obj.InvoiceTotalVatAmount)
                    //{
                    //    obj.Retry = 4;
                    //    obj.InvoiceCheckAddtional = false;
                    //    obj.Note = "Lệch thông tin VAT";
                    //}
                    _listInvoieHermesToInsert.Add(obj);
                    foreach (var item in invoiceDetailsViewModelResults)
                    {
                        HermesInvoiceDetail objDetail = new HermesInvoiceDetail();
                        objDetail.InvoiceIns = item.InvoiceIsn;
                        objDetail.InvoiceLineIns = item.InvoiceLineIsn;
                        objDetail.Item = item.Des;
                        objDetail.Unit = "AWB";
                        objDetail.Amount = (decimal)item.TotalAmount;
                        objDetail.TaxRate = item.PERCENTAGE.ToString();
                        objDetail.VAT = (decimal)item.TotalVatAmount;
                        objDetail.Unit = item.Unit;
                        objDetail.UnitPrice = (decimal)item.UnitPrice;
                        objDetail.Quantity = Math.Round(item.Quantity,2);
                        objDetail.StorageTimeLog = item.StorageTimeLog;
                        _listInvoiceDetailToInsert.Add(objDetail);
                    }
                    //invoiceCheck.Status = true;
                    //InvoiceCheckQuery.Update(invoiceCheck);
                }
                #endregion
                foreach (var item in invoiceDetailsViewModelResults)
                {
                    HermesInvoiceDetail objDetail = new HermesInvoiceDetail();
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
                    _listInvoiceDetailToInsert.Add(objDetail);
                }
                if ((decimal)invoiceDetailsViewModelResults.Sum(c => c.TotalAmount) == obj.InvoiceTotalNoVatAmount)
                {
                    Console.WriteLine("OK");
                }
                else
                {
                    Console.WriteLine("NOT OK");
                }
            }
            
        }
    }
}
