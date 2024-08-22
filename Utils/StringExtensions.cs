using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utils
{
    public static class StringExtensions
    {
        public static string ToHex(this string value, Encoding enc)
        {
            // Encode the array of chars.
            StringBuilder returnString = new StringBuilder(string.Empty);
            foreach (var byteValue in enc.GetBytes(value))
                returnString.Append(String.Format("{0:X2}", byteValue));
            return returnString.ToString();
        }

        /// <summary>
        /// Return null nếu xâu rỗng hoặc null
        /// </summary>
        public static string ToNullIfNullOrEmpty(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            return value;
        }

        /// <summary>
        /// Nâng hoa ký tự đầu tiên của một xâu
        /// </summary>
        public static string UppercaseFirstLetter(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                char[] array = value.ToCharArray();
                array[0] = char.ToUpper(array[0]);
                return new string(array);
            }
            return value;
        }
        /// <summary>
        /// Đếm số từ của một xâu
        /// </summary>
        public static int WordCount(this string value)
        {
            return value.Split(new char[] { ' ', '.', '?', ',', '!', ':' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static bool ContainsUnicodeCharacter(this string str)
        {
            for (int i = 0, n = str.Length; i < n; i++)
            {
                if (str[i] > 127)
                {
                    return true;
                }
            }
            return false;
        }

        public static int GetMessageCount(this string str)
        {
            int letter = str.Length;
            int letterPermessage = 0;
            //if (!str.ContainsUnicodeCharacter())
            //{
            if (letter <= 160)
                letterPermessage = 160;
            else
                letterPermessage = 153;
            //}
            //else
            //{
            //    if (letter <= 70)
            //        letterPermessage = 70;
            //    else
            //        letterPermessage = 67;
            //}
            double message = Math.Floor(Convert.ToDouble(letter) / letterPermessage);
            var div = letter % letterPermessage;
            if (div > 0) message += 1;
            if (message == 0) message = 1;
            return Convert.ToInt32(message);
        }
        /// <summary>
        /// Kiểm tra một xâu có phải là số hay không. Trả về true nếu là số, không thì ngược lại
        /// </summary>
        public static bool IsNumeric(this string value)
        {
            double output;
            return double.TryParse(value, out output);
        }

        /// <summary>
        /// Kiểm tra một xâu có phải là bolean hay khoong;
        /// </summary>
        public static bool IsBolean(this string value)
        {
            bool output;
            return bool.TryParse(value, out output);
        }

        /// <summary>
        /// Convert 1 xâu thành 1 số nguyên
        /// </summary>
        public static int ToInt(this string value)
        {
            int output = 0;
            int.TryParse(value, out output);
            return output;
        }

        /// <summary>
        /// Convert 1 xâu thành 1 kiểu dữ liêu Long (Int64)
        /// </summary>
        public static long ToLong(this string value)
        {
            try
            {
                return long.Parse(value);
            }
            catch { return 0; }
        }
        /// <summary>
        /// Convert 1 xâu thành 1 số nguyên 
        /// </summary>
        public static int? ToNullaleInt(this string value)
        {
            int output;
            if (int.TryParse(value, out output))
                return output;
            return null;
        }
        /// <summary>
        /// Kiểm tra một xâu có phải là Email không
        /// </summary>
        public static bool IsEmail(this string value)
        {
            Regex reg = new Regex(@"^[a-z0-9_]+(?:\.[a-z0-9]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])$", RegexOptions.IgnoreCase);
            try
            {
                return reg.IsMatch(value);
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Kiểm tra một xâu có phải là URL không
        /// </summary>
        public static bool IsURL(this string value)
        {
            Regex reg = new Regex(@"^(http|https|ftp)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&%\$#\=~])*[^\.\,\)\(\s]$", RegexOptions.IgnoreCase);
            try
            {
                return reg.IsMatch(value);
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Kiểm tra một xâu có phải là DateTime không
        /// </summary>
        public static bool IsDateTime(this string value)
        {
            DateTime dt;
            return DateTime.TryParse(value, out dt);
        }
        /// <summary>
        /// Convert 1 xâu thành DateTime
        /// </summary>
        public static DateTime? ToNullableDateTime(this string value)
        {
            if (value.IsDateTime())
                return DateTime.Parse(value);
            return null;
        }
        /// <summary>
        /// Kiểm tra một xâu có phải là số la mã không
        /// </summary>
        public static bool IsRomanNumber(this string value)
        {
            string[] soThuTuLaMa = {"I","II","III","IV","V","VI","VII","VIII","IX","X",
                                                 "XI","XII","XIII","XIV","XV","XVI","XVII","XVIII","XIX","XX",
                                                 "XXI","XXII","XXIII","XXIV","XXV","XXVI","XXVII","XXVIII","XXIX","XXX"};
            if (soThuTuLaMa.Contains(value)) return true;
            return false;
        }
        /// <summary>
        /// Chuẩn hóa một xâu
        /// </summary>
        public static string ToNormalString(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return string.Join(" ", value.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)).Trim();
            }
            return string.Empty;
        }
        /// <summary>
        /// Chuẩn hóa 1 số điện thoại (ví dụ: 841682821740)
        /// </summary>
        public static string ToNormalPhoneNumber(this string value)
        {
            string normalPhoneNumber = string.Empty;
            foreach (char item in value)
            {
                if (item >= '0' && item <= '9')
                    normalPhoneNumber += item;
            }
            //Kiểm tra 2 ký tự đầu, nếu không phải là 84 thì xử lý
            if (!normalPhoneNumber.StartsWith("84"))
            {
                //Kiểm tra 1 ký tự đầu, nếu là ký tự 0 thì cắt đi
                if (normalPhoneNumber.StartsWith("0"))
                {
                    normalPhoneNumber = normalPhoneNumber.Substring(1);
                }
                //Thêm ký tự 84 vào trước
                normalPhoneNumber = "84" + normalPhoneNumber;
            }
            return normalPhoneNumber;
        }
        /// <summary>
        /// Chuyển một xâu tiếng Việt có dấu thành không dấu (Nguồn: internet)
        /// </summary>
        public static string ToVietnameseWithoutAccent(this string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\p{IsCombiningDiacriticalMarks}+");
                string strFormD = text.Normalize(System.Text.NormalizationForm.FormD);
                return regex.Replace(strFormD, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
            }
            return text;
        }

        #region Name To Tag
        public static string NameToTag(this string strName)
        {
            string strReturn = "";
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            strReturn = Regex.Replace(strName, "[^\\w\\s]", string.Empty).Replace(" ", "-").ToLower();
            string strFormD = strReturn.Normalize(System.Text.NormalizationForm.FormD);
            return regex.Replace(strFormD, string.Empty).Replace("đ", "d");
        }
        #endregion
        #region Name To Tag2
        public static string NameToTag2(this string strName)
        {
            string strReturn = "";
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            strReturn = Regex.Replace(strName, "[^\\w\\s]", string.Empty).Replace(" ", "_").ToLower();
            string strFormD = strReturn.Normalize(System.Text.NormalizationForm.FormD);
            return regex.Replace(strFormD, string.Empty).Replace("đ", "d");
        }
        #endregion
        #region Name To Tag3
        public static string NameToTag3(this string strName)
        {
            string strReturn = "";
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            strReturn = Regex.Replace(strName, "[^\\w\\s]", string.Empty).Replace(" ", " ").ToLower();
            string strFormD = strReturn.Normalize(System.Text.NormalizationForm.FormD);
            return regex.Replace(strFormD, string.Empty).Replace("đ", "d");
        }
        #endregion
        // thêm các dấu chấm vào các level cấp con
        public static string ShowNameLevel(this string Name, string Level)
        {
            // chỉ thêm dấu chấm vào cột level có length>5
            int strLevel = (Level.Length / 5);
            string strReturn = "";
            for (int i = 1; i < strLevel; i++)
            {
                strReturn = strReturn + "-----";
            }
            strReturn += Name;
            return strReturn;
        }
    }
}
