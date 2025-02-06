using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using Renci.SshNet;

class Program
{
    static async Task Main(string[] args)
    {
        // Set up the download directory
        string downloadDirectory = "downloads";
        if (!Directory.Exists(downloadDirectory))
        {
            Directory.CreateDirectory(downloadDirectory);
        }

        //Set up appsettings Configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string username = configuration["Authentication:Username"];
        string password = configuration["Authentication:Password"];
        string proxyUsername = configuration["Proxy:Username"];
        string proxyPassword = configuration["Proxy:Password"];
        string proxyHost = configuration["Proxy:Host"];
        int proxyPort = int.Parse(configuration["Proxy:Port"]);
        string sftpHost = configuration["SFTP:Host"];
        string sftpUsername = configuration["SFTP:Username"];
        string sftpPassword = configuration["SFTP:Password"] ?? "";  // Default password empty
        int sftpPort = int.Parse(configuration["SFTP:Port"]);
        string s3Path = configuration["SFTP:PEMFilePath"];


        // Instantiate and run the job immediately for test
        var downloadFileJob = new DownloadFileJob(username, password, proxyUsername, proxyPassword, proxyHost, proxyPort, sftpHost, sftpUsername, sftpPassword, sftpPort, s3Path);
        await downloadFileJob.Execute(null);  // Pass null for the job context (not needed for this case)


        // Initialize Quartz.NET scheduler
        //var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        //await scheduler.Start();

        //// Define the job and tie it to the DownloadFileJob class
        //IJobDetail job = JobBuilder.Create<DownloadFileJob>()
        //    .WithIdentity("fileDownloadJob", "group1")
        //    .Build();

        //// Trigger the job to run every day at 12:00 AM (midnight)
        //ITrigger trigger = TriggerBuilder.Create()
        //    .WithIdentity("dailyTrigger", "group1")
        //    .StartNow()
        //    .WithCronSchedule("0 0 9 * * ?") // Cron expression Run at 9:00 AM every day.
        //    .Build();

        //// Schedule the job
        //await scheduler.ScheduleJob(job, trigger);

        // Keep the application running
        Console.WriteLine("Press [Enter] to exit.");
        Console.ReadLine();

        // Shut down the scheduler when done
        //await scheduler.Shutdown();
    }
}

// Job class to download the file
public class DownloadFileJob : IJob
{
    private readonly string _username;
    private readonly string _password;
    private readonly string _proxyUsername;
    private readonly string _proxyPassword;
    private readonly string _proxyHost;
    private readonly int _proxyPort;
    private readonly string _sftpHost;
    private readonly string _sftpUsername;
    private readonly string? _sftpPassword;
    private readonly int _sftpPort; 
    private readonly string _s3Path; // S3 path on the SFTP server (e.g., /s3-msaml-p01)

    public DownloadFileJob(string username, string password, string proxyUsername, string proxyPassword, string proxyHost, int proxyPort, string sftpHost, string sftpUsername, string sftpPassword, int sftpPort, string s3Path)
    {
        _username = username;
        _password = password;
        _proxyUsername = proxyUsername;
        _proxyPassword = proxyPassword;
        _proxyHost = proxyHost;
        _proxyPort = proxyPort;
        _sftpHost = sftpHost;
        _sftpUsername = sftpUsername;
        _sftpPassword = sftpPassword;
        _sftpPort = sftpPort;
        _s3Path = s3Path; // The path to upload files to, e.g., /s3-msaml-p01

    }
    public async Task Execute(IJobExecutionContext context)
    {
        // Set the URL for the CSV file to download
        string baseUrl = "https://djrcfeed.dowjones.com/csv/";         // Set the base URL and construct the dynamic filename
        //string currentDate = DateTime.Now.ToString("yyyyMMdd");  // Get current date in yyyymmdd format
        string currentDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");  // Get current date in yyyymmdd format
        string fileName = $"csv_pfa_{currentDate}2200_d.zip";  // Construct the filename
        string url = baseUrl + fileName;  // Full URL
        string downloadDirectory = "downloads";
        string filePath = Path.Combine(downloadDirectory, fileName);

        string username = _username;
        string password = _password;
        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

        //// Create an HttpClientHandler with proxy settings
        //var proxy = new WebProxy(_proxyHost, _proxyPort)
        //{
        //    Credentials = new NetworkCredential(_proxyUsername, _proxyPassword)
        //}; 

        //HttpClientHandler handler = new HttpClientHandler()
        //{
        //    Proxy = proxy,
        //    UseProxy = true,
        //    PreAuthenticate = true // Ensure pre-authentication
        //};

        // Use HttpClient to download the file
        //using (var client = new HttpClient(handler))
        using (var client = new HttpClient())
        {
            try
            {
                //Basic Auth for Login
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                Console.WriteLine($"url: {url}");

                // Download the file asynchronously
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Throw an exception if the status code is not successful

                // Read the file content and save it
                byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(filePath, fileBytes);

                Console.WriteLine($"File downloaded successfully: {filePath}");

                await UploadFileToSFTP(filePath);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading the file: {ex.Message}");
            }
        }
    }
    private async Task UploadFileToSFTP(string filePath)
    {
        try
        {
            // Use PEM file for SSH authentication
            string pemf = "amluser.pem";
            string pmfilePath = Path.Combine("pem/", pemf);
            // Ensure that the PEM file exists
            if (!File.Exists(pmfilePath))
            {
                Console.WriteLine("Error: PEM file does not exist.");
                return;
            }

            var privateKeyFile = new PrivateKeyFile(pmfilePath); // Path to PEM file

            // SSH connection info
            var connectionInfo = new ConnectionInfo(
                _sftpHost,
                _sftpUsername,
                new AuthenticationMethod[]
                {
                    new PrivateKeyAuthenticationMethod(_sftpUsername, privateKeyFile)
                }
            );

            // Establish an SFTP connection
            using (var sftpClient = new SftpClient(connectionInfo))
            {
                sftpClient.Connect();
                Console.WriteLine("Connected to SFTP server");

                string remoteFilePath = _s3Path + "/" + Path.GetFileName(filePath);
                Console.WriteLine($"Uploading to remote path: {remoteFilePath}");

                // Upload the file to the SFTP server
                using (var fileStream = new FileStream(filePath, FileMode.Open))
                {   
                    await Task.Run(() => sftpClient.UploadFile(fileStream, remoteFilePath));
                    Console.WriteLine($"File uploaded successfully to SFTP: {remoteFilePath}");
                }

                sftpClient.Disconnect();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading the file to SFTP: {ex.Message}");
        }
    }
}
