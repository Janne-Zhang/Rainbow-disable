using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Rainbow.Web.Configuration.Xml
{
    internal class XmlSchemas
    {
        private const string configuration_resource = "Rainbow.Web.Schemas.rainbow.xsd";

        public static readonly XmlSchema configuration_schema = ReadXmlSchemaFromEmbeddedResource(configuration_resource);

        private static XmlSchema ReadXmlSchemaFromEmbeddedResource(string resourceName)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();

            using (Stream resourceStream = executingAssembly.GetManifestResourceStream(resourceName))
            {
                var xmlSchema = XmlSchema.Read(resourceStream, null);
                return xmlSchema;
            }
        }

        public static XmlReaderSettings CreateConfigurationReaderSettings()
        {
            return CreateXmlReaderSettings(configuration_schema);
        }

        public static XmlReaderSettings CreateXmlReaderSettings(XmlSchema xmlSchema)
        {
            XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
            xmlSchemaSet.Add(xmlSchema);
            xmlSchemaSet.Compile();

            XmlReaderSettings settings = new XmlReaderSettings { ValidationType = ValidationType.Schema, Schemas = xmlSchemaSet };
            settings.ValidationEventHandler += (sender, e) =>
            {
                throw new RainbowException($"An exception occurred parsing xml :{e.Message}", e.Exception);
            };
            settings.IgnoreComments = true;

            return settings;
        }
    }
}
