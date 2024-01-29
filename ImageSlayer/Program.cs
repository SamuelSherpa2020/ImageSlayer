using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace ImageConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            string serverName = "DORJE";
            //string serverName = "LAZARUS\\SQLEXPRESS";
            string databaseName = "Putalibazarmun";
            string connString = $"Server={serverName};Database={databaseName};Integrated Security=True;";
            string outputPath = @"C:\Users\someo\OneDrive\PREV FILES\Documents\5-25-2023(Sifaris)";
            //string outputPath = @"E:\Sifaris\Putalibazar\Sifaris\MunicipalRecommendation\wwwroot\Files\";

            DataTable dt = new();

            try
            {
                using SqlConnection conn = new SqlConnection(connString);
                conn.Open();

                string query = "SELECT ScannedDocumentId, ScannedFile, ScannedFileName FROM ScannedDocument";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                foreach (DataRow row in dt.Rows)
                {
                    string scannedDocId = row["ScannedDocumentId"].ToString();
                    string base64Data = row["ScannedFile"].ToString();
                    string fileName = row["ScannedFileName"].ToString();

                    if (base64Data.Contains("wwwroot"))
                    {
                        // Already converted
                        continue;
                    }

                    // Convert and save image
                    string newFileName = Guid.NewGuid() + Path.GetExtension(fileName);
                    string filePath = Path.Combine(outputPath, newFileName);

                    byte[] imageBytes = Convert.FromBase64String(base64Data);
                    File.WriteAllBytes(filePath, imageBytes);

                    // Execute update query
                    string updateQuery = "UPDATE ScannedDocument SET ScannedFileName = @NewFileName, ScannedFile = '~/wwwroot/Files/' + @NewFileName WHERE ScannedDocumentId = @DocId";

                    using SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                    updateCmd.Parameters.Add("@NewFileName", SqlDbType.VarChar).Value = newFileName;
                    updateCmd.Parameters.Add("@DocId", SqlDbType.VarChar).Value = scannedDocId;
                    updateCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
