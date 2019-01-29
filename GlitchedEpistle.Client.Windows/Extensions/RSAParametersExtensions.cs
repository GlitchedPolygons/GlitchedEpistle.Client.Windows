using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Extensions
{
    public static class RSAParametersExtensions
    {
        /// <summary>
        /// Deserializes an <see cref="RSAParameters"/> instance from an xml <c>string</c>.
        /// </summary>
        /// <param name="xml">The XML containing the <see cref="RSAParameters"/>.</param>
        /// <returns><see cref="RSAParameters"/>.</returns>
        /// <exception cref="InvalidDataException">Invalid XML RSA key.</exception>
        public static RSAParameters FromXml(string xml)
        {
            var rsaParameters = new RSAParameters();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            if (xmlDoc.DocumentElement != null && xmlDoc.DocumentElement.Name == "RSAKeyValue")
            {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Modulus": rsaParameters.Modulus = Convert.FromBase64String(node.InnerText); break;
                        case "Exponent": rsaParameters.Exponent = Convert.FromBase64String(node.InnerText); break;
                        case "P": rsaParameters.P = Convert.FromBase64String(node.InnerText); break;
                        case "Q": rsaParameters.Q = Convert.FromBase64String(node.InnerText); break;
                        case "DP": rsaParameters.DP = Convert.FromBase64String(node.InnerText); break;
                        case "DQ": rsaParameters.DQ = Convert.FromBase64String(node.InnerText); break;
                        case "InverseQ": rsaParameters.InverseQ = Convert.FromBase64String(node.InnerText); break;
                        case "D": rsaParameters.D = Convert.FromBase64String(node.InnerText); break;
                    }
                }
            }
            else
            {
                throw new InvalidDataException("Invalid XML RSA key.");
            }

            return rsaParameters;
        }
    }
}
