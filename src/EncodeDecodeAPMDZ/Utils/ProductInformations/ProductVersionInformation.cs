using System;

namespace EncodeDecodeAPMDZ.Utils.ProductInformations
{
    internal sealed class ProductVersionInformation
    {
        public ProductVersionInformation(string productName, Uri productWebsite, Version version)
        {
            if (string.IsNullOrWhiteSpace(productName))
            { throw new ArgumentNullException(nameof(productName)); }
            if (version == null)
            { throw new ArgumentNullException(nameof(version)); }

            Name = productName;
            Version = version;
            Website = productWebsite;
        }

        //---------------------------------------------------------------------
        public string Name
        {
            get;
        }
        public Version Version
        {
            get;
        }
        public Uri Website
        {
            get;
        }
        //---------------------------------------------------------------------
    }
}