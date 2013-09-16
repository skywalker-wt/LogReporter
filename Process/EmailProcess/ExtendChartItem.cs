using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using LogReporter.Rule;
using System.IO;

namespace LogReporter.Process.EmailProcess
{
    public class ExtendChartItem : BaseEmailBodyItem
    {
        internal const string ATTRIBUTE_TITLE = "title";
        internal const string ATTRIBUTE_RELATED_RULE = "rule";
        internal const string ATTRIBUTE_FIELD_NAME = "nameField";
        internal const string ATTRIBUTE_FIELD_VALUE = "valueField";
        internal const string ATTRIBUTE_LEGEND = "legend";
        internal const string ATTRIBUTE_TYPE_NAME = "nameType";
        internal const string ATTRIBUTE_TYPE_VALUE = "valueType";
        internal const string ATTRIBUTE_NAME = "seriesName";
        internal const string ATTRIBUTE_SOURCE = "src";
        internal const string TAG_SERIES = "Series";
        internal const string TAG_POINT = "Point";
        internal const string TYPE_DOUBLE = "Double";
        internal const string TYPE_INT = "Integer";
        internal const string TYPE_DATE = "DateTime";
        internal const string TYPE_PERCENT = "Percentage";

        public string RelatedRule { get; set; }
        public string Title { get; set; }
        public string Legend { get; set; }
        public string NameField { get; set; }
        public string ValueField { get; set; }
        public string NameType { get; set; }
        public string ValueType { get; set; }
        public string SeriesName { get; set; }
        public string Source { get; set; }
        public List<KeyValuePair<string, string>> PointAttrs { get; private set; }
        public List<KeyValuePair<string, string>> HtmlAttrs { get; private set; }

        public ExtendChartItem(XmlNode config) : base(config)
        {
            HtmlAttrs = new List<KeyValuePair<string, string>>();
            PointAttrs = new List<KeyValuePair<string, string>>();
            ValueType = TYPE_DOUBLE;

            foreach (XmlAttribute att in config.Attributes)
            {
                switch (att.Name)
                {
                    case ATTRIBUTE_TITLE:
                        Title = att.Value;
                        break;
                    case ATTRIBUTE_SOURCE:
                        using (TextReader reader = new StreamReader(att.Value))
                        {
                            Source = reader.ReadToEnd();
                        }
                        break;
                    case ATTRIBUTE_RELATED_RULE:
                        RelatedRule = att.Value;
                        break;
                    case ATTRIBUTE_FIELD_NAME:
                        NameField = att.Value;
                        break;
                    case ATTRIBUTE_TYPE_NAME:
                        NameType = att.Value;
                        break;
                    case ATTRIBUTE_FIELD_VALUE:
                        ValueField = att.Value;
                        break;
                    case ATTRIBUTE_TYPE_VALUE:
                        ValueType = att.Value;
                        break;
                    case ATTRIBUTE_NAME:
                        SeriesName = att.Value;
                        break;
                    case ATTRIBUTE_LEGEND:
                        Legend = att.Value;
                        break;
                    default:
                        HtmlAttrs.Add(new KeyValuePair<string, string>(att.Name, att.Value));
                        break;
                }
            }
            foreach (XmlNode node in config.ChildNodes)
            {
                switch (node.Name)
                {
                    case TAG_POINT:
                        PointAttrs.Add(new KeyValuePair<string, string>(node.Attributes["name"].Value, node.Attributes["value"].Value));
                        break;
                }
            }
            if (Source == null)
            {
                Source = config.InnerXml;
            }
        }

        public ExtendChartItem(ExtendChartItem other)
            : base()
        {
            RelatedRule = other.RelatedRule;
            Title = other.Title;
            Legend = other.Legend;
            NameField = other.NameField;
            NameType = other.NameType;
            ValueField = other.ValueField;
            ValueType = other.ValueType;
            SeriesName = other.SeriesName;
            Source = other.Source;
            PointAttrs = other.PointAttrs.ToList();
            HtmlAttrs = other.HtmlAttrs.ToList();
        }

        public override bool CreateContent(Dictionary<string, BaseRule> rules)
        {
            Attachment attachment = GetAttachment(rules);
            Attachments.Add(attachment);

            StringBuilder contentBuilder = new StringBuilder();
            contentBuilder.Append(string.Format("<img src='cid:{0}' ", attachment.Name));
            foreach (KeyValuePair<string, string> attr in HtmlAttrs)
            {
                contentBuilder.Append(" " + attr.Key + "='" + attr.Value + "'");
            }
            contentBuilder.Append("/>");
            Content = contentBuilder.ToString();
            return true;
        }

        protected virtual void LoadTemplate(Chart chart)
        {
            if (!string.IsNullOrWhiteSpace(Source))
            {
                using (TextReader reader = new StringReader(Source)) {
                    chart.Serializer.Load(reader);
                }
            }
        }

