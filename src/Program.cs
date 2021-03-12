using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CommandLine;

namespace GSoft.CertificateTool
{
    public static class Program
    {
        private static X509KeyStorageFlags X509KeyStorageFlags
        {
            get
            {
                var keyStorageFlags = X509KeyStorageFlags.DefaultKeySet;

                // https://stackoverflow.com/questions/50340712/avoiding-the-keychain-when-using-x509certificate2-on-os-x
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    keyStorageFlags = X509KeyStorageFlags.Exportable;
                }

                return keyStorageFlags;
            }
        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<AddOptions, RemoveOptions, ListOptions>(args)
                .WithParsed<AddOptions>(
                    opts =>
                    {
                        if (!string.IsNullOrEmpty(opts.PfxPath))
                        {
                            InstallPfxCertificate(
                                opts.PfxPath,
                                opts.Password,
                                Enum.Parse<StoreName>(
                                    opts.StoreName,
                                    ignoreCase: true),
                                Enum.Parse<StoreLocation>(
                                    opts.StoreLocation,
                                    ignoreCase: true));
                        }
                        else if (!string.IsNullOrEmpty(opts.Base64))
                        {
                            InstallBase64Certificate(
                                opts.Base64,
                                opts.Password,
                                Enum.Parse<StoreName>(
                                    opts.StoreName,
                                    ignoreCase: true),
                                Enum.Parse<StoreLocation>(
                                    opts.StoreLocation,
                                    ignoreCase: true));
                        }
                        else if (!string.IsNullOrEmpty(opts.PublicCertPath))
                        {
                            InstallPemCertificate(
                                opts.PublicCertPath,
                                opts.PrivateKeyPath,
                                opts.Password,
                                Enum.Parse<StoreName>(
                                    opts.StoreName,
                                    ignoreCase: true),
                                Enum.Parse<StoreLocation>(
                                    opts.StoreLocation,
                                    ignoreCase: true));
                        }
                        else
                        {
                            throw new NotSupportedException("Arguments provided are invalid.");
                        }
                    })
                .WithParsed<RemoveOptions>(
                    opts => RemoveCertificate(
                        opts.Thumbprint,
                        Enum.Parse<StoreName>(
                            opts.StoreName,
                            ignoreCase: true),
                        Enum.Parse<StoreLocation>(
                            opts.StoreLocation,
                            ignoreCase: true)))
                .WithParsed<ListOptions>(
                    opts => ListCertificates(
                        Enum.Parse<StoreName>(
                            opts.StoreName,
                            ignoreCase: true),
                        Enum.Parse<StoreLocation>(
                            opts.StoreLocation,
                            ignoreCase: true)))
                .WithNotParsed(
                    errs =>
                        Console.WriteLine(
                            $"Error parsing\n {string.Join('\n', errs)}"));
        }

        private static void ListCertificates(StoreName storeName, StoreLocation storeLocation)
        {
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            if (store.Certificates.Count > 0)
            {
                Console.WriteLine($"Certificates stored in '{storeName}' certificate store (location: {storeLocation}):");
                Console.WriteLine();

                var counter = 0;
                foreach (var certificate in store.Certificates)
                {
                    counter++;
                    Console.WriteLine($"#{counter}:");

                    var certificateInfo = new Dictionary<string, string>
                    {
                        { "Subject", certificate.Subject },
                        { "Issuer", certificate.Issuer },
                        { "Serial Number", certificate.GetSerialNumberString() },
                        { "Not Before", certificate.GetEffectiveDateString() },
                        { "Not After", certificate.GetExpirationDateString() },
                        { "Thumbprint", certificate.Thumbprint },
                        { "Signature Algorithm", $"{certificate.SignatureAlgorithm.FriendlyName} ({certificate.SignatureAlgorithm.Value})" },
                        { "PublicKey Algorithm", $"{certificate.PublicKey.Oid.FriendlyName} ({certificate.PublicKey.Oid.Value})" },
                        { "Has PrivateKey", certificate.HasPrivateKey ? "Yes" : "No" }
                    };

                    foreach (var info in certificateInfo)
                    {
                        Console.WriteLine($"  {info.Key,-20}: {info.Value}");
                    }

                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine($"No certificates found in '{storeName}' certificate store (location: {storeLocation}).");
            }

            store.Close();
        }

        private static void RemoveCertificate(string thumbprint, StoreName storeName, StoreLocation storeLocation)
        {
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);

            Console.WriteLine($"Removing certificate '{thumbprint}' from '{storeName}' certificate store (location: {storeLocation})...");

            var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            if (certificates.Count == 0)
            {
                throw new ArgumentNullException($"Unable to find certificate '{thumbprint}' in certificate store.");
            }

            store.RemoveRange(certificates);
            Console.WriteLine("Done.");

            store.Close();
        }

