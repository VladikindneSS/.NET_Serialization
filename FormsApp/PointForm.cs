using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using MessagePack;
using Newtonsoft.Json;
using PointLib;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FormsApp
{
    public partial class PointForm: Form
    {
        private Point[] points = null;
        public PointForm()
        {
            InitializeComponent();
        } 

        private void btnCreate_Click(object sender, EventArgs e)
        {
            points = new Point[5];

            var rnd = new Random();

            for (int i = 0; i < points.Length; i++)
                points[i] = rnd.Next(3) % 2 == 0 ? new Point() : new Point3D();

            listBox.DataSource = points;

        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            if (points == null)
                return;

            Array.Sort(points);

            listBox.DataSource = null;
            listBox.DataSource = points;
        }

        private void btnSerialize_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "SOAP|*.soap|XML|*.xml|JSON|*.json|Binary|*.bin|YAML|*.yaml|Text File|*.txt";

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            using (var fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write))
            {
                switch (Path.GetExtension(dlg.FileName))
                {
                    case ".bin":
                        var bf = new BinaryFormatter();
                        bf.Serialize(fs, points);
                        break;
                    case ".soap":
                        var sf = new SoapFormatter();
                        sf.Serialize(fs, points);
                        break;
                    case ".xml":
                        var xf = new XmlSerializer(typeof(Point[]), new[] { typeof(Point3D) });
                        xf.Serialize(fs, points);
                        break;
                    case ".json":
                        var jf = new JsonSerializer();
                        using (var w = new StreamWriter(fs))
                            jf.Serialize(w, points);
                        break;
                    case ".yaml":
                        var serializer = new SerializerBuilder()
                            .WithTagMapping("!Point", typeof(Point))
                            .WithTagMapping("!Point3D", typeof(Point3D))
                            .Build();
                        using (var w = new StreamWriter(fs))
                        {
                            serializer.Serialize(w, points);
                        }
                        break;
                    case ".txt":
                        var textData = string.Join(Environment.NewLine,
                            points.Select(p => $"{p.GetType().Name},{p.X},{p.Y}{(p is Point3D p3d ? $",{p3d.Z}" : "")}"));
                        var bytes = Encoding.UTF8.GetBytes(textData);
                        fs.Write(bytes, 0, bytes.Length);
                        break;
                }
            }
        }
        private void btnDeserialize_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "SOAP|*.soap|XML|*.xml|JSON|*.json|Binary|*.bin|YAML|*.yaml|Text File|*.txt";

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            using (var fs = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read))
            {
                switch (Path.GetExtension(dlg.FileName))
                {
                    case ".bin":
                        var bf = new BinaryFormatter();
                        points = (Point[])bf.Deserialize(fs);
                        break;
                    case ".soap":
                        var sf = new SoapFormatter();
                        points = (Point[])sf.Deserialize(fs);
                        break;
                    case ".xml":
                        var xf = new XmlSerializer(typeof(Point[]), new[] { typeof(Point3D) });
                        points = (Point[])xf.Deserialize(fs);
                        break;
                    case ".json":
                        var jf = new JsonSerializer();
                        using (var r = new StreamReader(fs))
                            points = (Point[])jf.Deserialize(r, typeof(Point[]));
                        break;
                    case ".yaml":
                        var deserializer = new DeserializerBuilder()
                            .WithTagMapping("!Point", typeof(Point))
                            .WithTagMapping("!Point3D", typeof(Point3D))
                            .Build();
                        using (var r = new StreamReader(fs))
                        {
                            points = deserializer.Deserialize<Point[]>(r);
                        }
                        break;
                    case ".txt":
                        using (var reader = new StreamReader(fs))
                        {
                            var lines = reader.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                            var list = new List<Point>();

                            foreach (var line in lines)
                            {
                                var parts = line.Split(',');
                                if (parts[0] == "Point3D" && parts.Length == 4)
                                {
                                    list.Add(new Point3D(
                                        int.Parse(parts[1]),
                                        int.Parse(parts[2]),
                                        int.Parse(parts[3])));
                                }
                                else if (parts[0] == "Point" && parts.Length == 3)
                                {
                                    list.Add(new Point(
                                        int.Parse(parts[1]),
                                        int.Parse(parts[2])));
                                }
                            }
                            points = list.ToArray();
                        }
                        break;
                }
            }

            listBox.DataSource = null;
            listBox.DataSource = points;
        }
    }
}
