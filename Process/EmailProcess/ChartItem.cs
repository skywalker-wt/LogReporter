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
    public class ChartItem : BaseEmailBodyItem
    {
        internal const string ATTRIBUTE_TITLE = "title";
        internal const string ATTRIBUTE_RELATED_RULE = "rule";
        internal const string ATTRIBUTE_FIELD_NAME = "nameField";
        internal const string ATTRIBUTE_FIELD_VALUE = "valueField";
        internal const string ATTRIBUTE_LEGEND = "legend";
        internal const string ATTRIBUTE_WIDTH = "width";
        internal const string ATTRIBUTE_HEIGHT = "height";
        internal const string ATTRIBUTE_CHART_TYPE = "chartType";
        internal const string ATTRIBUTE_3D = "enable3D";

        internal const string TAG_SERIES = "Series";
        internal const string TAG_POINT = "Point";

        public string RelatedRule { get; set; }
        public SeriesChartType ChartType { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Title { get; set; }
        public string Legend { get; set; }
        public string NameField { get; set; }
        public string ValueField { get; set; }
        public List<KeyValuePair<string, string>> SeriesAttrs { get; set; }
        public List<KeyValuePair<string, string>> PointAttrs { get; set; }
        public bool Enable3D { get; set; }

        public ChartItem(XmlNode config)
        {
            ChartType = SeriesChartType.Column;
            Enable3D = false;
            SeriesAttrs = new List<KeyValuePair<string, string>>();
            PointAttrs = new List<KeyValuePair<string, string>>();

            foreach (XmlAttribute att in config.Attributes)
            {
                switch (att.Name)
                {
                    case ATTRIBUTE_TITLE:
                        Title = att.Value;
                        break;
                    case ATTRIBUTE_RELATED_RULE:
                        RelatedRule = att.Value;
                        break;
                    case ATTRIBUTE_FIELD_NAME:
                        NameField = att.Value;
                        break;
                    case ATTRIBUTE_FIELD_VALUE:
                        ValueField = att.Value;
                        break;
                    case ATTRIBUTE_LEGEND:
                        Legend = att.Value;
                        break;
                    case ATTRIBUTE_WIDTH:
                        Width = int.Parse(att.Value);
                        break;
                    case ATTRIBUTE_HEIGHT:
                        Height = int.Parse(att.Value);
                        break;
                    case ATTRIBUTE_CHART_TYPE:
                        ChartType = (SeriesChartType)Enum.Parse(typeof(SeriesChartType), att.Value);
                        break;
                    case ATTRIBUTE_3D:
                        Enable3D = bool.Parse(att.Value);
                        break;
                }

                foreach (XmlNode node in config.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case TAG_SERIES:
                            SeriesAttrs.Add(new KeyValuePair<string, string>(node.Attributes["name"].Value, node.Attributes["value"].Value));
                            break;
                        case TAG_POINT:
                            PointAttrs.Add(new KeyValuePair<string, string>(node.Attributes["name"].Value, node.Attributes["value"].Value));
                            break;
                    }
                }
            }
        }

        public override bool CreateContent(Dictionary<string, BaseRule> rules)
        {
            Attachment attachment = GetAttachment(rules);
            this.Attachments.Add(attachment);

            Content = string.Format("<img src='cid:{0}'/>", attachment.Name);
            return true;
        }

        protected virtual Attachment GetAttachment(Dictionary<string, BaseRule> rules)
        {
            BaseRule rule = rules[RelatedRule];

            Chart chart = new Chart();

            SetDefaultAttributes(chart);
            chart.ChartAreas.Add(CreateChartArea());

            chart.Width = Width;
            chart.Height = Height;
            if (Title != null) chart.Titles.Add(CreateTitle(Title));
            //if (Legend != null) chart.Legends.Add(CreateLegend(Legend));

            chart.Series.Add(CreateSeries(rule, ChartType));

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

        protected virtual Title CreateTitle(string titleStr)
        {
            Title title = new Title();
            title.Text = titleStr;
            title.Font = new Font("Trebuchet MS", 14F, FontStyle.Bold);
            title.ForeColor = Color.Black;
            return title;
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

            switch (ChartType)
            {
                case SeriesChartType.Column:
                    docking = Docking.Right;
                    break;
            }

            return docking;
        }

        protected virtual ChartArea CreateChartArea()
        {
            ChartArea chartArea = new ChartArea();
            chartArea.Name = "Result Chart";
            chartArea.BackColor = Color.Transparent;
            chartArea.AxisX.IsLabelAutoFit = true;
            chartArea.AxisY.IsLabelAutoFit = true;
            chartArea.Area3DStyle.Enable3D = Enable3D;

            return chartArea;
        }

        protected virtual Series CreateSeries(BaseRule rule, SeriesChartType sct)
        {
            Series seriesDetail = new Series();

            seriesDetail.IsValueShownAsLabel = false;
            seriesDetail.Color = Color.Black;
            seriesDetail.ChartType = sct;
            seriesDetail.Font = new Font("Trebuchet MS", 10F, FontStyle.Bold);
            seriesDetail.SmartLabelStyle.Enabled = true;

            foreach (KeyValuePair<string, string> seriesAttr in SeriesAttrs)
            {
                seriesDetail[seriesAttr.Key] = seriesAttr.Value;
            }

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
                    point.AxisLabel = item.GetField(NameField).ToString();
                    point.YValues = new double[] { double.Parse(item.GetField(ValueField).ToString()) };
                    points.Add(point);
                }
            }

            points.Sort((x, y) => Compare(x, y));
            double sum = points.Sum(p => p.YValues[0]);

            foreach (DataPoint point in points)
            {
                foreach (KeyValuePair<string, string> pointAttr in PointAttrs)
                {
                    point[pointAttr.Key] = pointAttr.Value;
                }

                seriesDetail.Points.Add(point);
            }

            return seriesDetail;
        }

        int Compare(DataPoint x, DataPoint y)
        {
            return -x.YValues[0].CompareTo(y.YValues[0]);
        }
    }
}
