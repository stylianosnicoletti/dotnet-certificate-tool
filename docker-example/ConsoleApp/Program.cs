using System;
using System.Security.Cryptography.X509Certificates;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            Console.WriteLine($"The following {store.Certificates.Count} certificate(s) are installed:");
            foreach (var certificate in store.Certificates)
            {
                Console.WriteLine($"\t- Subject Name: '{certificate.SubjectName.Name}'");
            }

            store.Close();
        }
    }
}