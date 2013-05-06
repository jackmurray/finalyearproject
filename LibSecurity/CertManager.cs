﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibConfig;
using LibTrace;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1.X509;

namespace LibSecurity
{
    public class CertManager
    {
        public X509Certificate Cert { get; set; }
        private static Trace Log = new Trace("LibSecurity");

        /// <summary>
        /// Gets the fingerprint of this cert. Defined as Hex(SHA1(DEREncodedCert)).
        /// </summary>
        public string Fingerprint
        {
            get
            {
                byte[] der = Cert.GetEncoded();
                return Hasher.HashHex(new Sha1Digest(), der);
            }
        }

        public string Subject
        {
            //Gotta be careful here that if we ever change the cert subject we'll break this.
            get { return Cert.SubjectDN.GetValueList()[0].ToString(); }
        }

        private CertManager(X509Certificate cert)
        {
            Cert = cert;
        }

        public static CertManager Create(KeyManager key)
        {
            X509V3CertificateGenerator gen = new X509V3CertificateGenerator();
            BigInteger serial = BigInteger.ValueOf(Math.Abs(new SecureRandom().NextLong())); //securerandom can return negative numbers
            Guid certGuid = Guid.NewGuid();
            gen.SetSerialNumber(serial);
            gen.SetSubjectDN(new Org.BouncyCastle.Asn1.X509.X509Name("CN=" + certGuid));
            gen.SetIssuerDN(new Org.BouncyCastle.Asn1.X509.X509Name("CN=" + certGuid)); //Same issuer and subject.
            gen.SetNotBefore(new DateTime(2013, 1, 1, 0, 0, 0));
            gen.SetNotAfter(new DateTime(2050, 1, 1, 0, 0, 0)); //valid for basically ever
            gen.SetPublicKey(key.Public);
            gen.SetSignatureAlgorithm("SHA256WithRSAEncryption"); //hardcoded for now, maybe change it later.
            gen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment));
            return new CertManager(gen.Generate(key.Private));
        }

        public static CertManager GetCert(KeyManager key)
        {
            string certFile = System.IO.Path.Combine(Config.Get(Config.CRYPTO_PATH), "cert.crt");
            CertManager cert = null;

            if (System.IO.File.Exists(certFile))
            {
                Log.Information("Not generating cert as we already have one.");
                try
                {
                    cert = CertManager.LoadFromFile(certFile);
                }
                catch (Exception ex)
                {
                    Log.Critical("Failed to load cert: " + ex.Message);
                }
            }
            else
            {
                try
                {
                    Log.Information("Generating cert.");
                    cert = CertManager.Create(key);
                    cert.WriteToFile(certFile);
                    Log.Information("Cert generated.");
                }
                catch (Exception ex)
                {
                    Log.Critical("Failed to generate cert: " + ex.Message);
                    throw;
                }
            }
            return cert;
        }

        public void WriteToFile(string filename)
        {
            PemWriter w = new PemWriter(new StreamWriter(filename));
            w.WriteObject(Cert);
            w.Writer.Close();
        }

        public static CertManager LoadFromFile(string filename)
        {
            PemReader r = new PemReader(new StreamReader(filename));
            return new CertManager((X509Certificate)r.ReadObject());
        }

        public System.Security.Cryptography.X509Certificates.X509Certificate2 ToDotNetCert(KeyManager key)
        {
            var dotnetcert = new System.Security.Cryptography.X509Certificates.X509Certificate2(DotNetUtilities.ToX509Certificate(Cert));
            dotnetcert.PrivateKey = key.ToDotNetKey();
            return dotnetcert;
        }
    }
}