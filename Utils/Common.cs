using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utils
{
    public static class Common
    {
        /// <summary>
        /// Kiểm tra địa chỉ webservice có sống không
        /// </summary>
        public static bool WebSerivceIsAvailable(string url, ref string strEx)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            // Set the credentials to the current user account
            request.Credentials = System.Net.CredentialCache.DefaultCredentials;
            request.Method = "GET";
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        strEx = string.Empty;
                        return true;
                    }
                    else
                    {
                        strEx = string.Format("{0} has die. Status Code: {1}, Status Description: {2}", url, response.StatusCode, response.StatusDescription);
                        return false;
                    }

                }
            }
            catch (Exception ex)
            {
                strEx = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra database có sống không
        /// </summary>
        public static ConnectionState IsDatabaseAlive(string connectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return connection.State;
                }
            }
            catch
            {
                return ConnectionState.Closed;
            }
        }

        public static bool RegExpressionValidation(string pattern, string str)
        {
            return RegExpressionValidation(pattern, str, false);
        }

        public static bool RegExpressionValidation(string pattern, string str, bool ignoreCase)
        {
            if (ignoreCase)
                return Regex.IsMatch(str, pattern, RegexOptions.IgnoreCase);
            return Regex.IsMatch(str, pattern);
        }

        /// <summary>
        /// Tìm đường dẫn của 1 file khi chỉ biết tên file và thư mục tìm kiếm
        /// </summary>
        /// <param name="fileNameWithExtention">Tên file bao gồm cả phần mở rộng</param>
        /// <param name="lookingFolderPath">Thư mục tìm kiếm</param>
        /// <returns></returns>
        public static string GetFilePath(string fileNameWithExtention, string lookingFolderPath)
        {
            string[] filePaths = Directory.GetFiles(lookingFolderPath);
            foreach (string filePath in filePaths)
            {
                if (filePath.EndsWith(fileNameWithExtention, StringComparison.CurrentCultureIgnoreCase))
                {
                    return filePath;
                }
            }

            string[] subFolderPaths = Directory.GetDirectories(lookingFolderPath);
            foreach (string subFolderPath in subFolderPaths)
            {
                string subFilePath = GetFilePath(fileNameWithExtention, subFolderPath);
                if (subFilePath != "") return subFilePath;
            }

            return "";
        }

        public static string ConvertToNULLString(object obj)
        {
            if (obj != null)
            {
                if (obj is Guid || obj is Guid? || obj is bool || obj is bool?)
                    return "'" + obj.ToString() + "'";
                else return obj.ToString();
            }
            return "NULL";
        }

        public static string ConvertToNULLString(string str)
        {
            if (str != null)
            {
                return "'" + str.Replace("'", "''") + "'";
            }
            return "NULL";
        }

        public static string ConvertToNULLString(DateTime? dateTime)
        {
            if (dateTime != null) return "'" + dateTime.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
            return "NULL";
        }
    }

    #region Enum
    public enum QuestionResult
    {
        Yes = 0,
        No = 1,
        Cancel = 2,
        YesAll = 3,
        NoAll = 4
    }
    #endregion
}
