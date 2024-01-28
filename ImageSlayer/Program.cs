using System;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
namespace ImageSlayer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string serverName = "LAZARUS\\SQLEXPRESS";
            string databaseName = "Putalibazarmun";
            string connectionString = $"Server={serverName};Database={databaseName};Integrated Security=True;";
            string outputPath = @"E:\Sifaris\Putalibazar\Sifaris\MunicipalRecommendation\wwwroot\Files\";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {

                    connection.Open();
                    string query = "SELECT top(15) ScannedDocumentId, ScannedFile, ScannedFileName FROM ScannedDocument";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.CommandTimeout = 120;

                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            //connection.Open();
                            string scannedDocumentId = (string)reader["ScannedDocumentId"];
                            string base64Data = reader["ScannedFile"].ToString();
                            string  FileName = reader["ScannedFileName"].ToString();

                            //Check if ScannedFileName contains 'wwwroot'
                            if (base64Data!.Contains("wwwroot"))
                            {
                                Console.WriteLine($"Skipping row with ScannedDocumentId{scannedDocumentId} as it has already been converted");
                                continue; // Skip processing this row
                            }

                            // Generate new filename with GUID
                            string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(FileName);
                            string filePath = Path.Combine(outputPath, newFileName);

                            //Decode base64 data
                            byte[] imageBytes = Convert.FromBase64String(base64Data);

                            // Create image file
                            File.WriteAllBytes(filePath, imageBytes);

                            //Update ScannedFileName and ScannedFile columns in the database
                            //string scannedFile = "~/wwwroot/Files/" + newFileName;
                            string updatedFileName = newFileName;
                            
                            UpdateDatabase(connection, scannedDocumentId, updatedFileName);
                          
                        }
                    }
                    else
                    {
                        Console.WriteLine("No rows found in the ScannedDocument table.");
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An Error Occured:{ex.Message}");
            }

        }

        static void UpdateDatabase(SqlConnection connection, string scannedDocumentId, string updatedFileName)
        {
            //string updateQuery = $"UPDATE ScannedDocument SET ScannedFileName= '{updatedFileName}', ScannedFile= '~/wwwroot/Files/'+ScannedFileName Where  ScannedDocumentId={scannedDocumentId}";
            //SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
            //updateCommand.ExecuteNonQuery();
               
            string updateQuery = "UPDATE ScannedDocument SET ScannedFileName = @UpdatedFileName, ScannedFile = '~/wwwroot/Files/' + @UpdatedFileName WHERE ScannedDocumentId = @ScannedDocumentId";

            using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
            {
                updateCommand.CommandTimeout = 300;
                updateCommand.Parameters.AddWithValue("@UpdatedFileName", updatedFileName);
                updateCommand.Parameters.AddWithValue("@ScannedDocumentId", scannedDocumentId);
                updateCommand.ExecuteNonQuery();
            }
            
        }
    }
}
