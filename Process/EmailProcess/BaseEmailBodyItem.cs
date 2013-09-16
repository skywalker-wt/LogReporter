using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using LogReporter.Rule;
using System.Net.Mail;

namespace LogReporter.Process.EmailProcess
{
    public abstract class BaseEmailBodyItem
    {
        public const string ATTRIBUTE_TYPE = "type";
        public const string ATTRIBUTE_PRIORITY = "priority";

        #region Generators
        public static List<IItemGenerator> Generators = new List<IItemGenerator> { new DefaultItemGenerators(), new ReflectionItemGenerator() };
        public static BaseEmailBodyItem Create(XmlNode config)
        {
            foreach (var generator in Generators)
            {
                try
                {
                    var item = generator.CreateBaseEmailBodyItem(config);
                    if (item != null)
                        return item;
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
            }
            return null;
        }
        #endregion

        #region Constructors
        public BaseEmailBodyItem()
        {
            Attachments = new List<Attachment>();
            Content = "";
            Priority = 0;
        }

        public BaseEmailBodyItem(XmlNode config)
        {
            Attachments = new List<Attachment>();
            Content = "";
            foreach (XmlAttribute attr in config.Attributes)
            {
                if (attr.Name == ATTRIBUTE_PRIORITY)
                    Priority = int.Parse(attr.Value);
            }
        }
        #endregion

        #region Properties
        public int Priority { get; private set; }
        public string Content { get; protected set; }
        public List<Attachment> Attachments { get; protected set; }
        #endregion

        public abstract bool CreateContent(Dictionary<string, BaseRule> rules);
    }

    class DefaultItemGenerators : IItemGenerator
    {
        public const string ATTRIBUTE_TYPE = "type";
        public const string TYPE_GROUP = "group";
        public const string TYPE_ONE_ROW_GROUP = "one_row_group";
        public const string TYPE_HTML_TEXT = "htmltext";
        public const string TYPE_TABLE = "table";
        public const string TYPE_CLASSIFY_TABLE = "classifier";
        public const string TYPE_CLASSIFY_TEMPLATE = "template";
        public const string TYPE_CHART = "chart";
        public const string TYPE_IMAGE = "image";
        public const string TYPE_EXTEND_CHART = "xchart";

        public BaseEmailBodyItem CreateBaseEmailBodyItem(XmlNode config)
        {
            string type = TYPE_GROUP;

            XmlAttribute typeAtt = config.Attributes[ATTRIBUTE_TYPE];
            if (typeAtt != null && !string.IsNullOrWhiteSpace(typeAtt.Value))
                type = typeAtt.Value;

            switch (type)
            {
                case TYPE_GROUP:
                    return new GroupItem(config);
                case TYPE_ONE_ROW_GROUP:
                    return new OneRowItem(config);
                case TYPE_HTML_TEXT:
                    return new HtmlTextItem(config);
                case TYPE_TABLE:
                    return new TableItem(config);
                case TYPE_CLASSIFY_TABLE:
                    return new ClassifyTableItem(config);
                case TYPE_CLASSIFY_TEMPLATE:
                    return new TemplateItem(config);
                case TYPE_CHART:
                    return new ChartItem(config);
                case TYPE_EXTEND_CHART:
                    return new ExtendChartItem(config);
                case TYPE_IMAGE:
                    return new ImageItem(config);
            }
            return null;
        }
    }

    public interface IItemGenerator
    {
        BaseEmailBodyItem CreateBaseEmailBodyItem(XmlNode config);
    }

}