        protected virtual Attachment GetAttachment(Dictionary<string, BaseRule> rules)
        {
            BaseRule rule = rules[RelatedRule];

            Chart chart = new Chart();
            SetDefaultAttributes(chart);
            LoadTemplate(chart);

            if (chart.ChartAreas.Count == 0)
                chart.ChartAreas.Add(new ChartArea());

            UpdateTitle(chart.Titles, Title);
            if (Legend != null) chart.Legends.Add(CreateLegend(Legend));

            UpdateSeries(chart.Series, rule, SeriesName);
            //chart.SaveImage(@"d:\result.jpg", ChartImageFormat.Jpeg);

            MemoryStream stream = new MemoryStream();
            chart.SaveImage(stream);
            stream.Position = 0;

            return new Attachment(stream, string.Format("{0}.png", Guid.NewGuid().ToString()));
        }

        protected virtual void SetDefaultAttributes(Chart chart)
        {
            chart.Width = 200;
            chart.Height = 200;
            chart.BackColor = Color.White;
            chart.BorderlineDashStyle = ChartDashStyle.Solid;
            chart.RenderType = RenderType.BinaryStreaming;
            chart.AntiAliasing = AntiAliasingStyles.All;
            chart.TextAntiAliasingQuality = TextAntiAliasingQuality.Normal;
        }

        protected virtual void UpdateTitle(TitleCollection titles, string titleStr)
        {
            if (Title == null)
                return;

            Title title = new Title();
            titles.Add(title);
            title.Text = titleStr;
            title.Font = new Font("Trebuchet MS", 14F, FontStyle.Bold);
            title.ForeColor = Color.Black;
        }

        protected virtual Legend CreateLegend(string legend)
        {
            Legend lg = new Legend();
            lg.BackColor = Color.White;
            lg.ForeColor = Color.Black;
            lg.Name = legend;
            lg.Font = new Font("Trebuchet MS", 10F, FontStyle.Bold);
            lg.BackImageWrapMode = ChartImageWrapMode.Scaled;
            lg.Docking = Docking.Right;
            return lg;
        }

        protected virtual Docking GetlegendDocking()
        {
            Docking docking = Docking.Bottom;

            return docking;
        }

        protected virtual void UpdateSeries(SeriesCollection series, BaseRule rule, String seriesName)
        {
            if ((rule == null) || (seriesName == null))
                return;
            if (series.IsUniqueName(seriesName))
            {
                series.Add(new Series(seriesName));
            }
            Series seriesDetail = series[seriesName];

            List<DataPoint> points = new List<DataPoint>();
            if (rule is GroupRule)
            {
                GroupRule groupRule = (GroupRule)rule;
                foreach (BaseRule subRule in groupRule.Rules)
                {
                    DataPoint point = new DataPoint();
                    point.AxisLabel = subRule.RuleName;
                    point.YValues = new double[] { double.Parse(subRule.Count.ToString()) };
                    points.Add(point);
                }
            }
            else
            {
                foreach (LogItem item in rule.LogItemsInRule)
                {
                    DataPoint point = new DataPoint();
                    //point.SetValueXY(item.GetField(NameField), new object[] {item.GetField(ValueField)});

                    if (!item.HasField(NameField))
                    {
                        Console.WriteLine("Field {0} not found in item {1}.", NameField, item);
                        continue;
                    }
                    string name = item.GetField(NameField).ToString();
                    point.AxisLabel = name;
                    point.XValue = ConvertValue(name, NameType) ?? point.XValue;
                    point.YValues = new double[] { ConvertValue(item.GetField(ValueField).ToString(), ValueType) ?? 0 };
                    points.Add(point);
                }
            }

            points.Sort((x, y) => PointCompare(x, y));

            foreach (DataPoint point in points)
            {
                foreach (KeyValuePair<string, string> pointAttr in PointAttrs)
                {
                    point[pointAttr.Key] = pointAttr.Value;
                }

                seriesDetail.Points.Add(point);
            }
        }

        protected virtual int PointCompare(DataPoint x, DataPoint y) {
            return CompareX(x, y);
        }

        int CompareX(DataPoint x, DataPoint y)
        {
            return x.XValue.CompareTo(y.XValue);
        }

        int CompareY(DataPoint x, DataPoint y)
        {
            return -x.YValues[0].CompareTo(y.YValues[0]);
        }

        double? ConvertValue(String value, String type)
        {
            try
            {
                switch (type)
                {
                    case TYPE_DATE:
                        return DateTime.Parse(value).ToOADate();
                    case TYPE_INT:
                        return Convert.ToInt32(value);
                    case TYPE_DOUBLE:
                        return double.Parse(value);
                    case TYPE_PERCENT:
                        return Convert.ToDouble(value.Replace("%", "").Trim()) / 100;
                    default:
                        return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
