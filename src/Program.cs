using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
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
            Console.WriteLine(string.Join(',', args));

            Parser.Default.ParseArguments<AddOptions, RemoveOptions>(args)
                .WithParsed<AddOptions>(
                    opts => InstallCertificate(
                        opts.CertificatePath,
                        opts.CertificateBase64,
                        opts.Password,
                        opts.Thumbprint,
                        Enum.Parse<StoreName>(
                            opts.StoreName,
                            ignoreCase: true)))
                .WithParsed<RemoveOptions>(
                    opts => RemoveCertificate(
                        opts.CertificatePath,
                        opts.CertificateBase64,
                        opts.Password,
                        opts.Thumbprint,
                        Enum.Parse<StoreName>(
                            opts.StoreName,
                            ignoreCase: true)))
                .WithNotParsed(
                    errs =>
                        Console.WriteLine(
                            $"Error parsing\n {string.Join('\n', errs)}"));
        }

        private static void RemoveCertificate(string path, string base64, string password, string thumbprint, StoreName storeName)
        {
            Console.WriteLine($"Removing certificate from '{path}' from current user's '{storeName}' certificate store...");
            
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
                Console.WriteLine($"Removing certificate from base 64 string from current user's '{storeName}' certificate store...");

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

            var store = new X509Store(storeName, StoreLocation.CurrentUser);
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

        private static void InstallCertificate(string path, string base64, string password, string thumbprint, StoreName storeName)
        {
            X509Certificate2 cert = null;
            if (!string.IsNullOrEmpty(path))
            {
                Console.WriteLine($"Installing certificate from '{path}' to current user's '{storeName}' certificate store...");
                
                cert = new X509Certificate2(
                    path,
                    password,
                    X509KeyStorageFlags.DefaultKeySet);
            }
            else if (!string.IsNullOrEmpty(base64))
            {
                Console.WriteLine($"Installing certificate from base 64 string to current user's '{storeName}' certificate store...");
                
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

            var store = new X509Store(storeName, StoreLocation.CurrentUser);
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
    internal sealed class AddOptions : Options { }
    
    [Verb("remove", HelpText = "Removes a pfx certificate from the personal certificate of the current user.")]
    internal sealed class RemoveOptions : Options { }

    internal abstract class Options
    {
        [Option(shortName: 'f', longName: "file")]
        public string CertificatePath { get; set; }

        [Option(shortName: 'b', longName: "base64")]
        public string CertificateBase64 { get; set; }

        [Option(shortName: 'p', longName: "password", Required = true)]
        public string Password { get; set; }

        [Option(shortName: 't', longName: "thumbprint", Required = true)]
        public string Thumbprint { get; set; }

        [Option(shortName: 's', longName: "store-name", Default = "My", HelpText = "Certificate store name (My, Root, etc.). See 'System.Security.Cryptography.X509Certificates.StoreName' for more information.")]
        public string StoreName { get; set; }
    }
}