        private static void InstallPemCertificate(string certificatePath, string privateKeyPath, string password, StoreName storeName, StoreLocation storeLocation)
        {
            if (!string.IsNullOrEmpty(certificatePath))
            {
                Console.WriteLine($"Installing certificate from '{certificatePath}' to '{storeName}' certificate store (location: {storeLocation})...");

                using var publicKey =  string.IsNullOrEmpty(password)
                    ? new X509Certificate2(certificatePath)
                    : new X509Certificate2(
                        certificatePath,
                        password,
                        X509KeyStorageFlags);

                X509Certificate2 keyPair = null;
                if (!string.IsNullOrEmpty(privateKeyPath))
                {
                    var privateKeyText = File.ReadAllText(privateKeyPath);
                    var privateKeyBlocks = privateKeyText.Split("-", StringSplitOptions.RemoveEmptyEntries);
                    var privateKeyBytes = Convert.FromBase64String(privateKeyBlocks[1]);
                    using var rsa = RSA.Create();

                    switch (privateKeyBlocks[0])
                    {
                        case "BEGIN PRIVATE KEY":
                            rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
                            break;
                        case "BEGIN ENCRYPTED PRIVATE KEY":
                            rsa.ImportEncryptedPkcs8PrivateKey(Encoding.ASCII.GetBytes(password), privateKeyBytes, out _);
                            break;
                        case "BEGIN RSA PRIVATE KEY":
                            rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
                            break;
                    }

                    keyPair = publicKey.CopyWithPrivateKey(rsa);
                }
                var cert = keyPair == null ? publicKey : new X509Certificate2(keyPair.Export(X509ContentType.Pfx, password), password, X509KeyStorageFlags);

                AddToStore(cert, storeName, storeLocation);
            }
        }

        private static void InstallPfxCertificate(string path, string password, StoreName storeName, StoreLocation storeLocation)
        {
            X509Certificate2 cert = null;
            if (!string.IsNullOrEmpty(path))
            {
                Console.WriteLine($"Installing certificate from '{path}' to '{storeName}' certificate store (location: {storeLocation})...");

                cert = string.IsNullOrEmpty(password)
                    ? new X509Certificate2(path)
                    : new X509Certificate2(
                        path,
                        password,
                        X509KeyStorageFlags);
            }

            if (cert == null)
            {
                throw new ArgumentNullException("Unable to create certificate from provided arguments.");
            }

            AddToStore(cert, storeName, storeLocation);
        }

        private static void InstallBase64Certificate(string base64, string password, StoreName storeName, StoreLocation storeLocation)
        {
            X509Certificate2 cert = null;
            if (!string.IsNullOrEmpty(base64))
            {
                Console.WriteLine($"Installing certificate from base 64 string to '{storeName}' certificate store (location: {storeLocation})...");

                var bytes = Convert.FromBase64String(base64);
                cert = string.IsNullOrEmpty(password)
                    ? new X509Certificate2(bytes)
                    : new X509Certificate2(
                        bytes,
                        password,
                        X509KeyStorageFlags);
            }

            if (cert == null)
            {
                throw new ArgumentNullException("Unable to create certificate from provided arguments.");
            }

            AddToStore(cert, storeName, storeLocation);
        }

        private static void AddToStore(X509Certificate2 cert, StoreName storeName, StoreLocation storeLocation)
        {
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);

            var thumbprint = cert.Thumbprint ?? string.Empty;

            var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            if (certificates.Count <= 0)
            {
                throw new ArgumentNullException("Unable to validate certificate was added to store.");
            }

            Console.WriteLine("Done.");
            store.Close();
        }
    }

    [Verb("add", HelpText = "Installs a pfx certificate to selected store.")]
    internal sealed class AddOptions : BaseOptions
    {
        [Option(shortName: 'f', longName: "file")]
        public string PfxPath { get; set; }

        [Option(shortName: 'c', longName: "cert")]
        public string PublicCertPath { get; set; }

        [Option(shortName: 'k', longName: "key")]
        public string PrivateKeyPath { get; set; }

        [Option(shortName: 'b', longName: "base64")]
        public string Base64 { get; set; }

        [Option(shortName: 'p', longName: "password")]
        public string Password { get; set; }
    }

    [Verb("remove", HelpText = "Removes a pfx certificate from selected store.")]
    internal sealed class RemoveOptions : BaseOptions
    {
        [Option(shortName: 't', longName: "thumbprint", Required = true)]
        public string Thumbprint { get; set; }
    }

    [Verb("list", HelpText = "List all certificates in selected store.")]
    internal sealed class ListOptions : BaseOptions { }

    internal abstract class BaseOptions
    {
        [Option(shortName: 's', longName: "store-name", Default = "My", HelpText = "Certificate store name (My, Root, etc.). See 'System.Security.Cryptography.X509Certificates.StoreName' for more information.")]
        public string StoreName { get; set; }

        [Option(shortName: 'l', longName: "store-location", Default = "CurrentUser", HelpText = "Certificate store location (CurrentUser, LocalMachine, etc.). See 'System.Security.Cryptography.X509Certificates.StoreLocation' for more information.")]
        public string StoreLocation { get; set; }
    }
}