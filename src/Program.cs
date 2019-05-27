using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using CommandLine;

namespace ShareGate.CertificateTool
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<AddOptions, RemoveOptions>(args)
                .WithParsed<AddOptions>(opts => InstallCertificate(opts.CertificatePath, opts.CertificateBase64, opts.Password, opts.Thumbprint))
                .WithParsed<RemoveOptions>(opts => RemoveCertificate(opts.CertificatePath, opts.CertificateBase64, opts.Password, opts.Thumbprint))
                .WithNotParsed(errs => Console.WriteLine($"Error parsing\n {string.Join('\n', errs)}"));
        }

        private static void RemoveCertificate(string path, string base64, string password, string thumbprint)
        {
            Console.WriteLine("Removing certificate...");
            
            X509Certificate2 cert = null;
            if (!string.IsNullOrEmpty(path))
            {
                cert = new X509Certificate2(
                    path,
                    password,
                    X509KeyStorageFlags.DefaultKeySet);
            }
            else if (!string.IsNullOrEmpty(base64))
            {
                var bytes = Convert.FromBase64String(base64);
                cert = new X509Certificate2(
                    bytes,
                    password,
                    X509KeyStorageFlags.DefaultKeySet);
            }

            if (cert == null)
            {
                throw new ArgumentNullException("Unable to create certificate from provided arguments.");
            }

            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Remove(cert);
            
            var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            if (certificates.Count > 0)
            {
                throw new ArgumentNullException("Unable to validate certificate was removed from store.");
            }

            Console.WriteLine("Done.");

            store.Close();
        }

        private static void InstallCertificate(string path, string base64, string password, string thumbprint)
        {
            X509Certificate2 cert = null;
            if (!string.IsNullOrEmpty(path))
            {
                Console.WriteLine($"Installing certificate from '{path}'...");
                
                cert = new X509Certificate2(
                    path,
                    password,
                    X509KeyStorageFlags.DefaultKeySet);
            }
            else if (!string.IsNullOrEmpty(base64))
            {
                Console.WriteLine($"Installing certificate from base 64 string...");
                
                var bytes = Convert.FromBase64String(base64);
                cert = new X509Certificate2(
                    bytes,
                    password,
                    X509KeyStorageFlags.DefaultKeySet);
            }
            
            if (cert == null)
            {
                throw new ArgumentNullException("Unable to create certificate from provided arguments.");
            }

            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            
            var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            if (certificates.Count <= 0)
            {
                throw new ArgumentNullException("Unable to validate certificate was added to store.");
            }

            Console.WriteLine("Done.");
            store.Close();
        }
    }

    [Verb("add", HelpText = "Installs a pfx certificate to personal certificate of the current user.")]
    internal class AddOptions
    {
        [Option(shortName: 'f', longName: "file")]
        public string CertificatePath { get; set; }
        
        [Option(shortName: 'b', longName: "base64")]
        public string CertificateBase64 { get; set; }

        [Option(shortName: 'p', longName: "password", Required = true)]
        public string Password { get; set; }
        
        [Option(shortName: 't', longName: "thumbprint", Required = true)]
        public string Thumbprint { get; set; }
    }
    
    [Verb("remove", HelpText = "Removes a pfx certificate from the personal certificate of the current user.")]
    internal class RemoveOptions
    {
        [Option(shortName: 'f', longName: "file")]
        public string CertificatePath { get; set; }
        
        [Option(shortName: 'b', longName: "base64")]
        public string CertificateBase64 { get; set; }

        [Option(shortName: 'p', longName: "password", Required = true)]
        public string Password { get; set; }
        
        [Option(shortName: 't', longName: "thumbprint", Required = true)]
        public string Thumbprint { get; set; }
    }
}