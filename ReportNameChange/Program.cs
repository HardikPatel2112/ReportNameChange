using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;



var builder = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
IConfigurationRoot config = builder.Build();
// load the configuration file.
var configBuilder = new ConfigurationBuilder().
   AddJsonFile("appsettings.json").Build();
// get the section to read
var configSection = configBuilder.GetSection("AppSettings");

//sql ser
string connectionString = configSection["ConnectionString"];




string sourceFolder = configSection["SourceFolder"];
string targetFolder = configSection["TargetFolder"];


List<string> QueryForUpdate = new List<string>();

using (SqlConnection connection = new SqlConnection(connectionString))
{
    connection.Open();
    DataTable dt = new DataTable();
    var c = "select * from [dbo].[ReportDefConfiguration]";
    using (SqlCommand command = new SqlCommand(c, connection))
    {
        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
        {
            adapter.Fill(dt);

            foreach (DataRow dr in dt.Rows)
            {

                var reportName = Convert.ToString(dr[dt.Columns[0]]);
                var reportFileName = Convert.ToString(dr[dt.Columns[1]]);

                QueryForUpdate.Add($" \r\n UPDATE [dbo].[ReportDefConfiguration] \r\n SET templatename = '{reportName}.pdf'   \r\nWHERE reportid ='{reportName}';");

                var Oldfileinfo = new FileInfo(sourceFolder + reportFileName);

                var newFile = new FileInfo(targetFolder + reportName + ".pdf");

                if (!newFile.Exists)
                {
                    newFile.Directory.Create();                    
                }

                try
                {
                    Oldfileinfo.CopyTo(newFile.FullName, true);
                }
                catch (Exception)
                {
                    Console.WriteLine($"file not exist. {Oldfileinfo}");

                }
              

            }

        }
        connection.Close();
    }

    foreach (var item in QueryForUpdate)
    {
        using (SqlCommand cmd = new SqlCommand(item, connection))
        {
           
            connection.Open();
            cmd.ExecuteNonQuery();
            connection.Close();
        }

    }
   
      

}

Console.WriteLine("Press any key to exit.");
Console.ReadLine();



