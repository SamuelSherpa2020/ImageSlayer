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

            //string serverName = "LAZARUS\\SQLEXPRESS";
            string serverName = "103.94.159.2";
            string databaseName = "Putalibazarmun";
            string connString = $"Server={serverName};Database={databaseName};user id=sa;password=Silicon321;Integrated Security=false;";
            //string outputPath = @"C:\Users\someo\OneDrive\PREV FILES\Documents\5-25-2023(Sifaris)";
            //string outputPath = @"E:\Sifaris\Putalibazar\Sifaris\MunicipalRecommendation\wwwroot\FilesDebug\"; //permission check
            string outputPath = @"C:\TrialImages\"; //permission check

            int batchSize = 100; // Set your desired batch size
            //DataTable dt = new();
            //int pageNumber = 1; // Set the initial page number

            // Initialize a variable to store the total number of pages
            int totalPages = 0;

            try
            {
                using SqlConnection conn = new SqlConnection(connString);

                conn.Open();
                Console.WriteLine("Started");

               

                // Calculate the total number of pages
                string countQuery = "SELECT COUNT(*) FROM ScannedDocument";
                using (SqlCommand countCmd = new SqlCommand(countQuery, conn))
                {
                    int totalRows = (int)countCmd.ExecuteScalar();
                    totalPages = (int)Math.Ceiling((double)totalRows / batchSize);
                }

                for (int pageNumber = 1; pageNumber <= totalPages; pageNumber++)
                {
                    Console.WriteLine(pageNumber);
                    var from = batchSize * (pageNumber - 1) + 1;
                    var to = batchSize * pageNumber;

                    string query = $@"
                        SELECT 
                            ScannedDocumentId, 
                            ScannedFile, 
                            ScannedFileName
                        FROM (
                            SELECT 
                                ScannedDocumentId, 
                                ScannedFile, 
                                ScannedFileName,
                                ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS RowNum
                            FROM ScannedDocument
                        ) AS OrderedResults
                        WHERE RowNum BETWEEN {from} AND {to}";


                    DataTable dt = new DataTable();
                    //dt.Columns.Add("ScannedDocumentId", typeof(string)); // Example: Adding ScannedDocumentId column of type int
                    //dt.Columns.Add("ScannedFile", typeof(string)); // Example: Adding ScannedFile column of type string
                    //dt.Columns.Add("ScannedFileName", typeof(string)); // Example: Adding ScannedFileName column of type string

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                    }
                    Console.WriteLine("data filled in datatable");

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

                        //// Check if there are more batches to process
                        //if (pageNumber > totalPages)
                        //    break;
                    }
                    Console.WriteLine($"data updated of pagenumber {pageNumber}");


                    dt.Clear();


                }
                Console.WriteLine("completed");

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
