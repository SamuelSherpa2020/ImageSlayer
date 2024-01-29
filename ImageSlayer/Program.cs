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

            int batchSize = 100; // Set your desired batch size
            //DataTable dt = new();
            int pageNumber = 1; // Set the initial page number

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    while (true)
                    {
                        string query = $@"
                           SELECT 
                                ROW_NUMBER() As RowNum
                                ScannedDocumentId, 
                                ScannedFile, 
                                ScannedFileName
                            FROM OrderedResults
                            WHERE RowNum BETWEEN {batchSize * (pageNumber - 1) + 1} AND {batchSize * pageNumber}";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                DataTable dt = new DataTable();

                                while (reader.Read())
                                {
                                    DataRow row = dt.NewRow();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        row[reader.GetName(i)] = reader[i];
                                    }
                                    dt.Rows.Add(row);

                                    if (dt.Rows.Count == batchSize)
                                    {
                                        ProcessBatch(dt, outputPath, conn);
                                        dt.Clear();
                                    }
                                }

                                // Process the remaining rows
                                if (dt.Rows.Count > 0)
                                {
                                    ProcessBatch(dt, outputPath, conn);
                                }
                            }
                        }

                        // Check if there are more batches to process
                        if (pageNumber * batchSize >= 100) // Replace TotalRowCount with the total number of rows based on your condition
                            break;

                        pageNumber++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static void ProcessBatch(DataTable batch, string outputPath, SqlConnection connection)
        {
            foreach (DataRow row in batch.Rows)
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

                using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                {
                    updateCmd.Parameters.Add("@NewFileName", SqlDbType.VarChar).Value = newFileName;
                    updateCmd.Parameters.Add("@DocId", SqlDbType.VarChar).Value = scannedDocId;

                    updateCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
